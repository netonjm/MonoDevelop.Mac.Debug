using System;

namespace MonoDevelop.Mac.Injector
{
	partial class MainClass
	{

		public static void Main (string[] args)
		{
			var targetExecutablePath = "/Users/josemedranojimenez/MonoDevelop.Mac.Debug/MacDebugExample.Mac/bin/Debug/DebugExample.exe";
			var appTarget = new TargetAppContext (targetExecutablePath);
			appTarget.Inject (0);
			//appTarget.Run ();
		}
	}
}
