using AppKit;
using Foundation;
using MonoDevelop.Mac.Debug;
using System.Linq;

namespace DebugExample
{
	[Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate()
		{
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			// Insert code here to initialize your application
			var window = NSApplication.SharedApplication.Windows
				 .OfType<NSWindow> ()
				 .FirstOrDefault ();
			// Do any additional setup after loading the view.
			InspectorContext.Attach (window, true);
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}
