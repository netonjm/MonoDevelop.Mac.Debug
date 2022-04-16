using System;

namespace VisualStudio.ViewInspector.Abstractions
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
        ImageBox
    }

    public interface IInspectorWindow : IWindow
	{
        event EventHandler<INativeObject> FirstRespondedChanged;
		event EventHandler<INativeObject> ItemDeleted;
        event EventHandler<ToolbarView> ItemInserted;

		void GenerateTree (IWindow window, InspectorViewMode viewMode);
		void Select (INativeObject view, InspectorViewMode mode);
		void RemoveItem ();
        void Initialize();
	}
}
