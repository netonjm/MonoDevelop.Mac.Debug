﻿// This file has been autogenerated from a class added in the UI designer.

using System;

namespace MonoDevelop.Inspector
{
	public interface IMainWindowWrapper: IWindowWrapper
	{
		InspectorViewMode ViewMode { get; set; }
    }

	public interface IWindowWrapper : INativeObject
    {
		IViewWrapper ContentView { get; set; }
		IViewWrapper FirstResponder { get; }
		bool HasParentWindow { get; }
      
        event EventHandler LostFocus;
		event EventHandler ResizeRequested;
		event EventHandler MovedRequested;

		void AddChildWindow (IWindowWrapper borderer);
		void RecalculateKeyViewLoop ();
		bool ContainsChildWindow(IWindowWrapper debugOverlayWindow);

		void AlignLeft(IWindowWrapper toView, int pixels);
		void AlignTop(IWindowWrapper toView, int pixels);
		void AlignRight(IWindowWrapper toView, int pixels);

		void SetTitle(string v);
		void SetContentSize(int toolbarWindowWidth, int toolbarWindowHeight);
		void Close();
        void RefreshKeyLoop();
        void SetAppareance(bool isDark);
    }
}
