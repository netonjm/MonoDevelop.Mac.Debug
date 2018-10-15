using Gdk;
using Gtk;
using MonoDevelop.Mac.Debug;

public partial class MainWindow : Gtk.Window
{
	GtkBordererWindow win;
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	protected override void OnShown()
	{
		base.OnShown();
		win = new GtkBordererWindow("ewreew");
		ShowAll();
		win.Show();
	}

	int margin = 20;
	protected override bool OnConfigureEvent(EventConfigure evnt)
	{
		if (win != null)
		{
			int x, y;
			GetPosition(out x, out y);
			win.Move(x, y + margin);

			win.WidthRequest = evnt.Width;
			win.HeightRequest = evnt.Height;
			ShowAll();
			win.KeepAbove = true;
		}
		return base.OnConfigureEvent(evnt);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
