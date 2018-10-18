using System;

namespace MonoDevelop.Mac.Debug
{
	public interface IInspectorWindow : IWindowWrapper
	{
		event EventHandler<IViewWrapper> RaiseFirstResponder;
		event EventHandler<IViewWrapper> RaiseDeleteItem;
		void GenerateTree (IWindowWrapper window, InspectorViewMode viewMode);
		void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate, InspectorViewMode mode);
		void RemoveItem ();
	}
}
