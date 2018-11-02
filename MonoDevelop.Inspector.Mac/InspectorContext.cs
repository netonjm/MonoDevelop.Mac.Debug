
using System;
using System.Collections.Generic;
using AppKit;

namespace MonoDevelop.Inspector.Mac
{
	public static class InspectorContext
	{
		readonly static List<IMainWindowWrapper> windows = new List<IMainWindowWrapper> ();
		static InspectorManager manager { get; set; }

		static InspectorContext ()
		{
			var macDelegate = new MacInspectorDelegate();
			manager = new InspectorManager (macDelegate);
		}

		public static void Attach (IMainWindowWrapper window) 
		{
			if (!windows.Contains (window)) {
				windows.Add(window);
			}
			manager.SetWindow(window);
		}

		public static void ChangeFocusedView(IViewWrapper nSView) => manager.ChangeFocusedView(nSView);

		public static void Remove (IMainWindowWrapper window)
		{
			windows.Remove(window);
			manager.SetWindow(null);
		}
	}
}
