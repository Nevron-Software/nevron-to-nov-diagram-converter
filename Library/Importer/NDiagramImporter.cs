using System;
using System.IO;

using Nevron.Diagram;
using Nevron.Nov.DataStructures;
using Nevron.Nov.Diagram.Expressions;
using Nevron.Nov.Diagram.Shapes;
using Nevron.Nov.Dom;
using Nevron.Nov.Graphics;
using Nevron.Nov.Text;
using Nevron.Nov.Text.Formats;

namespace Nevron.Nov.Diagram.Converter
{
    /// <summary>
    /// Base class for Nevron Drawing and Library importers.
    /// </summary>
    internal abstract class NDiagramImporter
    {
        #region Protected Overridable

        protected virtual void ApplyStyles(NGeometry novGeometry, Nevron.Diagram.NModel nevronModel)
        {
            novGeometry.Fill = NFillStyleImporter.ToFill(nevronModel.ComposeFillStyle());
            novGeometry.Stroke = NStrokeStyleImporter.ToStroke(nevronModel.ComposeStrokeStyle());
            novGeometry.BeginArrowhead = NArrowheadStyleImporter.ToArrowhead(nevronModel.ComposeStartArrowheadStyle());
            novGeometry.EndArrowhead = NArrowheadStyleImporter.ToArrowhead(nevronModel.ComposeEndArrowheadStyle());
        }
        protected virtual void AddShapeToGroup(NGroup novGroup, NShape novShape, Nevron.Dom.INNode nevronNode)
        {
            novGroup.Shapes.Add(novShape);
            m_Map.Add(nevronNode, novShape);
        }

        #endregion

        #region Protected Methods - Initialization

        protected void Initialize()
        {
            m_ConnectorFactory = new NConnectorShapeFactory();
            m_Map = new NMap<Nevron.Dom.INNode, NPageItem>();
        }

        #endregion

        #region Protected Methods - Page Items

        protected NPageItem CreatePageItem(Nevron.Dom.INNode nevronNode)
        {
            if (nevronNode is Nevron.Diagram.NGroup nevronGroup)
                return CreateGroup(nevronGroup);
            else if (nevronNode is Nevron.Diagram.NLineShape nevronLine)
                return CreateLine(nevronLine);
            else if (nevronNode is Nevron.Diagram.NStep2Connector nevronStep2Con)
                return CreateStep2Connector(nevronStep2Con);
            else if (nevronNode is Nevron.Diagram.NStep3Connector nevronStep3Con)
                return CreateStep3Connector(nevronStep3Con);
            else if (nevronNode is Nevron.Diagram.NRoutableConnector nevronRoutableConnector)
                return CreateRoutableConnector(nevronRoutableConnector);
            else if (nevronNode is Nevron.Diagram.NCompositeShape nevronCompositeShape)
                return CreateGroup(nevronCompositeShape);
            else if (nevronNode is Nevron.Diagram.NModel nevronModel)
                return CreateShape(nevronModel);
            else
                return null;
        }
        protected void AddPageItem(Nevron.Dom.INNode nevronNode, NPageItem novPageItem)
        {
            m_Map.Add(nevronNode, novPageItem);
        }
        /// <summary>
        /// Tries to get the NOV page item created from the given Nevron node.
        /// </summary>
        /// <param name="nevronNode"></param>
        /// <param name="novPageItem"></param>
        /// <returns></returns>
        protected bool TryGetNovPageItem(Nevron.Dom.INNode nevronNode, out NPageItem novPageItem)
        {
            if (nevronNode == null)
            {
                novPageItem = null;
                return false;
            }
            else
            {
                return m_Map.TryGet(nevronNode, out novPageItem);
            }
        }
        /// <summary>
        /// Called after the page item is added to a page. Do any necessary post processing
        /// in this method, for example, import shape geometry and styles, connection info, etc.
        /// </summary>
        /// <param name="novPageItem"></param>
        /// <param name="nevronNode"></param>
        protected void PostProcessPageItem(NPageItem novPageItem, Nevron.Dom.INNode nevronNode)
        {
            if (nevronNode is Nevron.Diagram.NModel == false)
                return;

            NShape novShape = (NShape)novPageItem;
            Nevron.Diagram.NModel nevronModel = (Nevron.Diagram.NModel)nevronNode;

            EvaluateNovOwnerDocument(novShape);

            // Import shape transform
            ImportTransform(novShape, nevronModel);
            EvaluateNovOwnerDocument(novShape);

            if (novShape.ShapeType == ENShapeType.Shape1D)
            {
                Configure1DShapeSize(novShape, nevronModel);
            }

            // Import geometry
            NGeometry novGeometry = NGeometryImporter.Import(novShape, nevronModel);
            EvaluateNovOwnerDocument(novShape);

            if (nevronModel is Nevron.Diagram.NShape nevronShape &&
                nevronShape.ShapeType == Nevron.Diagram.ShapeType.Shape2D)
            {
                // Create ports
                NPortCollection ports = NPortsImporter.ToPorts(nevronShape.Ports);
                if (ports != null)
                {
                    novShape.Ports = ports;
                }
            }

            if (novGeometry != null)
            {
                // Import fill and stroke style
                ApplyStyles(novGeometry, nevronModel);
            }

            // Import text position
            if (novShape.TextBlockNoCreate is NTextBlock novTextBlock)
            {
                GraphicsCore.NTextStyle nevronTextStyle = nevronModel.ComposeTextStyle();
                NTextStyleImporter.ImportPosition(novTextBlock, nevronTextStyle);
            }

            Nevron.Dom.NNodeList nevronChildNodes;
            if (nevronNode is Nevron.Diagram.NGroup nevronGroup)
            {
                // Post processs group shapes
                nevronChildNodes = nevronGroup.Shapes.Children(null);
            }
            else if (nevronNode is Nevron.Diagram.NCompositeShape nevronCompositeShape)
            {
                // Post process primitive shapes
                nevronChildNodes = nevronCompositeShape.Primitives.Children(null);
            }
            else
            {
                return;
            }

            for (int i = 0; i < nevronChildNodes.Count; i++)
            {
                Nevron.Dom.INNode nevronChildNode = nevronChildNodes[i];

                NPageItem newChildPageItem;
                if (TryGetNovPageItem(nevronChildNode, out newChildPageItem))
                {
                    PostProcessPageItem(newChildPageItem, nevronChildNode);
                }
            }
        }

        #endregion

        #region Protected Methods - Connectors

        protected void Connect(NShape novConnector, Nevron.Diagram.NShape nevronConnector)
        {
            NPageItem fromPageItem, toPageItem;
            if (!TryGetNovPageItem(nevronConnector.FromShape, out fromPageItem) ||
                !TryGetNovPageItem(nevronConnector.ToShape, out toPageItem))
                return;

            NShape novFromShape = (NShape)fromPageItem;
            NShape novToShape = (NShape)toPageItem;

            // Determine the ports the connector is connected to
            Nevron.Diagram.NPort nevronFromPort = nevronConnector.StartPlug.InwardPort;
            NPort newFromPort = novFromShape.Ports.GetPortByName(nevronFromPort.Name);

            Nevron.Diagram.NPort nevronToPort = nevronConnector.EndPlug.InwardPort;
            NPort newToPort = novToShape.Ports.GetPortByName(nevronToPort.Name);

            // Try glue the new connector
            if (newFromPort != null)
            {
                novConnector.GlueBeginToPort(newFromPort);
            }
            else
            {
                novConnector.GlueBeginToShape(novFromShape);
            }

            if (newToPort != null)
            {
                novConnector.GlueEndToPort(newToPort);
            }
            else
            {
                novConnector.GlueEndToShape(novToShape);
            }
        }

        #endregion

        #region Implementation - Groups and Shapes

        private NGroup CreateGroup(Nevron.Diagram.NGroup nevronGroup)
        {
            NGroup novGroup = new NGroup();
            ImportShapeTextAndProtections(novGroup, nevronGroup);

            // Import the Nevron group shapes
            Nevron.Dom.NNodeList nevronShapes = nevronGroup.Shapes.Children(null);
            if (nevronShapes != null)
            {
                for (int i = 0; i < nevronShapes.Count; i++)
                {
                    Nevron.Dom.INNode nevronNode = nevronShapes[i];
                    NPageItem novPageItem = CreatePageItem(nevronNode);

                    if (novPageItem is NShape novShape)
                    {
                        novGroup.Shapes.Add(novShape);
                        m_Map.Add(nevronNode, novShape);
                    }
                }
            }

            return novGroup;
        }
        private NGroup CreateGroup(Nevron.Diagram.NCompositeShape nevronCompositeShape)
        {
            NGroup novGroup = new NGroup();
            ImportShapeTextAndProtections(novGroup, nevronCompositeShape);

            // Import the Nevron group shapes
            Nevron.Dom.NNodeList primitives = nevronCompositeShape.Primitives.Children(null);
            if (primitives != null)
            {
                for (int i = 0; i < primitives.Count; i++)
                {
                    Nevron.Dom.INNode nevronNode = primitives[i];
                    NPageItem novPageItem = CreatePageItem(nevronNode);

                    if (novPageItem is NShape novShape)
                    {
                        AddShapeToGroup(novGroup, novShape, nevronNode);
                    }
                }
            }

            return novGroup;
        }
        private NShape CreateShape(Nevron.Diagram.NModel nevronModel)
        {
            NShape novShape = new NShape();
            Nevron.Diagram.NShape nevronShape = nevronModel as Nevron.Diagram.NShape;
            Nevron.Diagram.NPrimitiveModel nevronPrimitiveModel = nevronModel as Nevron.Diagram.NPrimitiveModel;

            if ((nevronShape != null && nevronShape.ShapeType == Nevron.Diagram.ShapeType.Shape1D) ||
                (nevronPrimitiveModel != null && nevronPrimitiveModel.Is1DPrimitive))
            {
                novShape.Init1DShape(EN1DShapeXForm.Vector);
            }
            else
            {
                novShape.Init2DShape();
            }

            ImportShapeTextAndProtections(novShape, nevronModel);

            return novShape;
        }
        private void ImportShapeTextAndProtections(NShape novShape, Nevron.Diagram.NModel nevronModel)
        {
            // Import text
            ImportText(novShape, nevronModel);

            // Import interactivity style
            NInteractivityStyleImporter.Import(novShape, Nevron.Diagram.NStyle.GetInteractivityStyle(nevronModel));

            // Import protections
            NProtectionsImporter.ImportProtections(novShape, nevronModel.Protection);
        }

        #endregion

        #region Implementation - Connectors

        private NShape CreateLine(Nevron.Diagram.NLineShape nevronLine)
        {
            NShape novLine = m_ConnectorFactory.CreateShape(ENConnectorShape.Line);
			ImportShapeTextAndProtections(novLine, nevronLine);

            return novLine;
        }
        private NShape CreateStep2Connector(Nevron.Diagram.NStep2Connector nevronConnector)
        {
            NShape novConnector;
            if (nevronConnector.FirstVertical)
            {
                novConnector = m_ConnectorFactory.CreateShape(ENConnectorShape.TopBottomToSide);
            }
            else
            {
                novConnector = m_ConnectorFactory.CreateShape(ENConnectorShape.SideToTopBottom);
            }

			ImportShapeTextAndProtections(novConnector, nevronConnector);
			return novConnector;
        }
        private NShape CreateStep3Connector(Nevron.Diagram.NStep3Connector nevronConnector)
        {
            NShape novConnector;
            if (nevronConnector.FirstVertical)
            {
                novConnector = m_ConnectorFactory.CreateShape(ENConnectorShape.BottomToTop1);

                if (nevronConnector.UseMiddleControlPointPercent)
                {
                    novConnector.Controls[0].SetFx(NControl.YProperty,
                        new NShapeWidthFactorFx(nevronConnector.MiddleControlPointPercent / 100.0));
                }
                else
                {
                }
            }
            else
            {
                novConnector = m_ConnectorFactory.CreateShape(ENConnectorShape.SideToSide1);

                if (nevronConnector.UseMiddleControlPointPercent)
                {
                    novConnector.Controls[0].SetFx(NControl.XProperty,
                        new NShapeWidthFactorFx(nevronConnector.MiddleControlPointPercent / 100.0));
                }
                else
                {
                }
            }

			ImportShapeTextAndProtections(novConnector, nevronConnector);
			return novConnector;
        }
        private NRoutableConnector CreateRoutableConnector(Nevron.Diagram.NRoutableConnector nevronConnector)
        {
            NRoutableConnector novConnector = new NRoutableConnector();
            NPoint[] novPoints = NDiagramConverter.ToNPoints(nevronConnector.Points);

            switch (nevronConnector.ConnectorType)
            {
                case Nevron.Diagram.RoutableConnectorType.DynamicHV:
                    novConnector.MakeOrthogonal(novPoints);
                    break;
                case Nevron.Diagram.RoutableConnectorType.DynamicPolyline:
                    novConnector.MakePolyline(novPoints);
                    break;
                case Nevron.Diagram.RoutableConnectorType.DynamicCurve:
                    NDebug.Assert(false, "Dynamic curve routable connectors are not supported in NOV.");
                    break;
                default:
                    NDebug.Assert(false, "New Nevron RoutableConnectorType?");
                    break;
            }

			ImportShapeTextAndProtections(novConnector, nevronConnector);
			return novConnector;
        }

		#endregion

		#region Implementation - Transform

		private void SetAngle(NShape novShape, NMatrix novParentPageTransform, Nevron.Diagram.NModel nevronModel)
		{
			// NOTE: The angle is measured as the rotation of a (0,0) (1,0) vector, transformed by a matrix calulated as:
			// nevronSceneTransform / novParentPageTransform
			NMatrix nevronSceneTransform = NDiagramConverter.ToNMatrix(nevronModel.SceneTransform);
			NMatrix novTransform = nevronSceneTransform;
			novTransform.Divide(novParentPageTransform);

			double angle = NGeometry2D.PointsAngle(
				novTransform.TransformPoint(new NPoint(0, 0)),
				novTransform.TransformPoint(new NPoint(1, 0)));

			novShape.SetAngle(new NAngle(angle, NUnit.Radian));
		}

		/// <summary>
		/// Imports the transform of the specified Nevron shape to the given NOV shape.
		/// </summary>
		/// <param name="novShape"></param>
		/// <param name="nevronModel"></param>
		private void ImportTransform(NShape novShape, Nevron.Diagram.NModel nevronModel)
        {
            NMatrix novParentPageTransform = GetNovParentShapePageTransform(novShape, nevronModel);

            if (novShape.ShapeType == ENShapeType.Shape1D)
            {
                // This is a 1D shape, so import begin and end points and be done with the transform
                NPoint beginPoint = novParentPageTransform.InvertPoint(NDiagramConverter.ToNPoint(nevronModel.StartPoint));
                novShape.SetBeginPoint(beginPoint);

                NPoint endPoint = novParentPageTransform.InvertPoint(NDiagramConverter.ToNPoint(nevronModel.EndPoint));
                novShape.SetEndPoint(endPoint);

				if (IsInLibrary(nevronModel) && !(nevronModel is NLineShape))
				{
					SetAngle(novShape, novParentPageTransform, nevronModel);
				}

                return;
            }

            #region Set Width and Height

            bool resizeX = novShape.GetFx(NShape.WidthProperty) == null;
            bool resizeY = novShape.GetFx(NShape.HeightProperty) == null;

            if (resizeX || resizeY)
            {
                // NOTE: Width and Height are measured as the distance of the basis points transformed in scene coordinates.
                // Because in NOV Diagram all shape transformations are not scaling, the measured width and height are correct.
                NPoint[] points = GetNevronShapeBasisPointsInSceneCoordinates(nevronModel);
                double width = NGeometry2D.PointsDistance(points[0], points[1]);
                double height = NGeometry2D.PointsDistance(points[0], points[2]);

                if (resizeX && resizeY)
                {
                    novShape.Resize(width, height);
                }
                else if (resizeX)
                {
                    NDebug.Assert(resizeY == false);
                    novShape.SetWidth(width);
                }
                else
                {
                    NDebug.Assert(resizeY);
                    novShape.SetHeight(height);
                }
            }

            #endregion

            #region Set Angle

            if (novShape.GetFx(NShape.AngleProperty) == null)
            {
				SetAngle(novShape, novParentPageTransform, nevronModel);
            }

            #endregion

            #region Set LocPin

            // NOTE: LocPin is measured relatively to the original 
            GraphicsCore.NPointF modelPin = nevronModel.ModelPinPoint - nevronModel.ModelBounds.Location;

            novShape.LocPinRelative = true;
            if (nevronModel.ModelWidth != 0)
            {
                novShape.LocPinX = modelPin.X / nevronModel.ModelWidth;
            }
            else
            {
                novShape.LocPinX = 0.5d;
            }

            if (nevronModel.ModelHeight != 0)
            {
                novShape.LocPinY = modelPin.Y / nevronModel.ModelHeight;
            }
            else
            {
                novShape.LocPinY = 0.5d;
            }

            #endregion

            #region Set Pin

            if (novShape.GetFx(NShape.PinXProperty) == null && novShape.GetFx(NShape.PinYProperty) == null)
            {
                // NOTE: The Pin defines the offset of the transformation. So it is calculated as the offset of the following transform:
                // nevronSceneTransform / novParentPageTransform
                NPoint pin = NDiagramConverter.ToNPoint(nevronModel.PinPoint);
                pin = novParentPageTransform.InvertPoint(pin);

                // PinX and PinY do not have expressions, so assign local values
                novShape.SetPinPoint(pin);
            }

            #endregion
        }
        /// <summary>
        /// Configures the size of a 1D shape.
        /// </summary>
        /// <param name="novShape"></param>
        /// <param name="nevronModel"></param>
        private void Configure1DShapeSize(NShape novShape, Nevron.Diagram.NModel nevronModel)
        {
            if (novShape.Width == 0)
            {
                novShape.Width = nevronModel.Width;
            }

            if (novShape.Height == 0)
            {
                novShape.Height = nevronModel.Height;
            }
        }
        private NMatrix GetNovParentShapePageTransform(NShape novShape, Nevron.Diagram.NModel nevronModel)
        {
            NMatrix matrix;
            if (novShape.OwnerGroup != null)
            {
                matrix = novShape.OwnerGroup.GetPageTransform();
            }
            else
            {
                matrix = NMatrix.Identity;
            }

            if (IsInLibrary(nevronModel))
            {
                // If the Nevron model is in a composite shape or a group, translate the matrix with the owner shape's location,
                // because the inner shape's pin point expressions will get wrong otherwise.
                Nevron.Diagram.NShape ownerCompositeShapeOrGroup = GetOwnerCompositeShapeOrGroup(nevronModel);
                if (ownerCompositeShapeOrGroup != null)
                {
                    matrix.Translate(ownerCompositeShapeOrGroup.Location.X, ownerCompositeShapeOrGroup.Location.Y);
                }
            }

            return matrix;
        }

        #endregion

        #region Implementation - Text

        private void ImportText(NShape novShape, Nevron.Diagram.NModel nevronModel)
        {
            string text;
            if (nevronModel is Nevron.Diagram.NShape nevronShape)
            {
                text = nevronShape.Text;
            }
            else if (nevronModel is Nevron.Diagram.NTextPrimitive nevronTextPrimitive)
            {
                text = nevronTextPrimitive.Text;
            }
            else
            {
                return;
            }

            ImportText(novShape, nevronModel, text);
        }
        private void ImportText(NShape novShape, Nevron.Diagram.NStyleableElement nevronElement, string text)
        {
            if (String.IsNullOrEmpty(text))
                return;

            GraphicsCore.NTextStyle nevronTextStyle = nevronElement.ComposeTextStyle();

            if (nevronTextStyle != null && nevronTextStyle.TextFormat == GraphicsCore.TextFormat.XML)
            {
                // XML formatted text is used, so convert it to plain text
                // 1. Import as HTML to NOV
                NRichTextDocument richTextDocument;
                byte[] htmlTextData = NEncoding.UTF8.GetBytes(text);
                using (MemoryStream ms = new MemoryStream(htmlTextData))
                {
                    richTextDocument = NTextFormat.Html.LoadFromStream(ms);
                }

                // 2. Save as TXT and set the text to the shape
                using (MemoryStream ms = new MemoryStream())
                {
                    NTextFormat.Txt.SaveToStream(richTextDocument, ms);
                    byte[] plainTextData = ms.ToArray();
                    novShape.Text = NEncoding.UTF8.GetString(plainTextData);
                }
            }
            else
            {
                // Plain text is used, so set shape's text directly
                novShape.Text = text;
            }

            // Import Nevron text style
            NTextStyleImporter.ImportStyle((NTextBlock)novShape.TextBlock, nevronTextStyle);
        }

        #endregion

        #region Implementation - Helpers

        private void EvaluateNovOwnerDocument(NElement element)
        {
            element.OwnerDocument.Evaluate();
        }
        /// <summary>
        /// Gets the basis points of the given Nevron shape in model coordinates.
        /// </summary>
        /// <returns></returns>
        private NPoint[] GetNevronShapeBasisPointsInModelCoordinates(Nevron.Diagram.NModel nevronModel)
        {
            GraphicsCore.NRectangleF modelBounds = nevronModel.ModelBounds;
            return new NPoint[] {
                new NPoint(modelBounds.X, modelBounds.Y),
                new NPoint(modelBounds.Right, modelBounds.Y),
                new NPoint(modelBounds.X, modelBounds.Bottom)
            };
        }
        /// <summary>
        /// Gets the basis points of the given Nevron shape in scene coordinates.
        /// </summary>
        /// <returns></returns>
        private NPoint[] GetNevronShapeBasisPointsInSceneCoordinates(Nevron.Diagram.NModel nevronModel)
        {
            NPoint[] points = GetNevronShapeBasisPointsInModelCoordinates(nevronModel);
            NDiagramConverter.ToNMatrix(nevronModel.SceneTransform).TransformPoints(points);
            return points;
        }

        #endregion

        #region Fields

        private NMap<Nevron.Dom.INNode, NPageItem> m_Map;
        private NConnectorShapeFactory m_ConnectorFactory;

        #endregion

        #region Static Methods

        /// <summary>
        /// Checks whether the given Nevron Diagram element is in a library document.
        /// </summary>
        /// <param name="nevronDiagramElement"></param>
        /// <returns></returns>
        internal static bool IsInLibrary(Nevron.Diagram.NDiagramElement nevronDiagramElement)
        {
            return nevronDiagramElement.Document is Nevron.Diagram.NLibraryDocument;
        }

        /// <summary>
        /// Gets the owner composite shape (if any) of the given Nevron model.
        /// </summary>
        /// <param name="nevronModel"></param>
        /// <returns></returns>
        private static Nevron.Diagram.NShape GetOwnerCompositeShapeOrGroup(Nevron.Diagram.NModel nevronModel)
        {
            Nevron.Dom.INNode nevronNode = nevronModel;
            while (nevronNode != null)
            {
                nevronNode = nevronNode.ParentNode;
                if (nevronNode is Nevron.Diagram.NCompositeShape nevronCompositeShape)
                    return nevronCompositeShape;
                else if (nevronNode is Nevron.Diagram.NGroup nevronGroup)
                    return nevronGroup;
            }

            return null;
        }

        #endregion
    }
}