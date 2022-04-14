using MonoDevelop.Components.Commands;
using MonoDevelop.Inspector;
using MonoDevelop.Inspector.Mac;
using System;
using MonoDevelop;
using MonoDevelop.Ide;
using AppKit;
using MonoDevelop.Inspector.Mac.Services;

namespace VisualStudio.ViewInspector
{
    static class VisualStudioInspectorService
    {
        static WindowWatcher watcher;
        static ObservableWindow observableWindow;

        public static void Start()
        {
            Stop();
            watcher.Start();
        }

        public static void Stop()
        {
            watcher.Stop();
        }

        public static void Initialize()
        {
            IdeApp.Initialized += (s, e) =>
            {
                watcher = new WindowWatcher();

                //wait to all things loaded
                var inspectorContext = InspectorContext.Current;
                var inspectorDelegate = new MacInspectorDelegate();
                var inspectorManager = inspectorDelegate.CreateInspectorManager();
                inspectorContext.Initialize(inspectorDelegate, inspectorManager, false);

                watcher.IsWindowAllowedFunc = w => inspectorManager.IsAllowedWindow(new WindowWrapper(w));

                var currentWindow = NSApplication.SharedApplication.ModalWindow ?? NSApplication.SharedApplication.KeyWindow ?? MessageService.RootWindow.GetNativeWidget<NSWindow>();

                observableWindow = new ObservableWindow(currentWindow);
                inspectorContext.Attach(observableWindow);

                watcher.ResponderChanged += (s, e) =>
                {
                    if (e is NSView view)
                    {
                        inspectorContext.ChangeFocusedView(new TreeViewItemView(view));
                    }
                    if (e != null)
                        Console.WriteLine(e.GetType().FullName);
                };

                watcher.WindowChanged += (s, e) =>
                {
                    var tmpWindow = new ObservableWindow(e);
                    if (inspectorManager.IsAllowedWindow(tmpWindow))
                    {
                        observableWindow = tmpWindow;
                        inspectorContext.Attach(observableWindow);
                        Console.WriteLine(e.Title);
                    }
                };

                watcher.Start();
            };
        }
    }

    class InitializeCommand : CommandHandler
    {
        protected override void Run()
        {
            VisualStudioInspectorService.Initialize();
        }
    }
}

