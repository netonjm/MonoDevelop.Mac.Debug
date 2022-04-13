﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MonoDevelop.Inspector
{
    public interface IInspectDelegate
	{
        void SetFont(IView view, IFontWrapper font);
		FontData GetFont(IView view);
		void ConvertToNodes(IView view, INodeView nodeView, InspectorViewMode viewMode);
        void ConvertToNodes(IWindow window, INodeView nodeView, InspectorViewMode viewMode);
        object GetWrapper (INativeObject viewSelected, InspectorViewMode viewMode);
		void Recursively (IView contentView, List<DetectedError> DetectedErrors, InspectorViewMode viewMode);
		void RemoveAllErrorWindows(IWindow windowWrapper);
		Task<IImage> OpenDialogSelectImage(IWindow selectedWindow);
        //IToolbarWrapperDelegateWrapper GetTouchBarDelegate(object element);
        void SetButton(IButton button, IImage image);
		void SetButton(IImageView imageview, IImage image);
		Task InvokeImageChanged(IView view, IWindow selectedWindow);
		IBorderedWindow CreateErrorWindow (IView view);
        IFontWrapper GetFromName(string selected, int fontSize);
        IMenu GetSubMenu();
        void ClearSubmenuItems(List<IMenuItem> menuItems, IMenu submenu);
        IMenuItem CreateMenuItem(string title, EventHandler menuItemOpenHandler);
        IMenuItem GetSeparatorMenuItem();
        IMenuItem GetShowInspectorWindowMenuItem(EventHandler menuItemOpenHandler);
        IMenuItem GetShowAccessibilityWindowMenuItem(EventHandler menuItemOpenHandler);
        IImage GetImageResource(string v);
        IButton GetImageButton(IImage invokeImage);
        void SetAppearance(bool isDark,params IWindow[] inspectorWindow);
        void CreateItem(IView view, ToolbarView e);
        void SetCultureInfo(CultureInfo cultureInfo);
	}
}