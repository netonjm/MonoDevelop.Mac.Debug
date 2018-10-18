using System;
using GLib;
using Gtk;

namespace MonoDevelop.Mac.Debug
{
	public class GtkAccInspectorWindow : GtkWindowWrapper, IMainWindowWrapper
	{
		public GtkAccInspectorWindow (IntPtr raw) : base (raw)
		{
		}

		public GtkAccInspectorWindow (WindowType type) : base (type)
		{
		}

		public GtkAccInspectorWindow (string title) : base (title)
		{
		}

		protected GtkAccInspectorWindow (GType gtype) : base (gtype)
		{
		}

		public InspectorViewMode ViewMode { get; set; } = InspectorViewMode.Native;
	}
}
