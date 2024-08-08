using Nevron.Nov.DataStructures;
using Nevron.Nov.Graphics;

namespace Nevron.Nov.Diagram.Converter
{
	internal sealed class NStrokeStyleImporter : NStyleImporter
	{
		#region Public Methods

		public static NStroke ToStroke(GraphicsCore.NStrokeStyle nevronStroke)
		{
			if (nevronStroke == null)
				return null;

			NStroke novStroke = new NStroke();
			novStroke.Color = ToColor(nevronStroke.Color);
			novStroke.Width = ToDips(nevronStroke.Width);
			novStroke.DashStyle = ToDashStyle(nevronStroke.Pattern);
			if (nevronStroke.Pattern == GraphicsCore.LinePattern.Custom)
			{
				novStroke.DashPattern = ToDashPattern(nevronStroke.CustomPattern, nevronStroke.Factor);
			}

			return novStroke;
		}

		#endregion

		#region Conversions

		private static ENDashStyle ToDashStyle(GraphicsCore.LinePattern linePattern)
		{
			switch (linePattern)
			{
				case GraphicsCore.LinePattern.Solid:
					return ENDashStyle.Solid;
				case GraphicsCore.LinePattern.Dot:
					return ENDashStyle.Dot;
				case GraphicsCore.LinePattern.Dash:
					return ENDashStyle.Dash;
				case GraphicsCore.LinePattern.DashDot:
					return ENDashStyle.DashDot;
				case GraphicsCore.LinePattern.DashDotDot:
					return ENDashStyle.DashDotDot;
				case GraphicsCore.LinePattern.Custom:
					return ENDashStyle.Custom;
				default:
					NDebug.Assert(false, "New Nevron LinePattern?");
					return default(ENDashStyle);
			}
		}
		private static NDashPattern ToDashPattern(int customPattern, int factor)
		{
			if (customPattern == 0)
				return new NDashPattern();

			NList<float> patternArray = new NList<float>();
			while (customPattern != 0)
			{
				int value = (customPattern % 2) * factor;
				patternArray.Add(value);

				customPattern /= 2;
			}

			// Make sure there's an even number of values in the pattern array
			if (patternArray.Count % 2 == 1)
			{
				if (patternArray[patternArray.Count - 1] == 0)
				{
					patternArray.PopBack();
				}
				else
				{
					patternArray.Add(0);
				}
			}

			return new NDashPattern(patternArray.ToArray());
		}

		#endregion
	}
}
