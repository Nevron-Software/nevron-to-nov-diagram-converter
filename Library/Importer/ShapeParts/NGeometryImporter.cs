using System;

using Nevron.Nov.DataStructures;
using Nevron.Nov.Graphics;

namespace Nevron.Nov.Diagram.Converter
{
    internal static class NGeometryImporter
    {
        #region Public Methods

        /// <summary>
        /// Imports the geometry of the specified Nevron shape to the given NOV shape.
        /// </summary>
        /// <param name="novShape"></param>
        /// <param name="nevronModel"></param>
        /// <returns>The geometry of the NOV shape.</returns>
        public static NGeometry Import(NShape novShape, Nevron.Diagram.NModel nevronModel)
        {
            NGeometry novGeometry = novShape.GeometryNoCreate;

            if (nevronModel is Nevron.Diagram.NPathShape nevronPathShape)
            {
                if (novGeometry == null)
                {
                    novGeometry = new NGeometry();
                    novShape.Geometry = novGeometry;
                }
                else if (nevronModel is Nevron.Diagram.NLineShape ||
                    nevronModel is Nevron.Diagram.NStep2Connector ||
                    nevronModel is Nevron.Diagram.NStep3Connector ||
                    nevronModel is Nevron.Diagram.NRoutableConnector)
                {
                    return novGeometry;
                }

                // Import the primitive of the path shape
                ImportPrimitive(novShape, nevronPathShape.Primitive);
            }
            else if (nevronModel is Nevron.Diagram.NPrimitiveModel)
            {
                if (novGeometry == null)
                {
                    novGeometry = new NGeometry();
                    novShape.Geometry = novGeometry;
                }

                ImportPrimitive(novShape, (Nevron.Diagram.NPrimitiveModel)nevronModel);
            }

            return novGeometry;
        }

        #endregion

        #region Implementation

        private static void ImportPrimitive(NShape novShape, Nevron.Diagram.NPrimitiveModel nevronPrimitive)
        {
            if (nevronPrimitive is Nevron.Diagram.NPathPrimitive)
            {
                ImportGeometryCommands(novShape.Geometry, (Nevron.Diagram.NPathPrimitive)nevronPrimitive);
            }
            else if (nevronPrimitive is Nevron.Diagram.NTextPrimitive)
            {
                // This is a text primitive, so no geometry commands should be imported
            }
        }
		/// <summary>
		/// Imports the geometry of the specified Nevron shape to the given NOV shape.
		/// </summary>
		/// <param name="novGeometry"></param>
		/// <param name="nevronPathPrimitive"></param>
		private static void ImportGeometryCommands(NGeometry novGeometry, Nevron.Diagram.NPathPrimitive nevronPathPrimitive)
		{
			if (nevronPathPrimitive == null)
				return;

			// The shape consists of a Nevron path primitive, so create a draw path command
			// Get the graphics path points in scene coordinates
			System.Drawing.PointF[] nevronPoints = nevronPathPrimitive.Path.PathPoints;
			NPoint[] novPoints = NDiagramConverter.ToPoints(nevronPoints);

			NMatrix pageTransform;
			if (NDiagramImporter.IsInLibrary(nevronPathPrimitive))
			{
				// The geometry is in a library, so use the transform to the library item as page transform
				pageTransform = novGeometry.OwnerShape.GetTransformToAncestor(novGeometry.OwnerShape.OwnerLibraryItem);
			}
			else
			{
				// The geometry is in a page, so get use the page transform of its owner page
				pageTransform = novGeometry.OwnerShape.GetPageTransform();
			}

            pageTransform.InvertPoints(novPoints);

            /* This is IDS specific code, which detects and converts rounded rectangles from Nevron Diagram to
             * rectangles with corner rounding
             * 
            if (IsRoundedRectangle(pathPrimitive))
            {
                // This is a rounded rectangle, so create a NOV rectangle with the default corner rounding (15)
                newGeometry.Add(new NDrawRectangle(NGeometry2D.GetBounds(newPoints)));
                newGeometry.CornerRounding = 15;
                return;
            }*/

            // Get shape bounds for relative point coordinates calculation
            NRectangle shapeBounds = novGeometry.OwnerShape.GetWHBox();

            // Convert the graphics path points to relative NOV path points
            bool relative;
            NGraphicsPathPoint[] pathPoints = ToGraphicsPathPoints(shapeBounds,
                novPoints, nevronPathPrimitive.PathPointsTypes, out relative);

            // Create a draw path command
            NDrawPath drawPath;
            if (relative)
            {
                drawPath = new NDrawPath(new NRectangle(0, 0, 1, 1), pathPoints);
                drawPath.Relative = true;
            }
            else
            {
                drawPath = new NDrawPath(NDiagramConverter.ToNRectangle(nevronPathPrimitive.ModelBounds), pathPoints);
                drawPath.Relative = false;
            }

            if (nevronPathPrimitive.PathType == Nevron.Diagram.PathType.OpenFigure)
            {
                // The path represents an open figure, so do not show the fill of the NOV draw path command
                drawPath.ShowFill = false;
            }

            novGeometry.Add(drawPath);
        }
        private static NGraphicsPathPoint[] ToGraphicsPathPoints(NRectangle bounds, NPoint[] newPoints, byte[] types, out bool relative)
        {
            // Graphics Path point types:
            // https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.graphicspath.pathtypes(v=vs.110).aspx#Anchor_1

            double width = bounds.Width;
            double height = bounds.Height;
            relative = true;

            if (width == 0)
            {
                width = 1;
                if (height == 0)
                {
                    height = 1;
                    relative = false;
                }
            }
            else if (height == 0)
            {
                height = 1;
            }

            // Create a list for the NOV graphics path points with some capacity reserve
            // for dummy close figure points, required by NOV graphics paths
            NList<NGraphicsPathPoint> points = new NList<NGraphicsPathPoint>(newPoints.Length + 10);

            NGraphicsPathPoint startPoint = new NGraphicsPathPoint(Double.NaN, Double.NaN, 0);
            for (int i = 0; i < newPoints.Length; i++)
            {
                // Calculate NOV point's X and Y
                double x = (newPoints[i].X - bounds.X) / width;
                double y = (newPoints[i].Y - bounds.Y) / height;

                // NOV Graphics Path point command types have a value with 1 larger than
                // the .NET graphics path point command types
                byte type = (byte)(types[i] + 1);
                NGraphicsPathPoint point;

                if ((type & CloseMask) == CloseMask)
                {
                    // This is a closing figure point
                    type = (byte)((type & NGraphicsPath.CommandMask) | NGraphicsPath.AttributeClose);
                    if (!NMath.EqualsEpsilon(x, startPoint.X) || !NMath.EqualsEpsilon(y, startPoint.Y))
                    {
                        // NOV graphics paths require the closing point of a figure to match the start point,
                        // but in this case it does not match it, so add the point and update the X and Y,
                        // so that a dummy point coinciding with the start point gets added, too
                        point = new NGraphicsPathPoint(x, y, (byte)(type & NGraphicsPath.CommandMask));
                        points.Add(point);

                        // Configure the properties of a line to command
                        x = startPoint.X;
                        y = startPoint.Y;
                        type = NGraphicsPath.CommandLineTo | NGraphicsPath.AttributeClose;
                    }
                }

                point = new NGraphicsPathPoint(x, y, type);
                points.Add(point);

                if ((type & NGraphicsPath.CommandStartFigure) == NGraphicsPath.CommandStartFigure)
                {
                    startPoint = points[i];
                }
            }

            return points.ToArray();
        }

        #endregion

        #region Implementation - IDS Specific

        /// <summary>
        /// Checks whether the given Nevron path primitive represents a rounded rectangle.
        /// </summary>
        /// <param name="pathPrimitive"></param>
        /// <returns></returns>
        private static bool IsRoundedRectangle(Nevron.Diagram.NPathPrimitive pathPrimitive)
        {
            // Rounded rectangles in Nevron Diagram are custom paths with 19 points
            byte[] types = pathPrimitive.PathPointsTypes;
            if (types.Length != RoundedRectPathPointTypes.Length)
                return false;

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] != RoundedRectPathPointTypes[i])
                    return false;
            }

            return true;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Represents the GraphicsPath Close mask.
        /// </summary>
        private const byte CloseMask = 0x80;
        /// <summary>
        /// The point types of a rounded rectangle custom path in Nevron Diagram.
        /// </summary>
        private static readonly byte[] RoundedRectPathPointTypes = new byte[] {
            0, 3, 3, 3,
            1, 3, 3, 3,
            1, 3, 3, 3,
            1, 3, 3, 3,
            3, 3, 131
        };

        #endregion
    }
}