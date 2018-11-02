﻿// This file has been autogenerated from a class added in the UI designer.

using System.Collections.Generic;
using System;

namespace MonoDevelop.Inspector.Services
{
	public class AccessibilityService
	{
		public const int MaxIssues = 200000;
		readonly public List<DetectedError> DetectedErrors = new List<DetectedError> ();
		public event EventHandler<IWindowWrapper> ScanFinished;

		IWindowWrapper window;

		AccessibilityService ()
		{

		}

		public int IssuesFound {
			get => DetectedErrors.Count;
		}


		bool HasError (IViewWrapper customView)
		{
			if (!customView.CanBecomeKeyView || customView.Hidden) {
				return false;
			}

			if (string.IsNullOrEmpty (customView.AccessibilityTitle)) {
				return true;
			}
			if (string.IsNullOrEmpty (customView.AccessibilityHelp)) {
				return true;
			}

			return false;
		}

		public void Reset ()
		{
			DetectedErrors.Clear();
		}

		public void ScanErrors (IInspectDelegate inspectDelegate, IWindowWrapper currentWindow, InspectorViewMode viewMode)
		{
			window = currentWindow;
			DetectedErrors.Clear();
			inspectDelegate.Recursively(window.ContentView, DetectedErrors, viewMode);
			ScanFinished?.Invoke (this, window);
		}

		public static AccessibilityService Current { get; } = new AccessibilityService();
	}
}