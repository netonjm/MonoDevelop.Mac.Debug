﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using AppKit;
using CoreGraphics;
using Foundation;
using System.Linq;
using System.Globalization;
using System.Threading;
using ObjCRuntime;

namespace MonoDevelop.Inspector.Mac
{
	public class MacWindowWrapper : NSWindow, IWindowWrapper
	{
		public MacWindowWrapper ()
		{
			Initialize ();
		}

		public bool HasParentWindow => base.ParentWindow != null;

		public MacWindowWrapper (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		public MacWindowWrapper (CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base (contentRect, aStyle, bufferingType, deferCreation)
		{
			Initialize ();
		}

		public MacWindowWrapper (CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, NSScreen screen) : base (contentRect, aStyle, bufferingType, deferCreation, screen)
		{
			Initialize ();
		}

		protected MacWindowWrapper (NSObjectFlag t) : base (t)
		{
			Initialize ();
		}

		public MacWindowWrapper(NativeHandle handle) : base(handle)
		{
		}

		void Initialize ()
		{
			DidResize += (s, e) => {
				ResizeRequested?.Invoke (this, EventArgs.Empty);
			};

			DidMove += (s, e) => {
				MovedRequested?.Invoke (this, EventArgs.Empty);
			};

			DidResignKey += (s, e) => {
				LostFocus?.Invoke (this, EventArgs.Empty);
			};
		}

		public object NativeObject => this;

		public void AddChildWindow (IWindowWrapper borderer)
		{
			base.AddChildWindow(borderer.NativeObject as NSWindow, NSWindowOrderingMode.Above);
		}

		public bool ContainsChildWindow (IWindowWrapper debugOverlayWindow)
		{
			return this.ChildWindows.Contains(debugOverlayWindow.NativeObject as NSWindow);
		}

		IViewWrapper IWindowWrapper.ContentView {
			get {
				if (ContentView is NSView view) {
					return new MacViewWrapper (view);
				}
				return null;
			}
			set {
				ContentView = value.NativeObject as NSView;
			}
		}

		IViewWrapper IWindowWrapper.FirstResponder {
			get {
				if (FirstResponder is NSView view) {
					return new MacViewWrapper (view);
				}
				return null;
			}
		}

		public float FrameX => (float) Frame.X;
		public float FrameY => (float) Frame.Y;
		public float FrameWidth => (float)Frame.Width;
		public float FrameHeight => (float)Frame.Height;

		public void AlignRight(IWindowWrapper toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Frame;
			frame.Location = new CGPoint(toViewWindow.Frame.Right + pixels, toViewWindow.Frame.Bottom - frame.Height);
			SetFrame(frame, true);
		}

		public void AlignLeft(IWindowWrapper toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Frame;
			frame.Location = new CGPoint(toViewWindow.Frame.Left - Frame.Width - pixels, toViewWindow.Frame.Bottom - frame.Height);
			SetFrame(frame, true);
		}

		public void AlignTop ( IWindowWrapper toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Frame;
			frame.Location = new CGPoint(toViewWindow.Frame.Left, toViewWindow.AccessibilityFrame.Y + toViewWindow.Frame.Height + pixels);
			SetFrame(frame, true);
		}

		public void SetTitle(string v)
		{
			Title = v;
		}

		public void SetContentSize(int toolbarWindowWidth, int toolbarWindowHeight)
		{
			base.SetContentSize(new CGSize(toolbarWindowWidth, toolbarWindowHeight));
		}

        public void SetAppareance(bool isDark)
        {
            base.Appearance = NSAppearance.GetAppearance(isDark ? NSAppearance.NameVibrantDark : NSAppearance.NameVibrantLight);
        }

        public event EventHandler ResizeRequested;
		public event EventHandler MovedRequested;
		public event EventHandler LostFocus;
	}
}
