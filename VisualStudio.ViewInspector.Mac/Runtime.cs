namespace VisualStudio.ViewInspector
{
    public static class Runtime
    {
        public static InspectorContext Initialize(bool hasToolkit = false)
        {
            var inspectorDelegate = new MacInspectorDelegate();
            var inspectorManager = inspectorDelegate.CreateInspectorManager();
            InspectorContext.Current.Initialize(inspectorDelegate, inspectorManager, hasToolkit);
            inspectorManager.ShowsToolBarWindow = true;
            inspectorManager.ShowsInspectorWindow = true;
            inspectorManager.IsFirstResponderOverlayVisible = true;
            return InspectorContext.Current;
        }
    }
}
