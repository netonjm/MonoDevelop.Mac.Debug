﻿// This file has been autogenerated from a class added in the UI designer.

using AppKit;
using CoreGraphics;

namespace MonoDevelop.Inspector.Mac
{
	internal class VerticalSeparator : NSView
	{
		public VerticalSeparator ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			SetContentCompressionResistancePriority(850, NSLayoutConstraintOrientation.Horizontal);
			SetContentHuggingPriorityForOrientation(850, NSLayoutConstraintOrientation.Horizontal);
		}

		CGSize size = new CGSize (10, 17);

		public override CGSize IntrinsicContentSize => size;

		public override void DrawRect (CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);

			var line = new NSBezierPath ();
			line.MoveTo (new CGPoint (Frame.Width / 2, 0));
			line.LineTo (new CGPoint (Frame.Width / 2, Frame.Height));
			line.LineWidth = 1;
			NSColor.Gray.Set ();
			line.Stroke ();
		}
	}
}
