using AppKit;
using CoreGraphics;
using MonoDevelop.Mac.Debug;

namespace ExampleDebugMac
{
	static class MainClass
	{
		static void Main (string[] args)
		{
			NSApplication.Init();
			NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;

			var mainWindow = new MacAccInspectorWindow(new CGRect(0, 0, 300, 368), NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false);

			var stackView = new NSStackView() { Orientation = NSUserInterfaceLayoutOrientation.Vertical };
			mainWindow.ContentView = stackView;
			stackView.AddArrangedSubview(new NSTextField { StringValue = "123" });
			stackView.AddArrangedSubview(new NSTextField { StringValue = "45" });
			stackView.AddArrangedSubview(new NSTextField { StringValue = "345" });
			var button = new NSButton { Title = "Testing" };
			stackView.AddArrangedSubview(button);

			button.Activated += (sender, e) => {
				var alert = new NSAlert ();
				alert.MessageText = "You clicked the button!!!";
				alert.InformativeText = "Are you sure!?";
				alert.AddButton ("OK!");
				alert.RunModal ();
			};

			stackView.AddArrangedSubview(new NSButton { Title = "123" });
			mainWindow.Title = "Example Debug Xamarin.Mac";

			//mainWindow.MakeKeyWindow();
			mainWindow.MakeKeyAndOrderFront(null);
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
			NSApplication.SharedApplication.Run();
			//mainWindow.Dispose();
		}
	}
}
