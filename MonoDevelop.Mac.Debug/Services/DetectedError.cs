﻿// This file has been autogenerated from a class added in the UI designer.

using System.Collections.Generic;
using System;

namespace MonoDevelop.Mac.Debug.Services
{
	public class DetectedError
	{
		public IViewWrapper View { get; set; }

		public IViewWrapper View2 { get; set; }

		public nfloat ContrastRatio { get; set; }

		public string Color1 { get; set; }

		public string Color2 { get; set; }

		public DetectedErrorType ErrorType { get; set; }

		public string GetTitleMessage ()
		{
			List<string> errors = new List<string> ();
			if (ErrorType.HasFlag (DetectedErrorType.AccessibilityTitle) || ErrorType.HasFlag (DetectedErrorType.AccessibilityHelp)) {
				errors.Add ("no description");
			}
			if (ErrorType.HasFlag (DetectedErrorType.AccessibilityParent)) {
				errors.Add ("no accessibility parent set");
			}
			if (ErrorType.HasFlag (DetectedErrorType.Contrast)) {
				errors.Add ("constrast issues");
			}
			return string.Format ("Element has {0}", string.Join (",", errors)); ;
		}

		public string GetChildMessage ()
		{
			List<string> errors = new List<string> ();
			List<string> additionalLines = new List<string> ();
			var type = View.Content.GetType ().ToString ();
			if (ErrorType.HasFlag (DetectedErrorType.AccessibilityHelp)) {
				additionalLines.Add ($"This {type} needs set the AccessibilityHelp field");
			}
			if (ErrorType.HasFlag (DetectedErrorType.AccessibilityTitle)) {
				additionalLines.Add ($"This {type} needs set the AccessibilityTitle field");
			}
			if (ErrorType.HasFlag (DetectedErrorType.AccessibilityParent)) {
				additionalLines.Add ($"This {type} needs set the AccessibilityParent field");
			}

			if (ErrorType.HasFlag (DetectedErrorType.Contrast)) {
				additionalLines.Add (string.Format ("The text constrast ratio is {0}. This is based in color {1} compared with color {2}", ContrastRatio, Color1, Color2));
			}

			return string.Join (Environment.NewLine, additionalLines);
		}
	}
}