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
				var window = NativeObject as NSWindow;
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
				var host = NativeObject as NSWindow;
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
		public event EventHandler LostFocus;

		protected override void OnShown ()
		{
			//InspectorContext.Attach (this);
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

		public void AddChildWindow(IWindowWrapper borderer)
		{

		}

		public void RecalculateKeyViewLoop()
		{

		}

		public bool ContainsChildWindow(IWindowWrapper debugOverlayWindow)
		{
			return false;
		}

		public void AlignLeft(IWindowWrapper toView, int pixels)
		{
			throw new NotImplementedException();
		}

		public void AlignTop(IWindowWrapper toView, int pixels)
		{
			throw new NotImplementedException();
		}

		public void AlignRight(IWindowWrapper toView, int pixels)
		{
			throw new NotImplementedException();
		}

		public void SetTitle(string v)
		{
			throw new NotImplementedException();
		}

		void IWindowWrapper.Close()
		{
			throw new NotImplementedException();
		}

		public object NativeObject => BackendHost.Backend.Window;

		public bool HasParentWindow => throw new NotImplementedException();
	}
}
