using AppKit;
using MonoDevelop.Inspector.Mac.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xwt.Drawing;

namespace MonoDevelop.Inspector.Mac
{
	public class GtkInspectorDelegate : IInspectDelegate
	{
		public void ConvertToNodes (IViewWrapper view, NodeView nodeView, InspectorViewMode viewMode)
		{
			throw new NotImplementedException ();
		}

		public IBorderedWindow CreateErrorWindow (IViewWrapper view)
		{
			throw new NotImplementedException ();
		}

		public FontData GetFont (IViewWrapper view)
		{
			throw new NotImplementedException ();
		}

		public object GetWrapper (IViewWrapper viewSelected, InspectorViewMode viewMode)
		{
			throw new NotImplementedException ();
		}

		public Task InvokeImageChanged (IViewWrapper view, IWindowWrapper selectedWindow)
		{
			throw new NotImplementedException ();
		}

		public Task<Image> OpenDialogSelectImage (IWindowWrapper selectedWindow)
		{
			throw new NotImplementedException ();
		}

		public void Recursively (IViewWrapper contentView, List<DetectedError> DetectedErrors, InspectorViewMode viewMode)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAllErrorWindows (IWindowWrapper windowWrapper)
		{
			throw new NotImplementedException ();
		}

		public void SetButton (NSButton button, Image image)
		{
			throw new NotImplementedException ();
		}

		public void SetButton (NSImageView imageview, Image image)
		{
			throw new NotImplementedException ();
		}

		public void SetFont (IViewWrapper view, NSFont font)
		{
			throw new NotImplementedException ();
		}
	}
}