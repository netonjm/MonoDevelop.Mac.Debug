using System;
using AppKit;
using Xwt;
using System.Linq;
using CoreGraphics;
using System.Collections.Generic;
using System.Globalization;
using Foundation;

namespace MonoDevelop.Inspector.Mac
{
	public class MacXwtWindowWrapper : Xwt.Window, IWindowWrapper
	{
		public InspectorViewMode ViewMode { get; set; } = InspectorViewMode.Native;

		public IViewWrapper ContentView {
			get {
				if (ViewMode == InspectorViewMode.Native) {
					if (NativeObject is NSWindow window && window.ContentView != null) {
						return new MacViewWrapper (window.ContentView);
					}
				} else {
					return new MacXwtViewWrapper (Content);
				}
				return null;
			}
			set {
				base.Content = value.NativeObject as Widget;
			}
		}

		public IViewWrapper FirstResponder {
			get {
				if (ViewMode == InspectorViewMode.Native) {
					if (NativeObject is NSWindow host && host.FirstResponder != null) {
						return new MacViewWrapper (host.FirstResponder as NSView);
					}
				} else {
					//TODO: NEEDS COMPLETE
				}
				return null;
			}
		}

		protected override void OnBoundsChanged (BoundsChangedEventArgs a)
		{
			MovedRequested?.Invoke (this, EventArgs.Empty);
			base.OnBoundsChanged (a);
		}

		public event EventHandler ResizeRequested;
		public event EventHandler MovedRequested;
		public event EventHandler LostFocus;
		public event EventHandler GotFocus;
		public event EventHandler<CultureInfo> CultureChanged;
		public event EventHandler BecomeMainWindow;

		class WindowDelegate : NSWindowDelegate
		{
			public event EventHandler ResizeRequested;
			public event EventHandler MovedRequested;
			public event EventHandler BecomeMainWindow;
			public event EventHandler BecomeKeyWindow;
			public event EventHandler LostFocus;

			public override void DidResize(NSNotification notification)
			{
				ResizeRequested?.Invoke(this, EventArgs.Empty);
			}
			public override void DidMove(NSNotification notification)
			{
				MovedRequested?.Invoke(this, EventArgs.Empty);
			}

			public override void DidBecomeMain(NSNotification notification)
			{
				BecomeMainWindow?.Invoke(this, EventArgs.Empty);
			}

			public override void DidBecomeKey(NSNotification notification)
			{
				BecomeKeyWindow?.Invoke(this, EventArgs.Empty);
			}

			public override void DidResignKey(NSNotification notification)
			{
				LostFocus?.Invoke(this, EventArgs.Empty);
			}
		}
		protected override void OnShown ()
		{
			var lol = new WindowDelegate();
			Window.Delegate = lol;

			lol.ResizeRequested += (s, e) => {
				ResizeRequested?.Invoke (this, EventArgs.Empty);
			};

			lol.MovedRequested += (s, e) => {
				MovedRequested?.Invoke (this, EventArgs.Empty);
			};

			lol.LostFocus += (s, e) => {
				LostFocus?.Invoke (this, EventArgs.Empty);
			};

			lol.BecomeMainWindow += (s, e) => {
				OnBecomeMainWindow (this, EventArgs.Empty);
			};

			lol.BecomeKeyWindow += (sender, e) => {
				GotFocus?.Invoke(this, EventArgs.Empty);
			};

			base.OnShown ();
		}

		protected virtual void OnBecomeMainWindow (object sender, EventArgs args)
		{

		}

		public void AddChildWindow (IWindowWrapper borderer)
		{
			Window.AddChildWindow (borderer.NativeObject as NSWindow, NSWindowOrderingMode.Above);
		}

		public void RecalculateKeyViewLoop ()
		{
			Window.RecalculateKeyViewLoop ();
		}

		public bool ContainsChildWindow (IWindowWrapper debugOverlayWindow)
		{
			return Window.ChildWindows.Contains (debugOverlayWindow.NativeObject as NSWindow);
		}

		public void AlignLeft (IWindowWrapper toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Window.Frame;
			frame.Location = new CGPoint (toViewWindow.Frame.Left - Window.Frame.Width - pixels, toViewWindow.Frame.Bottom - frame.Height);
			Window.SetFrame (frame, true);
		}

		public void AlignTop (IWindowWrapper toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Window.Frame;
			frame.Location = new CGPoint (toViewWindow.Frame.Left, toViewWindow.AccessibilityFrame.Y + toViewWindow.Frame.Height + pixels);
			Window.SetFrame (frame, true);
		}

		public void AlignRight (IWindowWrapper toView, int pixels)
		{
			var toViewWindow = toView.NativeObject as NSWindow;
			var frame = Window.Frame;
			frame.Location = new CGPoint (toViewWindow.Frame.Right + pixels, toViewWindow.Frame.Bottom - frame.Height);
			Window.SetFrame (frame, true);
		}

		public void SetTitle (string v)
		{
			Window.Title = v;
		}

		void IWindowWrapper.Close ()
		{
			Window.Close ();
		}

		public void SetContentSize (int toolbarWindowWidth, int toolbarWindowHeight)
		{
			Window.SetContentSize (new CGSize (toolbarWindowWidth, toolbarWindowHeight));
		}

        public void SetAppareance(bool isDark)
        {
            Window.Appearance = NSAppearance.GetAppearance(isDark ? NSAppearance.NameVibrantDark : NSAppearance.NameVibrantLight);
        }

        protected NSWindow Window => NativeObject as NSWindow;

		public object NativeObject => BackendHost.Backend.Window;

		public bool HasParentWindow => Window.ParentWindow != null;

        float IWindowWrapper.FrameX => (float) base.X;

        float IWindowWrapper.FrameY => (float)base.Y;

        float IWindowWrapper.FrameWidth => (float)base.Width;

        float IWindowWrapper.FrameHeight => (float)base.Height;
    }
}
