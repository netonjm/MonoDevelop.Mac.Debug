
using System;
using System.Collections.Generic;
using AppKit;

namespace MonoDevelop.Mac.Debug
{
	public static class InspectorContext
	{
		readonly static List<NSWindow> windows = new List<NSWindow>();
		static InspectorManager manager { get; set; }

		static InspectorContext ()
		{
			manager = new InspectorManager ();
		}

		internal static void Attach (NSWindow window) 
		{
			if (!windows.Contains (window)) {
				windows.Add(window);
			}
			manager.SetWindow(window);
		}

		internal static void ChangeFocusedView(NSView nSView) => manager.ChangeFocusedView(nSView);

		public static void Attach (NSWindow window, bool needsWatcher)
		{
			Attach(window);
			if (needsWatcher) {
				manager.StartWatcher();
			}
		}

		public static void Remove (NSWindow window)
		{
			windows.Remove(window);
			manager.SetWindow(null);
		}
	}
}
