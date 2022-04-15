namespace VisualStudio.ViewInspector
{
    public static class Runtime
    {
        public static void Initialize(bool hasToolkit = false)
        {
            var inspectorDelegate = new MacInspectorDelegate();
            var inspectorManager = inspectorDelegate.CreateInspectorManager();
            InspectorContext.Current.Initialize(inspectorDelegate, inspectorManager, hasToolkit);
            inspectorManager.ShowsInspectorWindow = true;
            inspectorManager.IsFirstResponderOverlayVisible = true;
        }
    }
}
