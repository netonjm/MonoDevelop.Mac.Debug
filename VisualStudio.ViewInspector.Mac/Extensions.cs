﻿// This file has been autogenerated from a class added in the UI designer.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CoreGraphics;
using MonoDevelop.Inspector;
using MonoDevelop.Inspector.Mac;
using VisualStudio.ViewInspector;
using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Mac.Views;
using Xamarin.PropertyEditing.Drawing;

namespace AppKit
{
	public static class Extensions
	{
		public static bool IsLabel(this NSTextField nSTextField)
        {
			return !nSTextField.DrawsBackground && !nSTextField.Editable && !nSTextField.Bezeled;
		}

		public static CGColor ToCGColor(this CommonColor color)
			=> new CGColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

		public static NSColor ToNSColor(this CommonColor color)
			=> NSColor.FromRgba(color.R/255, color.G/255, color.B/255, color.A/255);

		public static CommonColor ToCommonColor(this NSColor color)
			=> new CommonColor((byte)(color.RedComponent * 255), (byte)(color.GreenComponent * 255), (byte)(color.BlueComponent * 255), (byte)(color.AlphaComponent * 255), "sRGB");

		public static CommonRectangle ToCommonRectangle(this CGRect rect)
			=>  new CommonRectangle(rect.X, rect.Y, rect.Width, rect.Height);

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

		internal static VerticalSeparator AddVerticalSeparator(this NSStackView stack)
		{
			var separator = new VerticalSeparator();
			stack.AddArrangedSubview(separator);
			return separator;
		}

		public static bool IsBlockedType (this IView customView)
		{
			if (customView is NSTableViewCell)
			{
				return true;
			}
			return false;
		}

		public static PropertyInfo GetProperty(this object obj, string propertyName)
		{
			return obj.GetType().GetProperty(propertyName);
		}

		internal static TreeNode Search (this TreeNode nodeView, INativeObject view)
		{
			if (nodeView.NativeObject != null && nodeView.NativeObject.NativeObject == view.NativeObject) {
				return nodeView;
			}

			if (nodeView.ChildCount == 0) {
				return null;
			}

			for (int i = 0; i < nodeView.ChildCount; i++) {
				var node = (TreeNode) nodeView.GetChild (i);
				var found = Search (node, view);
				if (found != null) {
					return found;
				}
			}
			return null;
		}
		public static void AlignRight (this NSWindow sender, NSWindow toView, int pixels)
		{
			var frame = sender.Frame;
			frame.Location = new CGPoint(toView.Frame.Right + pixels, toView.Frame.Bottom - frame.Height);
			sender.SetFrame(frame, true);
		}

		public static void AlignLeft (this NSWindow sender, NSWindow toView, int pixels)
		{
			var frame = sender.Frame;
			frame.Location = new CGPoint(toView.Frame.Left - sender.Frame.Width - pixels, toView.Frame.Bottom - frame.Height);
			sender.SetFrame(frame, true);
		}

		public static void AlignTop (this NSWindow from, NSWindow toView, int pixels)
		{
			var frame = from.Frame;
			frame.Location = new CGPoint(toView.Frame.Left, toView.AccessibilityFrame.Y + toView.Frame.Height + pixels);
			from.SetFrame(frame, true);
		}

		public static CGRect Add (this CGRect sender, CGRect toAdd)
		{
			return new CGRect (sender.X + toAdd.X, sender.Y + toAdd.Y, sender.Width + toAdd.Width, sender.Height + toAdd.Height);
		}

		public static CGRect Add (this CGRect sender, CGPoint toAdd)
		{
			return new CGRect (sender.X + toAdd.X, sender.Y + toAdd.Y, sender.Width, sender.Height);
		}

		public static CGRect Add (this CGRect sender, CGSize toAdd)
		{
			return new CGRect (sender.X, sender.Y, sender.Width + toAdd.Width, sender.Height + toAdd.Height);
		}
	}
}
