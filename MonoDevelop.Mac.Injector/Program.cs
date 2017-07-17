using System;

namespace MonoDevelop.Mac.Injector
{
	partial class MainClass
	{

		public static void Main (string[] args)
		{
			var targetExecutablePath = "/Users/josemedranojimenez/CloudStation/Projects/profiler/src/XamarinProfiler.Mac/bin/Debug/Xamarin Profiler.exe";
			var appTarget = new TargetAppContext (targetExecutablePath);
			//appTarget.Inject (0);
			appTarget.Run ();
		}
	}
}
