using System;
using Gdk;
using GLib;
using Gtk;

namespace MonoDevelop.Mac.Debug
{
	public class GtkBordererWindow : GtkWindowWrapper, IBorderedWindow
	{
		IViewWrapper NativeContent { get; set; }

		public GtkBordererWindow(IntPtr raw) : base(raw)
		{
			Init();
		}

		Xwt.Drawing.Color ForegroundColor = Xwt.Drawing.Colors.Red;

		float IBorderedWindow.BorderWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public GtkBordererWindow (IViewWrapper view, Xwt.Drawing.Color color) :this ("")
		{
			NativeContent = view;
		}

		public GtkBordererWindow(string title) : base(title)
		{
			Init();
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

		public void SetParentWindow(IWindowWrapper selectedWindow)
		{
			base.ParentWindow = selectedWindow.NativeObject as Gdk.Window;
		}

		public void AlignWindowWithContentView()
		{
			if (NativeContent != null)
			{
				AlignWith(NativeContent);
			}
		}

		public void OrderFront()
		{
			KeepAbove = true;
		}
	}

}
