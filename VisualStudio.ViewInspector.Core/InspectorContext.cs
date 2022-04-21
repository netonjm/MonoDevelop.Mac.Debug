
using System;
using System.Collections.Generic;
using System.IO;
using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Modules;

namespace VisualStudio.ViewInspector
{
	public class InspectorContext : IDisposable
	{
        //refresh bar
        public event EventHandler<IView> FocusedViewChanged;

        public readonly List<IInspectorTabModule> Modules = new List<IInspectorTabModule>();

        readonly public string ModulesDirectoryPath;

        protected bool hasToolkit;

        IInspectDelegate Delegate;
        InspectorManager Manager;
        IWindowWatcher Watcher;

        public IView SelectedView => Manager.SelectedView;

        public bool Started => Watcher?.Started ?? false;

        public InspectorContext ()
		{
            var home = Environment.GetEnvironmentVariable("HOME");
            ModulesDirectoryPath = Path.Combine(home, ".cache", "MonoDevelop.Inspector", "modules");
        }

        internal void Initialize (IInspectDelegate inspectDelegate, InspectorManager manager, bool hasToolkit)
        {
            this.Delegate = inspectDelegate;
            this.hasToolkit = hasToolkit;

            Manager = manager;
            Manager.FocusedViewChanged += (s, e) => FocusedViewChanged?.Invoke(s, e);
        }

        public void StartWatcher ()
        {
            StopWatcher();

            Watcher = this.Delegate.CreateWatcher();
            Watcher.IsWindowAllowedFunc = w => this.Delegate.IsAllowedWindow(w) && Manager.IsAllowedWindow(w);

            //IMainWindow currentWindow = Delegate.GetTopWindow();

            //Attach(currentWindow);

            Watcher.ResponderChanged += Watcher_ResponderChanged;
            Watcher.WindowChanged += Watcher_WindowChanged;

            Watcher.Start();
        }

        public void StopWatcher()
        {
            if (Watcher == null)
                return;

            Attach(null);

            Watcher.Stop();
            Watcher.ResponderChanged -= Watcher_ResponderChanged;
            Watcher.WindowChanged -= Watcher_WindowChanged;
        }

        void Watcher_WindowChanged(object sender, IMainWindow e)
        {
            Attach(e);
        }

        void Watcher_ResponderChanged(object sender, INativeObject e)
        {
            if (e == null)
            {
                ChangeFocusedView(null);
                return;
            }
            if (Delegate.TryGetView(e, out var view))
            {
                ChangeFocusedView(view);
            }
        }

        public void Attach (IMainWindow window) 
		{
			Manager.SetWindow(window);
		}

		public void ChangeFocusedView(IView nSView) => Manager.ChangeFocusedView(nSView);

        public void Dispose()
        {
            StopWatcher();
        }

        public static InspectorContext Current { get; set; } = new InspectorContext();
    }
}
