using System;
using AppKit;

namespace MonoDevelop.Mac.Debug
{
	public class ContrastAnalisys
	{
		public NSView View1 { get; set; }
		public NSView View2 { get; set; }
		public NSColor Color1 { get; private set; }
		public NSColor Color2 { get; private set; }
		public NSFont Font { get; private set; }

		public ContrastAnalisys(NSColor color11, NSColor color12, NSFont font)
		{
			Color1 = color11;
			Color2 = color12;
			Font = font;

			Analize();
		}

		public bool IsPassed { get; private set; }
		public string Category { get; private set; }
		public bool	IsLargeText { get; private set; }
		public nfloat Contrast { get; private set; }

		public void Analize ()
		{
			Contrast = ContrastHelper.GetColorContrast(Color1, Color2);
			// Check against AA / AAA

			var fontSize = Font.PointSize;
			bool isBold = ContrastHelper.IsBold(Font);

			var largeTextRegular = ContrastHelper.ConvertWcagPointsToNsFontPoints(18);
			var largeTextBold = ContrastHelper.ConvertWcagPointsToNsFontPoints(14);

			if ((fontSize >= largeTextRegular || (isBold && fontSize >= largeTextBold)) && Contrast >= 3)
			{
				IsPassed = true;
				Category = "AA";
				IsLargeText = true;
			}
			else if (Contrast >= 4.5)
			{
				Category = "AA";
				IsPassed = true;
			}
			else if ((fontSize >= largeTextRegular || (isBold && fontSize >= largeTextBold)) && Contrast >= 4.5)
			{
				Category = "AAA";
				IsPassed = true;
				IsLargeText = true;
			}
			else if (Contrast >= 7.0)
			{
				Category = "AAA";
				IsPassed = true;
			}
			else
			{
				Category = "AA";
			}
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}{2}", Category, IsPassed ? "PASSED" : "NOT PASSED", IsLargeText ? " (Large Text)" : "");
		}
	}

	public static class ContrastHelper
	{
		public static string ToHex (this NSColor color)
		{
			var r = ((int)color.RedComponent * 0xFF).ToString("00");
			var g = ((int)color.GreenComponent * 0xFF).ToString("00");
			var b = ((int)color.BlueComponent * 0xFF).ToString("00");
			return string.Format("#{0}{1}{2}", r, g, b);
		}

		static (nfloat R, nfloat G, nfloat B) NormalizeColor (NSColor color)
		{
			return (
			NormalizeColor(color.RedComponent),
			NormalizeColor(color.GreenComponent),
			NormalizeColor(color.BlueComponent)
			);
		}

		static nfloat NormalizeColor (nfloat colorComponent)
		{
			if (colorComponent <= 0.3928) {
				colorComponent = colorComponent / 12.92f;
			} else {
				colorComponent = (float)Math.Pow((colorComponent + 0.055f) / 1.055f, 2.4f);
			}
			return colorComponent;
		}

		public static bool IsBold (NSFont font)
		{
			var symTrails = font.FontDescriptor.SymbolicTraits;
			return symTrails.HasFlag(NSFontSymbolicTraits.BoldTrait);
		}

		public static nfloat ConvertWcagPointsToNsFontPoints(nfloat points)
		{
			// WCAG uses CSS points at 96 ppi, 1pt = ~1.3334px
			// Sketch uses NSFont points at 72 ppi, 1pt = 1px
			return (points * 96f) / 72f;
		}

		public static nfloat GetColorContrast (NSColor color1, NSColor color2)
		{
			var bColor1 = NormalizeColor(color1);
			var bColor2 = NormalizeColor(color2);
			nfloat L1 = 0.2126f * bColor1.R + 0.7152f * bColor1.G + 0.0722f * bColor1.B;
			nfloat L2 = 0.2126f * bColor2.R + 0.7152f * bColor2.G + 0.0722f * bColor2.G;
			if (L1 <= L2)
			{
				var temp = L2;
				L2 = L1;
				L1 = temp;
			}
			var contrast = (L1 + 0.05f) / (L2 + 0.05f);
			return contrast;
		}
	}
}
