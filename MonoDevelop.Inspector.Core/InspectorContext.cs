
using System;
using System.Collections.Generic;
using System.IO;

namespace MonoDevelop.Inspector
{
	public class InspectorContext
	{
        public event EventHandler<IViewWrapper> FocusedViewChanged;
        public readonly List<IInspectorTabModule> Modules = new List<IInspectorTabModule>();

        readonly List<IMainWindowWrapper> windows = new List<IMainWindowWrapper> ();
        public IInspectorManager Manager { get; set; }

        readonly public string ModulesDirectoryPath;

        public InspectorContext ()
		{
            var home = Environment.GetEnvironmentVariable("HOME");
            ModulesDirectoryPath = Path.Combine(home, ".cache", "MonoDevelop.Inspector", "modules");
        }

        protected bool hasToolkit;
        public void Initialize (IInspectorManager manager, bool hasToolkit)
        {
            this.hasToolkit = hasToolkit;
            Manager = manager;
            Manager.FocusedViewChanged += (s,e) => FocusedViewChanged?.Invoke (s,e);


			foreach (var window in Manager.Windows) {
				window.Initialize ();
			}
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
