using System;
using Gdk;
using GLib;
using Gtk;

namespace MonoDevelop.Mac.Debug
{
	public class GtkBordererWindow : Gtk.Window
	{
		public GtkBordererWindow(IntPtr raw) : base(raw)
		{
			Init();
		}

		Xwt.Drawing.Color ForegroundColor = Xwt.Drawing.Colors.Red;

		public GtkBordererWindow(string title) : base(title)
		{
			Init();
		}

		const int toolbarMargin = 20;

		public void Reposition (int x, int y, int width, int height)
		{
			//int px, py;
			//window.GetPosition (out px, out py);

			Move (x, y);
			WidthRequest = width;
			HeightRequest = height;
			//ShowAll ();
			KeepAbove = true;
		}

		public void Reposition (Gtk.Window window, int width, int height)
		{
			int px, py;
			window.GetPosition (out px, out py);
			Reposition (px, py + toolbarMargin, width, height);
		}

		public void Reposition (Gtk.Widget widget)
		{
			var window = widget.ParentWindow;
			int wx, wy;
			window.GetPosition (out wx, out wy);

			int ux, uy;
			widget.TranslateCoordinates (widget.Toplevel, 0, 0, out ux, out uy);

			Reposition (wx + ux,wy + uy, widget.Allocation.Width, widget.Allocation.Height);
		}

		protected GtkBordererWindow(GType gtype) : base(gtype)
		{
			Init();
		}

		protected override void OnSizeRequested(ref Requisition requisition)
		{
			QueueDraw();
			base.OnSizeRequested(ref requisition);
		}

		protected override void OnScreenChanged(Screen previous_screen)
		{

			base.OnScreenChanged(previous_screen);
		}

		protected override bool OnExposeEvent(EventExpose evnt)
		{
			//Decorated = false;
			Cairo.Context cr = Gdk.CairoHelper.Create(this.GdkWindow);

			//clean window
			cr.SetSourceRGBA(ForegroundColor.Red, ForegroundColor.Green, ForegroundColor.Blue, 0);
			cr.Operator = Cairo.Operator.Source;
			cr.Paint();

			cr.SetSourceRGBA(ForegroundColor.Red, ForegroundColor.Green, ForegroundColor.Blue, 1);

			cr.LineWidth = 5;
			cr.MoveTo(0, 0);
			cr.LineTo(WidthRequest, 0);
			cr.MoveTo(WidthRequest, 0);
			cr.LineTo(WidthRequest, HeightRequest);
			cr.MoveTo(WidthRequest, HeightRequest);
			cr.LineTo(0, HeightRequest);
			cr.MoveTo(0, HeightRequest);
			cr.LineTo(0, 0);
			cr.Stroke();
			cr.Dispose();
			return false;
		}

		private void Init()
		{
			BorderWidth = 0;
			Decorated = false;
			this.CanFocus = false;
			AppPaintable = true;
			Colormap = Screen.RgbaColormap;
		}
	}

}
