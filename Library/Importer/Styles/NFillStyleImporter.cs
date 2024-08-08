using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Nevron.Nov.Graphics;
using Nevron.Nov.IO;

namespace Nevron.Nov.Diagram.Converter
{
	internal sealed class NFillStyleImporter : NStyleImporter
	{
		#region Public Methods

		public static NFill ToFill(GraphicsCore.NFillStyle nevronFill)
		{
			if (nevronFill == null)
				return null;

			switch (nevronFill.FillStyleType)
			{
				case GraphicsCore.FillStyleType.Color:
					return new NColorFill(ToColor(nevronFill.GetPrimaryColor()));
				case GraphicsCore.FillStyleType.Gradient:
					return ToGradientFill((GraphicsCore.NGradientFillStyle)nevronFill);
				case GraphicsCore.FillStyleType.Image:
					return ToImageFill((GraphicsCore.NImageFillStyle)nevronFill);
				case GraphicsCore.FillStyleType.Hatch:
					return ToHatchFill((GraphicsCore.NHatchFillStyle)nevronFill);
				case GraphicsCore.FillStyleType.AdvancedGradient:
					break;
				default:
					NDebug.Assert(false, "New Nevron FillStyleType?");
					break;
			}

			return null;
		}

		#endregion

		#region Implementation

		private static NStockGradientFill ToGradientFill(GraphicsCore.NGradientFillStyle nevronGradientFill)
		{
			NStockGradientFill novGradient = new NStockGradientFill(
				ToGradientStyle(nevronGradientFill.Style),
				ToGradientVariant(nevronGradientFill.Style, nevronGradientFill.Variant),
				ToColor(nevronGradientFill.BeginColor),
				ToColor(nevronGradientFill.EndColor));
			return novGradient;
		}
		private static NHatchFill ToHatchFill(GraphicsCore.NHatchFillStyle nevronHatchFill)
		{
			NHatchFill novHatchFill = new NHatchFill(
				ToHatchStyle(nevronHatchFill.Style),
				ToColor(nevronHatchFill.ForegroundColor),
				ToColor(nevronHatchFill.BackgroundColor));

			if (nevronHatchFill.TextureMappingStyle != null)
			{
				novHatchFill.TextureMapping = ToTextureMapping(nevronHatchFill.TextureMappingStyle);
			}

			return novHatchFill;
		}
		private static NAdvancedGradientFill ToAdvancedGradient(GraphicsCore.NAdvancedGradientFillStyle nevronAdvancedGradient)
		{
			// FIX: implement
			return null;
		}
		private static NImageFill ToImageFill(GraphicsCore.NImageFillStyle nevronImageFill)
		{
			try
			{
				if (!String.IsNullOrEmpty(nevronImageFill.FileName))
				{
					// Create an image fill that links to an image file
					string fileName = nevronImageFill.FileName;
					if (!NPath.Current.IsPathRooted(fileName))
					{
						fileName = NPath.Current.Combine(Directory.GetCurrentDirectory(), fileName);
					}

					return new NImageFill(NImage.FromFileEmbedded(fileName));
				}
				else if (nevronImageFill.Bitmap != null)
				{
					// Create an image fill with an embedded image
					NImage novImage = NDiagramConverter.ToNImage(nevronImageFill.Bitmap);
					return new NImageFill(novImage);
				}
			}
			catch (Exception ex)
			{
				NDebug.WriteLine("Failed to import an image. Exception was: " + ex.Message);
			}

			return null;
		}

		#endregion

		#region Conversions

		private static ENGradientStyle ToGradientStyle(GraphicsCore.GradientStyle gradientStyle)
		{
			switch (gradientStyle)
			{
				case GraphicsCore.GradientStyle.Horizontal:
					return ENGradientStyle.Horizontal;
				case GraphicsCore.GradientStyle.Vertical:
					return ENGradientStyle.Vertical;
				case GraphicsCore.GradientStyle.DiagonalUp:
					return ENGradientStyle.DiagonalUp;
				case GraphicsCore.GradientStyle.DiagonalDown:
					return ENGradientStyle.DiagonalDown;
				case GraphicsCore.GradientStyle.FromCorner:
					return ENGradientStyle.FromCorner;
				case GraphicsCore.GradientStyle.FromCenter:
					return ENGradientStyle.FromCenter;
				case GraphicsCore.GradientStyle.StartToEnd:
					return ENGradientStyle.Horizontal;
				default:
					NDebug.Assert(false, "New Nevron GradientStyle?");
					return default(ENGradientStyle);
			}
		}
		private static ENGradientVariant ToGradientVariant(GraphicsCore.GradientStyle gradientStyle,
			GraphicsCore.GradientVariant gradientVariant)
		{
			switch (gradientVariant)
			{
				case GraphicsCore.GradientVariant.Variant1:
					return gradientStyle == GraphicsCore.GradientStyle.FromCenter ?
						ENGradientVariant.Variant2 :
						ENGradientVariant.Variant1;
				case GraphicsCore.GradientVariant.Variant2:
					return gradientStyle == GraphicsCore.GradientStyle.FromCenter ?
						ENGradientVariant.Variant1 :
						ENGradientVariant.Variant2;
				case GraphicsCore.GradientVariant.Variant3:
					return ENGradientVariant.Variant3;
				case GraphicsCore.GradientVariant.Variant4:
					return ENGradientVariant.Variant4;
				default:
					NDebug.Assert(false, "New Nevron GradientVariant?");
					return default(ENGradientVariant);
			}
		}

		private static ENHatchStyle ToHatchStyle(System.Drawing.Drawing2D.HatchStyle hatchStyle)
		{
			return (ENHatchStyle)(int)hatchStyle;
		}
		private static NTextureMapping ToTextureMapping(GraphicsCore.NTextureMappingStyle nevronTextureMapping)
		{
			switch (nevronTextureMapping.MapLayout)
			{
				case GraphicsCore.MapLayout.Stretched:
					return new NStretchTextureMapping();
				case GraphicsCore.MapLayout.Fitted:
				case GraphicsCore.MapLayout.CropFitted:
					return new NFitAndAlignTextureMapping();
				case GraphicsCore.MapLayout.Centered:
					return new NAlignTextureMapping(ENHorizontalAlignment.Center, ENVerticalAlignment.Center);
				case GraphicsCore.MapLayout.Tiled:
					return new NTileTextureMapping();
				case GraphicsCore.MapLayout.StretchedToWidth:
					return new NStretchXAlignYTextureMapping();
				case GraphicsCore.MapLayout.StretchedToHeight:
					return new NStretchYAlignXTextureMapping();
				default:
					NDebug.Assert(false, "New Nevron texture mapping MapLayout?");
					return null;
			}
		}

		#endregion
	}
}