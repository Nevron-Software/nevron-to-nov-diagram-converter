using Nevron.Nov.DataStructures;
using Nevron.Nov.Graphics;
using Nevron.Nov.UI;

namespace Nevron.Nov.Diagram.Converter
{
    /// <summary>
    /// Base class for all style importers.
    /// </summary>
	internal class NStyleImporter
	{
		#region Constructors

		/// <summary>
		/// Static constructor.
		/// </summary>
		static NStyleImporter()
		{
			double dpi = NScreen.PrimaryScreen.Resolution;
			UnitConverter = new GraphicsCore.NMeasurementUnitConverter((float)dpi, (float)dpi);
		}

		#endregion

		#region Protected Static Methods

		protected static NColor ToColor(GraphicsCore.NColor nevronColor)
		{
			return ToColor(nevronColor.ToColor());
		}
		protected static NColor ToColor(System.Drawing.Color netColor)
		{
			return new NColor((uint)netColor.ToArgb());
		}

		#endregion

		#region Static Methods - Fill and Stroke

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

		#region Static Methods - Lengths

		public static double ToDips(GraphicsCore.NLength length)
		{
			return UnitConverter.ConvertX(length.MeasurementUnit,
				GraphicsCore.NGraphicsUnit.Pixel, length.Value);
		}
		public static double ToPoints(GraphicsCore.NLength length)
		{
			return UnitConverter.ConvertX(length.MeasurementUnit,
				GraphicsCore.NGraphicsUnit.Point, length.Value);
		}

		#endregion

		#region Constants

		private static readonly GraphicsCore.NMeasurementUnitConverter UnitConverter;

		#endregion
	}
}