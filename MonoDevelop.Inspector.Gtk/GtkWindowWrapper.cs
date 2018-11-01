using System;
using GLib;
using Gtk;

namespace MonoDevelop.Inspector.Mac
{
	public class GtkWindowWrapper : Gtk.Window, IWindowWrapper
	{
		const int toolbarMargin = 20;

		public GtkWindowWrapper(IntPtr raw) : base(raw)
		{
			Initialize();
		}

		private void Initialize()
		{

		}

		protected override void OnSizeRequested(ref Requisition requisition)
		{
			ResizeRequested?.Invoke(this, EventArgs.Empty);
			base.OnSizeRequested(ref requisition);
		}

		public GtkWindowWrapper(WindowType type) : base(type)
		{
			Initialize(); 
		}

		public GtkWindowWrapper(string title) : base(title)
		{
			Initialize();
		}

		protected GtkWindowWrapper(GType gtype) : base(gtype)
		{
			Initialize();
		}

		public IViewWrapper ContentView { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public IViewWrapper FirstResponder => throw new NotImplementedException();

		public bool HasParentWindow => throw new NotImplementedException();

		public object NativeObject => this;

		public event EventHandler LostFocus;
		public event EventHandler ResizeRequested;
		public event EventHandler MovedRequested;

		public void AddChildWindow(IWindowWrapper borderer)
		{
			throw new NotImplementedException();
		}

		Xwt.Rectangle GetFrame (Window window)
		{
			int wx, wy;
			window.GetPosition(out wx, out wy);
			return new Xwt.Rectangle(wx, wy, window.Allocation.Width, window.Allocation.Height);
		}

		public void AlignLeft(IWindowWrapper toView, int pixels)
		{
			var frame = GetFrame(this);
			var toViewFrame = GetFrame(toView as Window);
			//var frame = Frame;
			//frame.Location = new CGPoint(toViewWindow.Frame.Left - Frame.Width - pixels, toViewWindow.Frame.Bottom - frame.Height);
			//SetFrame(frame, true);
		}

		public void AlignRight(IWindowWrapper toView, int pixels)
		{
			throw new NotImplementedException();
		}

		public void AlignTop(IWindowWrapper toView, int pixels)
		{
			throw new NotImplementedException();
		}

		public void Close()
		{
			base.Destroy();
		}

		//public bool ContainsChildWindow(IWindowWrapper debugOverlayWindow)
		//{

		//}

		public void RecalculateKeyViewLoop()
		{

		}

		public void SetContentSize(int toolbarWindowWidth, int toolbarWindowHeight)
		{
			WidthRequest = toolbarWindowWidth;
			HeightRequest = toolbarWindowHeight;
		}

		public void SetFrame(Xwt.Rectangle rectangle)
		{
			SetFrame(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}

		public void SetFrame(double x, double y, double width, double height)
		{
			//int px, py;
			//window.GetPosition (out px, out py);

			Move((int) x, (int) y);
			WidthRequest = (int)width;
			HeightRequest = (int)height;
			//ShowAll ();
			KeepAbove = true;
		}

		public void AlignWith(IViewWrapper view)
		{
			var item = view.NativeView as Widget;
			var position = GetAccessibilityFrame(item);
			SetFrame(position);
		}

		public Xwt.Rectangle GetAccessibilityFrame (Gtk.Widget widget)
		{
			var window = widget.ParentWindow;
			int wx, wy;
			window.GetPosition(out wx, out wy);

			int ux, uy;
			widget.TranslateCoordinates(widget.Toplevel, 0, 0, out ux, out uy);

			return new Xwt.Rectangle(wx + ux, wy + uy, widget.Allocation.Width, widget.Allocation.Height);
		}

		public void Reposition(Gtk.Widget widget)
		{
			var window = widget.ParentWindow;
			int wx, wy;
			window.GetPosition(out wx, out wy);

			int ux, uy;
			widget.TranslateCoordinates(widget.Toplevel, 0, 0, out ux, out uy);

			SetFrame(wx + ux, wy + uy, widget.Allocation.Width, widget.Allocation.Height);
		}

		public void SetTitle(string v)
		{
			base.Title = v;
		}
	}
}
