using System;
using CoreGraphics;
using AppKit;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Tests;
using Xamarin.PropertyEditing.Themes;
using Foundation;
using MonoDevelop.Mac.Debug.Services;

namespace MonoDevelop.Mac.Debug
{
	class InspectorWindow : ContentWindow
	{
		const ushort DeleteKey = 51;

		public event EventHandler<IViewWrapper> RaiseFirstResponder;
		public event EventHandler<IViewWrapper> RaiseDeleteItem;

		public const int ButtonWidth = 30;
		const int margin = 10;
		const int ScrollViewSize = 150;
		 MockEditorProvider editorProvider;
		 MockResourceProvider resourceProvider;
		 MockBindingProvider bindingProvider;

		PropertyEditorPanel propertyEditorPanel;
		NSLayoutConstraint constraint;

		NSView contentView;
		MethodListView methodListView;

		public OutlineView outlineView { get; private set; }

		readonly IInspectDelegate inspectorDelegate;

		public InspectorWindow (IInspectDelegate inspectorDelegate)
		{
			//new CGRect (10, 10, 600, 700))
			X = 10;
			Y = 10;
			Width = 600; Height = 700;
			this.inspectorDelegate = inspectorDelegate;
			OnInitialized ();
		}

		protected override void OnInitialized ()
		{
			base.OnInitialized ();
			currentWindow.ShowsToolbarButton = false;
			currentWindow.MovableByWindowBackground = false;

			currentWindow.StyleMask = NSWindowStyle.Titled | NSWindowStyle.Resizable;

			propertyEditorPanel = new PropertyEditorPanel();

			editorProvider = new MockEditorProvider();
			resourceProvider = new MockResourceProvider();
			bindingProvider = new MockBindingProvider();

			propertyEditorPanel.TargetPlatform = new TargetPlatform (editorProvider, resourceProvider, bindingProvider) {
				SupportsCustomExpressions = true,
				SupportsMaterialDesign = true,
			};

			var currentThemeStyle = NSUserDefaults.StandardUserDefaults.StringForKey ("AppleInterfaceStyle") ?? "Light";
			PropertyEditorPanel.ThemeManager.Theme = currentThemeStyle == "Dark" ? PropertyEditorTheme.Dark :  PropertyEditorTheme.Light;

			contentView = new NSView() { TranslatesAutoresizingMaskIntoConstraints = false };
			currentWindow.ContentView = contentView;

			var stackView = NativeViewHelper.CreateVerticalStackView(margin);
			contentView.AddSubview (stackView);

			stackView.LeftAnchor.ConstraintEqualToAnchor(contentView.LeftAnchor, margin).Active = true;
			stackView.RightAnchor.ConstraintEqualToAnchor(contentView.RightAnchor, -margin).Active = true;
			stackView.TopAnchor.ConstraintEqualToAnchor(contentView.TopAnchor, margin).Active = true;

			constraint = stackView.HeightAnchor.ConstraintEqualToConstant(contentView.Frame.Height-margin * 2);
			constraint.Active = true;

			outlineView = new OutlineView ();

			var outlineViewScrollView = new ScrollContainerView (outlineView);
			stackView.AddArrangedSubview (outlineViewScrollView);
			outlineViewScrollView.HeightAnchor.ConstraintEqualToConstant (ScrollViewSize).Active = true;

			outlineView.SelectionNodeChanged += (s, e) => {
				if (outlineView.SelectedNode is NodeView nodeView) {
					RaiseFirstResponder?.Invoke (this, nodeView.View);
				}
			};

			outlineView.KeyPress += (sender, e) =>
			{
				if (e == DeleteKey) {
					if (outlineView.SelectedNode is NodeView nodeView)
					{
						RaiseDeleteItem?.Invoke(this, nodeView.View);
					}
				}
			};

			//Method list view
			methodListView = new MethodListView();
			methodListView.AddColumn(new NSTableColumn("col") { Title = "Methods" });
			methodListView.DoubleClick += MethodListView_DoubleClick;

			var scrollView = new ScrollContainerView (methodListView);

			stackView.AddArrangedSubview(scrollView);
			scrollView.HeightAnchor.ConstraintEqualToConstant(ScrollViewSize).Active = true;

			var titleContainter = NativeViewHelper.CreateHorizontalStackView();
			stackView.AddArrangedSubview(titleContainter);

			var invokeButton = new ImageButton(ResourceService.GetNSImage("execute-16.png"));
			invokeButton.ToolTip = "Invoke Method!";
			invokeButton.WidthAnchor.ConstraintEqualToConstant(ButtonWidth).Active = true;

			titleContainter.AddArrangedSubview(invokeButton);
			invokeButton.Activated += (s, e) => InvokeSelectedView();

			titleContainter.AddArrangedSubview(NativeViewHelper.CreateLabel ("Result: "));

			resultMessage = NativeViewHelper.CreateLabel ("");
			resultMessage.LineBreakMode = NSLineBreakMode.ByWordWrapping;
			resultMessage.SetContentCompressionResistancePriority (250, NSLayoutConstraintOrientation.Vertical);
			resultMessage.SetContentHuggingPriorityForOrientation (250, NSLayoutConstraintOrientation.Vertical);

			titleContainter.AddArrangedSubview(resultMessage);

			//add property panel
			stackView.AddArrangedSubview (propertyEditorPanel);

			//methodListView.SelectionChanged += (s, e) =>
			//{
			//	//if (methodListView.SelectedItem is MethodTableViewItem itm) {
			//	//	invokeButton.Enabled = itm.MethodInfo.GetParameters().Count() == 0;
			//	//}
			//};

			currentWindow.DidResize += Handle_DidResize;
		}


		NodeView data;

		internal void GenerateTree(IWindowWrapper window)
		{
			data = new NodeView(window.ContentView);
			inspectorDelegate.ConvertToNodes(window.ContentView, data);
			outlineView.SetData(data);
		}

		NSTextField resultMessage;

		void InvokeSelectedView ()
		{
			if (viewSelected == null)
			{
				return;
			}

			if (methodListView.SelectedItem is MethodTableViewItem itm)
			{
				//itm.MethodInfo
				var method = itm.MethodInfo;
				var parameters = method.GetParameters();

				List<object> arguments = null;
				if (parameters.Count () > 0) {
					arguments = new List<object> ();
					foreach (var item in parameters) {
						arguments.Add (null);
					}
				}
			
				try
				{
					var response = method.Invoke(viewSelected.Content, parameters);
					resultMessage.StringValue = response?.ToString() ?? "<null>";
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			};
		}

		void MethodListView_DoubleClick(object sender, EventArgs e) => InvokeSelectedView();

		void Handle_DidResize(object sender, EventArgs e)
		{
			//constraint.Constant = contentView.Frame.Height - margin * 2;
		}

		IViewWrapper viewSelected;

		public void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate)
		{
			viewSelected = view;
			propertyEditorPanel.Select(new ViewWrapper[] { inspectDelegate.GetWrapper (viewSelected) });
			methodListView.SetObject (view.Content);
			if (data != null) {
				var found = data.Search(view);
				if (found != null) {
					outlineView.FocusNode (found);
				}
			}
		}

		internal void RemoveItem()
		{
			if (outlineView.SelectedNode is NodeView nodeView)
			{
				RaiseDeleteItem?.Invoke(this, nodeView.View);
			}
		}

		protected override void Dispose(bool disposing)
		{
			methodListView.DoubleClick -= MethodListView_DoubleClick;

			currentWindow.DidResize -= Handle_DidResize;
			base.Dispose(disposing);
		}
	}
}
