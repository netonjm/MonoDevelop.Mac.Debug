using System;
using System.Globalization;
using System.Threading;
using AppKit;
using CoreGraphics;
using ExampleDebugMac.Resource;
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
            var mainWindow = new MacAccInspectorWindow();



			var stackView = new NSStackView() { Orientation = NSUserInterfaceLayoutOrientation.Vertical };
			mainWindow.ContentView = stackView;
			stackView.AddArrangedSubview(new NSTextField { StringValue = "123" });
          
            stackView.AddArrangedSubview(new NSTextField { StringValue = "45" });
			stackView.AddArrangedSubview(new NSTextField { StringValue = "345" });
			var button = new NSButton { Title =  "Press to show a message" };

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

            var button2 = new NSButton { Title = "Opens Localized text" };
            button2.Activated += (sender, e) => {
                var window = new NSWindow() { StyleMask = NSWindowStyle.Titled | NSWindowStyle.Resizable | NSWindowStyle.Closable };
                var stack = NativeViewHelper.CreateHorizontalStackView();
                var label = NativeViewHelper.CreateLabel("hello");
                stack.AddArrangedSubview(label);
                window.ContentView = stack;
                window.WillClose += (sender1, e1) =>
                {
                    window.Dispose();
                };
                window.MakeKeyAndOrderFront(mainWindow);
            };
            stackView.AddArrangedSubview(button2);
            button2.HeightAnchor.ConstraintEqualTo(100).Active = true; ;

            mainWindow.Title = "Example Debug Xamarin.Mac";

			//mainWindow.MakeKeyWindow();
			mainWindow.MakeKeyAndOrderFront(null);
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
			NSApplication.SharedApplication.Run();
            //mainWindow.Dispose();
          
        }
	}
}
