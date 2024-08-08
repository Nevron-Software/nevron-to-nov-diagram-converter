using System;

using Nevron.Nov.Graphics;
using Nevron.Nov.Text;

namespace Nevron.Nov.Diagram.Converter
{
	internal sealed class NTextStyleImporter : NStyleImporter
	{
		#region Public Methods

		public static void ImportStyle(NTextBlock novTextBlock, GraphicsCore.NTextStyle nevronTextStyle)
		{
			if (novTextBlock == null || nevronTextStyle == null)
				return;

			// Font style
			GraphicsCore.NFontStyle nevronFontStyle = nevronTextStyle.FontStyle;
			if (nevronFontStyle != null)
			{
				novTextBlock.FontName = nevronFontStyle.Name;
				novTextBlock.FontSize = ToPoints(nevronFontStyle.EmSize);
				novTextBlock.FontStyle = (ENFontStyle)(int)nevronFontStyle.Style;
			}

			// Text fill
			if (nevronTextStyle.FillStyle != null)
			{
				novTextBlock.Fill = NFillStyleImporter.ToFill(nevronTextStyle.FillStyle);
			}

			// Text alignment
			GraphicsCore.NStringFormatStyle stringFormat = nevronTextStyle.StringFormatStyle;
			if (stringFormat != null)
			{
				novTextBlock.HorizontalAlignment = ToHorizontalAlignment(stringFormat.HorzAlign);
				novTextBlock.VerticalAlignment = ToVerticalAlignment(stringFormat.VertAlign);
			}

			// Other settings
			novTextBlock.KeepUpward = false;
		}
		public static void ImportPosition(NTextBlock novTextBlock, GraphicsCore.NTextStyle nevronTextStyle)
		{
			if (novTextBlock == null || nevronTextStyle == null)
				return;

            // Text position
            GraphicsCore.NLength offsetX = nevronTextStyle.OffsetX;
            GraphicsCore.NLength offsetY = nevronTextStyle.OffsetY;
            GraphicsCore.NResolution resolution = GetResoltuion(nevronTextStyle.OwnerElement as Nevron.Diagram.NDiagramElement);
			GraphicsCore.NMarginsL padding = nevronTextStyle.BackplaneStyle != null ? nevronTextStyle.BackplaneStyle.Padding : GraphicsCore.NMarginsL.Empty;

            if (offsetX.Value != 0 && novTextBlock.Width != 0)
            {
                double factor = offsetX.MeasurementUnit.ConvertXValueToPixels(new GraphicsCore.NMeasurementUnitConverter(resolution));
                double offset = (offsetX.Value - padding.Left.Value - padding.Right.Value) * factor;
				novTextBlock.LocPinX -= offset / novTextBlock.Width;
            }

			if (offsetY.Value != 0 && novTextBlock.Height != 0)
			{
				double factor = offsetY.MeasurementUnit.ConvertYValueToPixels(new GraphicsCore.NMeasurementUnitConverter(resolution));
				double offset = (offsetY.Value - padding.Top.Value - padding.Bottom.Value) * factor;
				novTextBlock.LocPinY -= offset / novTextBlock.Height;
			}
        }

        #endregion

        #region Implementation

        private static GraphicsCore.NResolution GetResoltuion(Nevron.Diagram.NDiagramElement diagramElement)
		{
			if (diagramElement != null &&
				diagramElement.Document is Nevron.Diagram.NDrawingDocument drawingDocument)
			{
				return new GraphicsCore.NResolution(drawingDocument.Resolution, drawingDocument.Resolution);
			}
			else
			{
				return DefaultResolution;
			}
		}

		#endregion

		#region Conversions

		private static ENAlign ToHorizontalAlignment(HorzAlign horizontalAlignment)
		{
			switch (horizontalAlignment)
			{
				case HorzAlign.Center:
					return ENAlign.Center;
				case HorzAlign.Left:
					return ENAlign.Left;
				case HorzAlign.Right:
					return ENAlign.Right;
				default:
					NDebug.Assert(false, "New HorzAlign?");
					return default(ENAlign);
			}
		}
		private static ENVerticalAlignment ToVerticalAlignment(VertAlign verticalAlignment)
		{
			switch (verticalAlignment)
			{
				case VertAlign.Center:
					return ENVerticalAlignment.Center;
				case VertAlign.Top:
					return ENVerticalAlignment.Top;
				case VertAlign.Bottom:
					return ENVerticalAlignment.Bottom;
				default:
					NDebug.Assert(false, "New VertAlign?");
					return default(ENVerticalAlignment);
			}
		}

		#endregion

		#region Constants

		private static readonly GraphicsCore.NResolution DefaultResolution = new GraphicsCore.NResolution(96, 96);

        #endregion
    }
}