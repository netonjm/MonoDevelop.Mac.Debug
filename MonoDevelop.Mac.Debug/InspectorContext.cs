
using System;
using System.Collections.Generic;
using AppKit;

namespace MonoDevelop.Mac.Debug
{
	public static class InspectorContext
	{
		readonly static List<IWindowWrapper> windows = new List<IWindowWrapper>();
		static InspectorManager manager { get; set; }

		static InspectorContext ()
		{
			var macDelegate = new MacInspectorDelegate();
			manager = new InspectorManager (macDelegate);
		}

		internal static void Attach (IWindowWrapper window) 
		{
			if (!windows.Contains (window)) {
				windows.Add(window);
			}
			manager.SetWindow(window);
		}

		internal static void ChangeFocusedView(IViewWrapper nSView) => manager.ChangeFocusedView(nSView);

		public static void Attach (IWindowWrapper window, bool needsWatcher)
		{
			Attach(window);
			if (needsWatcher) {
				manager.StartWatcher();
			}
		}

		public static void Remove (IWindowWrapper window)
		{
			windows.Remove(window);
			manager.SetWindow(null);
		}
	}
}
