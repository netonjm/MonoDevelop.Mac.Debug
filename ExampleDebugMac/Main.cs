using AppKit;
using CoreGraphics;
using Foundation;
using MonoDevelop.Inspector.Mac;
using ObjCRuntime;

namespace ExampleDebugMac
{
	static class MainClass
	{

        [Export("makeFirstResponder:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public static bool MakeFirstResponder(NSResponder aResponder)
        {
            return true;
        }


        static void Main (string[] args)
		{
			NSApplication.Init();
			NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;

            var xPos = NSScreen.MainScreen.Frame.Width / 2;// NSWidth([[window screen] frame])/ 2 - NSWidth([window frame])/ 2;
            var yPos = NSScreen.MainScreen.Frame.Height / 2; // NSHeight([[window screen] frame])/ 2 - NSHeight([window frame])/ 2;
            var mainWindow = new MacAccInspectorWindow(new CGRect(xPos, yPos, 300, 368), NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false);

            var stackView = new NSStackView() { Orientation = NSUserInterfaceLayoutOrientation.Vertical };
			mainWindow.ContentView = stackView;
			stackView.AddArrangedSubview(new NSTextField { StringValue = "123" });
          
            stackView.AddArrangedSubview(new NSTextField { StringValue = "45" });
			stackView.AddArrangedSubview(new NSTextField { StringValue = "345" });
			var button = new NSButton { Title = "Testing" };
            button.WidthAnchor.ConstraintEqualToConstant(100).Active = true;;

            stackView.AddArrangedSubview(button);

            var hotizontalView = new NSStackView() { Orientation = NSUserInterfaceLayoutOrientation.Horizontal };
            hotizontalView.AddArrangedSubview (new NSTextField() { StringValue = "test" });

            stackView.AddArrangedSubview(hotizontalView);

            button.Activated += (sender, e) => {
				var alert = new NSAlert ();
				alert.MessageText = "You clicked the button!!!";
				alert.InformativeText = "Are you sure!?";
				alert.AddButton ("OK!");
				alert.RunModal ();
			};

            var button2 = new NSButton { Title = "123" };

            stackView.AddArrangedSubview(button2);
            button2.WidthAnchor.ConstraintEqualToConstant(100).Active = true; ;
            button2.HeightAnchor.ConstraintEqualToConstant(100).Active = true; ;

            mainWindow.Title = "Example Debug Xamarin.Mac";

			//mainWindow.MakeKeyWindow();
			mainWindow.MakeKeyAndOrderFront(null);
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
			NSApplication.SharedApplication.Run();
			//mainWindow.Dispose();
		}
	}
}
