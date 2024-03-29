﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Globalization;

namespace VisualStudio.ViewInspector.Abstractions
{
    public interface IToolbarWrapperDelegateWrapper : INativeObject
    {

    }

    interface IToolbarWindow : IWindow
	{
        event EventHandler ShowInspectorButtonPressed;
        event EventHandler ShowAccessibilityPressed;

        event EventHandler<bool> KeyViewLoop;
		event EventHandler<bool> NextKeyViewLoop;
		event EventHandler<bool> PreviousKeyViewLoop;
		event EventHandler<bool> ThemeChanged;
        event EventHandler<CultureInfo> CultureChanged;
        event EventHandler ItemDeleted;
		event EventHandler ItemImageChanged;
		event EventHandler<FontData> FontChanged;
		event EventHandler<InspectorViewMode> InspectorViewModeChanged;
        event EventHandler RefreshTreeViewRequested;

        void ChangeView(InspectorManager manager, INativeObject nativeObject);

        event EventHandler<IColor> ViewBackgroundColorRequested;
        event EventHandler ViewBackgroundColorClearRequested;
    }
}
