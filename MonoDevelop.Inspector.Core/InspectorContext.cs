
using System;
using System.Collections.Generic;
using System.IO;

namespace MonoDevelop.Inspector
{
	internal class InspectorContext
	{
        public event EventHandler<IView> FocusedViewChanged;
        public readonly List<IInspectorTabModule> Modules = new List<IInspectorTabModule>();

        readonly public string ModulesDirectoryPath;

        public InspectorContext ()
		{
            var home = Environment.GetEnvironmentVariable("HOME");
            ModulesDirectoryPath = Path.Combine(home, ".cache", "MonoDevelop.Inspector", "modules");
        }

        protected bool hasToolkit;
        internal IInspectDelegate InspectorDelegate { get; private set; }
        internal InspectorManager Manager { get; private set; }

        public bool IsAllowedWindow(IWindow window)
        {
            return Manager.IsAllowedWindow(window);
        }

        internal void Initialize (IInspectDelegate inspectDelegate, InspectorManager manager, bool hasToolkit)
        {
            this.InspectorDelegate = inspectDelegate;
            this.hasToolkit = hasToolkit;

            Manager = manager;
            Manager.FocusedViewChanged += (s, e) => FocusedViewChanged?.Invoke(s, e);
        }

        public void Attach (IMainWindow window) 
		{
			Manager.SetWindow(window);
		}

		public void ChangeFocusedView(IView nSView) => Manager.ChangeFocusedView(nSView);

		public void Remove (IMainWindow window)
		{
			Manager.SetWindow(null);
		}

        protected virtual InspectorManager GetInitializationContext()
        {
            return Manager;
        }

        public static InspectorContext Current { get; set; } = new InspectorContext();
    }
}
