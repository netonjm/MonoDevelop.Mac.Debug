using System;
using AppKit;
using MonoDevelop.Mac.Debug;
using Xwt;

namespace DebugExampleDebugWindow
{
	class XwtInspectWindow : Xwt.Window, IWindowWrapper
	{
		public IViewWrapper ContentView {
			get {
				var window = GetWindow();
				if (window != null && window.ContentView != null) {
					return new MacViewWrapper (window.ContentView);
				}
				return null;
			}
			set {
				base.Content = value.Content as Widget;
			}
		}

		public IViewWrapper FirstResponder {
			get {
				var host = base.BackendHost.Backend.Window as NSWindow;
				if (host != null && host.FirstResponder != null) {
					return new MacViewWrapper (host.FirstResponder as NSView);
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

		protected override void OnShown ()
		{
			InspectorContext.Attach (this, false);
			base.OnShown ();
		}

		public override void OnFocusChanged (object focused)
		{
			if (focused is NSView focusedView)
			{
				var wrapperView = new MacViewWrapper(focusedView);
				InspectorContext.ChangeFocusedView(wrapperView);
			} 
		}

		public NSWindow GetWindow ()
		{
			return BackendHost.Backend.Window as NSWindow;
		}
	}
}
