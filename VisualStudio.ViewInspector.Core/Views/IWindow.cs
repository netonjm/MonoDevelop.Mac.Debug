﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Globalization;

namespace VisualStudio.ViewInspector.Abstractions
{
	public interface IMainWindow: IWindow
	{
		InspectorViewMode ViewMode { get; set; }
    }

	public interface IWindow : INativeObject
    {
		IView ContentView { get; set; }
		IView FirstResponder { get; }

		IWindow ParentWindow { get; }

		public string Title { get; set; }

        event EventHandler LostFocus;
		event EventHandler ResizeRequested;
		event EventHandler Closing;
		event EventHandler MovedRequested;

		void AddChildWindow (IWindow borderer);
		void RemoveChildWindow(IWindow borderer);

		void RecalculateKeyViewLoop ();
		bool ContainsChildWindow(IWindow debugOverlayWindow);

		void AlignLeft(IWindow toView, int pixels);
		void AlignTop(IWindow toView, int pixels);
		void AlignRight(IWindow toView, int pixels);

        float FrameX { get; }
        float FrameY { get; }
        float FrameWidth { get; }
        float FrameHeight { get; }

		void SetContentSize(int toolbarWindowWidth, int toolbarWindowHeight);
		void Close();
        void SetAppareance(bool isDark);
    }
}
