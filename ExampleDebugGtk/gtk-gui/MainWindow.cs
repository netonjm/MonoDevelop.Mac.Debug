
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.VBox vbox2;

	private global::Gtk.HBox hbox3;

	private global::Gtk.Button button3;

	private global::Gtk.Button button5;

	private global::Gtk.ToggleButton togglebutton1;

	private global::Gtk.SpinButton spinbutton3;

	private global::Gtk.RadioButton radiobutton1;

	private global::Gtk.HBox hbox5;

	private global::Gtk.Button button7;

	private global::Gtk.ToggleButton togglebutton3;

	private global::Gtk.Alignment alignment1;

	private global::Gtk.Label label5;

	private global::Gtk.HBox hbox7;

	private global::Gtk.ScrolledWindow GtkScrolledWindow;

	private global::Gtk.TextView textview1;

	private global::Gtk.ComboBox combobox1;

	private global::Gtk.Label label3;

	protected virtual void Build()
	{
		global::Stetic.Gui.Initialize(this);
		// Widget MainWindow
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString("MainWindow");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox2 = new global::Gtk.VBox();
		this.vbox2.Name = "vbox2";
		this.vbox2.Spacing = 6;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox3 = new global::Gtk.HBox();
		this.hbox3.Name = "hbox3";
		this.hbox3.Spacing = 6;
		// Container child hbox3.Gtk.Box+BoxChild
		this.button3 = new global::Gtk.Button();
		this.button3.CanFocus = true;
		this.button3.Name = "button3";
		this.button3.UseUnderline = true;
		this.button3.Label = global::Mono.Unix.Catalog.GetString("GtkButton");
		this.hbox3.Add(this.button3);
		global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.button3]));
		w1.Position = 0;
		w1.Expand = false;
		w1.Fill = false;
		// Container child hbox3.Gtk.Box+BoxChild
		this.button5 = new global::Gtk.Button();
		this.button5.CanFocus = true;
		this.button5.Name = "button5";
		this.button5.UseUnderline = true;
		this.button5.Label = global::Mono.Unix.Catalog.GetString("GtkButton");
		this.hbox3.Add(this.button5);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.button5]));
		w2.Position = 1;
		w2.Expand = false;
		w2.Fill = false;
		// Container child hbox3.Gtk.Box+BoxChild
		this.togglebutton1 = new global::Gtk.ToggleButton();
		this.togglebutton1.CanFocus = true;
		this.togglebutton1.Name = "togglebutton1";
		this.togglebutton1.UseUnderline = true;
		this.togglebutton1.Label = global::Mono.Unix.Catalog.GetString("GtkToggleButton");
		this.hbox3.Add(this.togglebutton1);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.togglebutton1]));
		w3.Position = 2;
		w3.Expand = false;
		w3.Fill = false;
		// Container child hbox3.Gtk.Box+BoxChild
		this.spinbutton3 = new global::Gtk.SpinButton(0D, 100D, 1D);
		this.spinbutton3.CanFocus = true;
		this.spinbutton3.Name = "spinbutton3";
		this.spinbutton3.Adjustment.PageIncrement = 10D;
		this.spinbutton3.ClimbRate = 1D;
		this.spinbutton3.Numeric = true;
		this.hbox3.Add(this.spinbutton3);
		global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.spinbutton3]));
		w4.Position = 3;
		w4.Expand = false;
		w4.Fill = false;
		// Container child hbox3.Gtk.Box+BoxChild
		this.radiobutton1 = new global::Gtk.RadioButton(global::Mono.Unix.Catalog.GetString("radiobutton1"));
		this.radiobutton1.CanFocus = true;
		this.radiobutton1.Name = "radiobutton1";
		this.radiobutton1.DrawIndicator = true;
		this.radiobutton1.UseUnderline = true;
		this.radiobutton1.Group = new global::GLib.SList(global::System.IntPtr.Zero);
		this.hbox3.Add(this.radiobutton1);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.radiobutton1]));
		w5.Position = 4;
		this.vbox2.Add(this.hbox3);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox3]));
		w6.Position = 0;
		w6.Expand = false;
		w6.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox5 = new global::Gtk.HBox();
		this.hbox5.Name = "hbox5";
		this.hbox5.Spacing = 6;
		// Container child hbox5.Gtk.Box+BoxChild
		this.button7 = new global::Gtk.Button();
		this.button7.CanFocus = true;
		this.button7.Name = "button7";
		this.button7.UseUnderline = true;
		this.button7.Label = global::Mono.Unix.Catalog.GetString("GtkButton");
		this.hbox5.Add(this.button7);
		global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.button7]));
		w7.Position = 0;
		w7.Expand = false;
		w7.Fill = false;
		// Container child hbox5.Gtk.Box+BoxChild
		this.togglebutton3 = new global::Gtk.ToggleButton();
		this.togglebutton3.CanFocus = true;
		this.togglebutton3.Name = "togglebutton3";
		this.togglebutton3.UseUnderline = true;
		this.togglebutton3.Label = global::Mono.Unix.Catalog.GetString("GtkToggleButton");
		this.hbox5.Add(this.togglebutton3);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.togglebutton3]));
		w8.Position = 1;
		w8.Expand = false;
		w8.Fill = false;
		// Container child hbox5.Gtk.Box+BoxChild
		this.alignment1 = new global::Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
		this.alignment1.Name = "alignment1";
		// Container child alignment1.Gtk.Container+ContainerChild
		this.label5 = new global::Gtk.Label();
		this.label5.Name = "label5";
		this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("label5");
		this.alignment1.Add(this.label5);
		this.hbox5.Add(this.alignment1);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.alignment1]));
		w10.Position = 2;
		w10.Expand = false;
		w10.Fill = false;
		this.vbox2.Add(this.hbox5);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox5]));
		w11.Position = 1;
		w11.Expand = false;
		w11.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox7 = new global::Gtk.HBox();
		this.hbox7.Name = "hbox7";
		this.hbox7.Spacing = 6;
		// Container child hbox7.Gtk.Box+BoxChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.textview1 = new global::Gtk.TextView();
		this.textview1.CanFocus = true;
		this.textview1.Name = "textview1";
		this.GtkScrolledWindow.Add(this.textview1);
		this.hbox7.Add(this.GtkScrolledWindow);
		global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.GtkScrolledWindow]));
		w13.Position = 0;
		// Container child hbox7.Gtk.Box+BoxChild
		this.combobox1 = global::Gtk.ComboBox.NewText();
		this.combobox1.Name = "combobox1";
		this.hbox7.Add(this.combobox1);
		global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.combobox1]));
		w14.Position = 1;
		w14.Expand = false;
		w14.Fill = false;
		// Container child hbox7.Gtk.Box+BoxChild
		this.label3 = new global::Gtk.Label();
		this.label3.Name = "label3";
		this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("label3");
		this.hbox7.Add(this.label3);
		global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.label3]));
		w15.Position = 2;
		w15.Expand = false;
		w15.Fill = false;
		this.vbox2.Add(this.hbox7);
		global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox7]));
		w16.Position = 2;
		w16.Expand = false;
		w16.Fill = false;
		this.Add(this.vbox2);
		if ((this.Child != null))
		{
			this.Child.ShowAll();
		}
		this.DefaultWidth = 427;
		this.DefaultHeight = 300;
		this.Show();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler(this.OnDeleteEvent);
	}
}