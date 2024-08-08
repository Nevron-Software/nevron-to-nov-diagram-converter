namespace Nevron.Nov.Diagram.Converter
{
	internal sealed class NArrowheadStyleImporter : NStyleImporter
	{
		public static NArrowhead ToArrowhead(Nevron.Diagram.NArrowheadStyle nevronArrowhead)
		{
			if (nevronArrowhead == null)
				return null;

			NArrowhead novArrowhead = new NArrowhead();

			bool flip;
			double widthMultiplier;
			novArrowhead.Shape = ToArrowheadShape(nevronArrowhead.Shape, out flip, out widthMultiplier);
			novArrowhead.FlipAngle = flip;

			novArrowhead.Fill = NFillStyleImporter.ToFill(nevronArrowhead.FillStyle);
			novArrowhead.Stroke = NStrokeStyleImporter.ToStroke(nevronArrowhead.StrokeStyle);

			novArrowhead.Width = ToDips(nevronArrowhead.Size.Width) * widthMultiplier;
			novArrowhead.Height = ToDips(nevronArrowhead.Size.Height);

			return novArrowhead;
		}

		private static ENArrowheadShape ToArrowheadShape(Nevron.Diagram.ArrowheadShape arrowheadShape, out bool flip, out double widthMultiplier)
		{
			flip = false;
			widthMultiplier = 2;

			switch (arrowheadShape)
			{
				case Nevron.Diagram.ArrowheadShape.None:
					return ENArrowheadShape.None;
				case Nevron.Diagram.ArrowheadShape.Custom:
					return ENArrowheadShape.None;
				case Nevron.Diagram.ArrowheadShape.Arrow:
					return ENArrowheadShape.Triangle;
				case Nevron.Diagram.ArrowheadShape.Circle:
					return ENArrowheadShape.Circle;
				case Nevron.Diagram.ArrowheadShape.ClosedFork:
					// No equivalent in NOV, so use the closest arrowhead available in NOV
					flip = true;
					return ENArrowheadShape.TriangleNoFill;
				case Nevron.Diagram.ArrowheadShape.DoubleArrow:
					widthMultiplier = 1;
					return ENArrowheadShape.DoubleTriangle;
				case Nevron.Diagram.ArrowheadShape.Fork:
					flip = true;
					return ENArrowheadShape.TriangleNoFill;
				case Nevron.Diagram.ArrowheadShape.Losangle:
					return ENArrowheadShape.DiamondNoFill;
				case Nevron.Diagram.ArrowheadShape.Many:
					return ENArrowheadShape.InvertedLineArrow;
				case Nevron.Diagram.ArrowheadShape.ManyOptional:
					widthMultiplier = 1;
					return ENArrowheadShape.InvertedLineArrowWithCircleNoFill;
				case Nevron.Diagram.ArrowheadShape.One:
					return ENArrowheadShape.VerticalLine;
				case Nevron.Diagram.ArrowheadShape.OneOptional:
					flip = true;
					widthMultiplier = 1;
					return ENArrowheadShape.CircleNoFillVerticalLine;
				case Nevron.Diagram.ArrowheadShape.OneOrMany:
					widthMultiplier = 1;
					return ENArrowheadShape.InvertedLineArrowWithVerticalLine;
				case Nevron.Diagram.ArrowheadShape.OpenedArrow:
					return ENArrowheadShape.LineArrow;
				case Nevron.Diagram.ArrowheadShape.QuillArrow:
					// No equivalent in NOV, so use the closest arrowhead available in NOV
					return ENArrowheadShape.TriangleWithInwardCurveNoFill;
				case Nevron.Diagram.ArrowheadShape.SunkenArrow:
					return ENArrowheadShape.TriangleWithInwardCurveNoFill;
				default:
					NDebug.Assert(false, "New Nevron ArrowheadShape?");
					return ENArrowheadShape.None;
			}
		}
	}
}