using Gdk;
using Gtk;
using MonoDevelop.Inspector.Mac;

public partial class MainWindow : Gtk.Window
{
	GtkBordererWindow win;
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	Widget focusedWidget;

	protected override void OnShown()
	{
		base.OnShown();
		win = new GtkBordererWindow("ewreew");
		win.ParentWindow = this.GdkWindow; 
		ShowAll();
		win.Show();
	}

	protected override void OnSetFocus (Widget focus)
	{
		focusedWidget = focus;
		Refresh ();
		base.OnSetFocus (focus);
	}

	void Refresh ()
	{
		if (focusedWidget != null) {
			win?.Reposition (focusedWidget);
		}
	}

	protected override bool OnConfigureEvent(EventConfigure evnt)
	{
		Refresh ();
		return base.OnConfigureEvent(evnt);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
