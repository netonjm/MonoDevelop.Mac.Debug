﻿using System;

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
		ComboBoxItem,
        ImageBox,
        ScrollView,
        CustomView,
        SegmentedControl,
        Box,
        TabView,
        TabViewItem,
    }

	public interface IInspectorManagerWindow : IWindowWrapper
	{
		void Initialize ();
	}

	public interface IInspectorWindow : IInspectorManagerWindow
	{
        event EventHandler<INativeObject> RaiseFirstResponder;
		event EventHandler<INativeObject> RaiseDeleteItem;
        event EventHandler<ToolbarView> RaiseInsertItem;

		void GenerateTree (IWindowWrapper window, InspectorViewMode viewMode);
		void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate, InspectorViewMode mode);
		void RemoveItem ();
	}
}
