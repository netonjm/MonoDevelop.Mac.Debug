
using System;
using System.Collections.Generic;
using System.IO;

namespace MonoDevelop.Inspector
{
	internal class InspectorContext
	{
        public event EventHandler<IView> FocusedViewChanged;
        public readonly List<IInspectorTabModule> Modules = new List<IInspectorTabModule>();

        readonly List<IMainWindow> windows = new List<IMainWindow> ();
        internal InspectorManager Manager { get; set; }

        readonly public string ModulesDirectoryPath;

        public InspectorContext ()
		{
            var home = Environment.GetEnvironmentVariable("HOME");
            ModulesDirectoryPath = Path.Combine(home, ".cache", "MonoDevelop.Inspector", "modules");
        }

        protected bool hasToolkit;
        public void Initialize (InspectorManager manager, bool hasToolkit)
        {
            this.hasToolkit = hasToolkit;
            Manager = manager;
            Manager.FocusedViewChanged += (s,e) => FocusedViewChanged?.Invoke (s,e);
        }

        public void Attach (IMainWindow window) 
		{
			if (!windows.Contains (window)) {
				windows.Add(window);
			}
			Manager.SetWindow(window);
		}

		public void ChangeFocusedView(IView nSView) => Manager.ChangeFocusedView(nSView);

		public void Remove (IMainWindow window)
		{
			windows.Remove(window);
			Manager.SetWindow(null);
		}

        protected virtual InspectorManager GetInitializationContext()
        {
            return Manager;
        }

        public static InspectorContext Current { get; set; } = new InspectorContext();
    }
}
