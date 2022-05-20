using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace VisualStudio.ViewInspector.Extension
{
    class InitializeCommand : CommandHandler
    {
        protected override void Run()
        {
            IdeApp.Initialized += (s, e) =>
            {
                var propertyPanelDelegate = new PropertyPanelDelegate();
                Runtime.Initialize(propertyPanelDelegate);
            };
        }
    }
}

