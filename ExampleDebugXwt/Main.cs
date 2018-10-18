using Xwt;

namespace DebugExampleDebugWindow
{
	static class MainClass
	{
		static void Main (string[] args)
		{
#if GTK
			Application.Initialize(Xwt.ToolkitType.Gtk);
#else
			Application.Initialize(Xwt.ToolkitType.XamMac);
#endif
			var mainWindow = new XwtInspectWindow();
			mainWindow.Width = 1024;
			mainWindow.Height = 768;
			mainWindow.Title = "Example Debug Xwt.Mac";
			var verticalBox = new Xwt.VBox();
			mainWindow.Content = verticalBox;

			var hbox = new Xwt.HBox();
			hbox.PackStart(new Xwt.Label("First"));
			hbox.PackStart(new Xwt.Label("Second"));
			hbox.PackStart(new Xwt.TextEntry() { Text = "Text1" });
			hbox.PackStart(new Xwt.Button() { Label = "Button1" });
			verticalBox.PackStart(hbox);
		
			mainWindow.Show();

			Application.Run();
			mainWindow.Dispose();
		}
	}
}
