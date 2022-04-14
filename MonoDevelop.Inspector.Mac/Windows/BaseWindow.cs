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
	public class BaseWindow : NSWindow, IWindow
	{
		public BaseWindow ()
		{
			Initialize ();
		}

        public bool HasParentWindow => base.ParentWindow != null;

		public BaseWindow (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		public BaseWindow (CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base (contentRect, aStyle, bufferingType, deferCreation)
		{
			Initialize ();
		}

		public BaseWindow (CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation, NSScreen screen) : base (contentRect, aStyle, bufferingType, deferCreation, screen)
		{
			Initialize ();
		}

		protected BaseWindow (NSObjectFlag t) : base (t)
		{
			Initialize ();
		}

		public BaseWindow(NativeHandle handle) : base(handle)
		{
		}

        class WindowDelegate : NSWindowDelegate
        {
			WeakReference<BaseWindow> weakWindow;

			public WindowDelegate(BaseWindow window)
            {
				this.weakWindow = new WeakReference<BaseWindow>(window);
            }

            public override void DidResize(NSNotification notification)
            {
				if (weakWindow.TryGetTarget(out var target))
                {
					target.ResizeRequested?.Invoke(this, EventArgs.Empty);
					return;
				}
                base.DidResize(notification);
            }

            public override void DidMove(NSNotification notification)
            {
				if (weakWindow.TryGetTarget(out var target))
				{
					target.MovedRequested?.Invoke(this, EventArgs.Empty);
					return;
				}
				base.DidMove(notification);
            }
			
            public override void DidResignKey(NSNotification notification)
            {
				if (weakWindow.TryGetTarget(out var target))
				{
					target.LostFocus?.Invoke(this, EventArgs.Empty);
					return;
				}
				base.DidResignKey(notification);
            }
        }

		void Initialize ()
		{
			Delegate = new WindowDelegate(this);
		}

		public object NativeObject => this;

		public void AddChildWindow (IWindow borderer)
		{
			if (borderer.NativeObject is NSWindow currentWindow)
            {
				if (currentWindow.ParentWindow != null && currentWindow.ParentWindow != this)
                {
					currentWindow.ParentWindow.RemoveChildWindow(currentWindow);
				}
				base.AddChildWindow(currentWindow, NSWindowOrderingMode.Above);
			}
		}

		public bool ContainsChildWindow (IWindow debugOverlayWindow)
		{
			return this.ChildWindows.Contains(debugOverlayWindow.NativeObject as NSWindow);
		}

		IView IWindow.ContentView {
			get {
				if (ContentView is NSView view) {
					return new TreeViewItemView (view);
				}
				return null;
			}
			set {
				ContentView = value.NativeObject as NSView;
			}
		}

		IView IWindow.FirstResponder {
			get {
				if (FirstResponder is NSView view) {
					return new TreeViewItemView (view);
				}
				return null;
			}
		}

		public float FrameX => (float) Frame.X;
		public float FrameY => (float) Frame.Y;
		public float FrameWidth => (float)Frame.Width;
		public float FrameHeight => (float)Frame.Height;

		IWindow IWindow.ParentWindow
		{
			get
			{
				var paren = base.ParentWindow;
				if (paren == null)
				{
					return null;
				}
				return new Services.WindowWrapper(paren);
			}
		}

        public void AlignRight(IWindow toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Frame;
			frame.Location = new CGPoint(toViewWindow.Frame.Right + pixels, toViewWindow.Frame.Bottom - frame.Height);
			SetFrame(frame, true);
		}

		public void AlignLeft(IWindow toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Frame;
			frame.Location = new CGPoint(toViewWindow.Frame.Left - Frame.Width - pixels, toViewWindow.Frame.Bottom - frame.Height);
			SetFrame(frame, true);
		}

		public void AlignTop ( IWindow toView, int pixels)
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