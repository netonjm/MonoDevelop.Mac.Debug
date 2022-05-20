using System;
using AppKit;
using CoreGraphics;
using ObjCRuntime;
using Xamarin.PropertyEditing.Drawing;

namespace VisualStudio.ViewInspector.Extension
{
    public static class PropertyPanelExtensions
    {
		public static CGColor ToCGColor(this CommonColor color)
		 => new CGColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

		public static NSColor ToNSColor(this CommonColor color)
			=> NSColor.FromRgba(color.R / 255, color.G / 255, color.B / 255, color.A / 255);

		public static CommonColor ToCommonColor(this NSColor color)
			=> new CommonColor((byte)(color.RedComponent * 255), (byte)(color.GreenComponent * 255), (byte)(color.BlueComponent * 255), (byte)(color.AlphaComponent * 255), "sRGB");

		public static CommonRectangle ToCommonRectangle(this NSEdgeInsets rect)
		=> new CommonRectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);

		public static NSEdgeInsets ToEdgeInserts(this CommonRectangle rect)
		=> new NSEdgeInsets((nfloat)rect.Y, (nfloat)rect.X, (nfloat)rect.Height, (nfloat)rect.Width);

		public static CommonRectangle ToCommonRectangle(this CGRect rect)
			=> new CommonRectangle(rect.X, rect.Y, rect.Width, rect.Height);

		public static CGRect ToCGRect(this CommonRectangle rect)
			=> new CGRect(rect.X, rect.Y, rect.Width, rect.Height);

		public static CommonSize ToCommonSize(this CGSize size)
			=> new CommonSize(size.Width, size.Height);

		public static CGSize ToCGSize(this CommonSize rect)
			=> new CGSize(rect.Width, rect.Height);

		public static CommonPoint ToCommonSize(this CGPoint size)
			=> new CommonPoint(size.X, size.Y);

		public static CGPoint ToCGSize(this CommonPoint point)
			=> new CGPoint(point.X, point.Y);

		public static NSImage ToNSImage(this CommonImageBrush img)
		{
			return null;
		}

		public static CommonImageBrush ToCommonImageBrush(this NSImage img)
		{
			return null;
		}
	}
}

