
using System;
using System.Collections.Generic;

namespace MonoDevelop.Inspector
{
	abstract internal class InspectorContext
	{
        public event EventHandler<IViewWrapper> FocusedViewChanged;

        readonly List<IMainWindowWrapper> windows = new List<IMainWindowWrapper> ();
        InspectorManager manager { get; set; }

        public InspectorContext ()
		{

		}

        protected bool hasToolkit;
        public void Initialize (bool hasToolkit)
        {
            this.hasToolkit = hasToolkit;
            manager = GetInitializationContext();
            manager.FocusedViewChanged += (s,e) => FocusedViewChanged?.Invoke (s,e);
        }

        protected abstract InspectorManager GetInitializationContext();

        public void Attach (IMainWindowWrapper window) 
		{
			if (!windows.Contains (window)) {
				windows.Add(window);
			}
			manager.SetWindow(window);
		}

		public void ChangeFocusedView(IViewWrapper nSView) => manager.ChangeFocusedView(nSView);

		public void Remove (IMainWindowWrapper window)
		{
			windows.Remove(window);
			manager.SetWindow(null);
		}
	}
}
