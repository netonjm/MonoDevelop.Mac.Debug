﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using CoreGraphics;
using AppKit;

namespace MonoDevelop.Mac.Debug
{
	public class BorderedWindow : NSWindow
	{
		NSView ObjContent
		{
			get;
			set;
		}
		readonly NSBox box;

		public NSColor BorderColor {
			get { return box.BorderColor; }
			set {
				box.BorderColor = value;
			}
		}

		public NSColor FillColor {
			get {
				return box.FillColor;
			}
			set {
				BackgroundColor = value;
				box.FillColor = value;
			}
		}

		public float BorderWidth {
			get { return (float)box.BorderWidth; }
			set {
				box.BorderWidth = value;
			}
		}

		public NSBorderType BorderType {
			get { return box.BorderType; }
			set {
				box.BorderType = value;
			}
		}

		public bool Visible { get {
				return !box.Transparent;
			} 
			set {
				box.Transparent = !value;
			}
		}

		public string ContentViewIdentifier => ObjContent?.Identifier ?? "";

		public BorderedWindow (IntPtr handle) : base (handle)
		{

		}

		public BorderedWindow(NSView content, NSColor borderColor, NSBorderType borderType = NSBorderType.LineBorder, float borderWidth = 3) : this(content.Frame, borderColor, NSColor.Clear, borderType, borderWidth)
		{
			ObjContent = content;
			//AlignWith (ObjContent);
		}

		public BorderedWindow (CGRect frame, NSColor borderColor, NSBorderType borderType = NSBorderType.LineBorder, float borderWidth = 3) : this (frame, borderColor, NSColor.Clear, borderType, borderWidth)
		{

		}

		public BorderedWindow (CGRect frame, NSColor borderColor, NSColor fillColor, NSBorderType borderType = NSBorderType.LineBorder, float borderWidth = 3) : base (frame, NSWindowStyle.Borderless, NSBackingStore.Buffered, false)
		{
			IsOpaque = false;
			ShowsToolbarButton = false;
			IgnoresMouseEvents = true;
			box = new NSBox () {
				BoxType = NSBoxType.NSBoxCustom
			};
			ContentView = box;

			FillColor = fillColor;
			BorderWidth = borderWidth;
			BorderColor = borderColor;
			BorderType = borderType;
			Visible = false;
		}

		public void AlignWith (NSView view)
		{
			var frame = view.AccessibilityFrame;
			SetFrame (frame, true);
		}

		public void AlignWindowWithContentView ()
		{
			if (ObjContent != null)
			{
				AlignWith(ObjContent);
			}
		}
	}
}
