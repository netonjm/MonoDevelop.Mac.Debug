using System;

namespace MonoDevelop.Mac.Debug
{
	public interface IInspectorWindow : IWindowWrapper
	{
		event EventHandler<IViewWrapper> RaiseFirstResponder;
		event EventHandler<IViewWrapper> RaiseDeleteItem;
		void GenerateTree (IWindowWrapper window);
		void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate);
		void RemoveItem ();
	}
}
