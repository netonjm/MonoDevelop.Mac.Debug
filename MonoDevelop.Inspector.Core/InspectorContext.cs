
using System;
using System.Collections.Generic;

namespace MonoDevelop.Inspector
{
	internal class InspectorContext
	{
        public event EventHandler<IViewWrapper> FocusedViewChanged;
        public readonly List<IInspectorTabModule> Modules = new List<IInspectorTabModule>();

        readonly List<IMainWindowWrapper> windows = new List<IMainWindowWrapper> ();
        internal InspectorManager Manager { get; set; }

        public InspectorContext ()
		{

		}

        protected bool hasToolkit;
        public void Initialize (InspectorManager manager, bool hasToolkit)
        {
            this.hasToolkit = hasToolkit;
            Manager = manager;
            Manager.FocusedViewChanged += (s,e) => FocusedViewChanged?.Invoke (s,e);
        }

        public void Attach (IMainWindowWrapper window) 
		{
			if (!windows.Contains (window)) {
				windows.Add(window);
			}
			Manager.SetWindow(window);
		}

		public void ChangeFocusedView(IViewWrapper nSView) => Manager.ChangeFocusedView(nSView);

		public void Remove (IMainWindowWrapper window)
		{
			windows.Remove(window);
			Manager.SetWindow(null);
		}
        public static InspectorContext Current { get; set; } = new InspectorContext();
    }
}
