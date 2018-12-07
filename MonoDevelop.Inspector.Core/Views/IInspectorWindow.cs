using System;

namespace MonoDevelop.Inspector
{
    public enum ToolbarView
    {
        DatePicker,
        ScrollableTextView,
        WrappingLabel,
        TextField,
        PushButton,
        Label,
        Search,
        ComboBox,
        ImageBox,
        ScrollView,
        CustomView,
        SegmentedControl,
        Box,
        TabView,
        TabViewItem
    }

    public interface IInspectorWindow : IWindowWrapper
	{
        event EventHandler<INativeObject> RaiseFirstResponder;
		event EventHandler<INativeObject> RaiseDeleteItem;
        event EventHandler<ToolbarView> RaiseInsertItem;

		void GenerateTree (IWindowWrapper window, InspectorViewMode viewMode);
		void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate, InspectorViewMode mode);
		void RemoveItem ();
        void Initialize();
	}
}
