
using System;
using System.Collections.Generic;

namespace MonoDevelop.Inspector
{
	abstract internal class InspectorContext
	{
		readonly List<IMainWindowWrapper> windows = new List<IMainWindowWrapper> ();
        InspectorManager manager { get; set; }

        public InspectorContext ()
		{
            manager = GetInitializationContext();
		}

        public void Initialize ()
        {
            //
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
