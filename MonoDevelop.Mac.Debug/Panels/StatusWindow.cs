using System;
using CoreGraphics;
using AppKit;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Tests;
using Xamarin.PropertyEditing.Themes;
using Foundation;

namespace MonoDevelop.Mac.Debug
{
	class StatusWindow : NSWindow
	{
		public event EventHandler<NSView> RaiseFirstResponder;

		public const int ButtonWidth = 30;
		const int margin = 10;
		const int ScrollViewSize = 150;
		readonly MockEditorProvider editorProvider;
		readonly MockResourceProvider resourceProvider;
		readonly MockBindingProvider bindingProvider;

		PropertyEditorPanel propertyEditorPanel;
		NSLayoutConstraint constraint;

		NSView contentView;
		MethodListView methodListView;

		public OutlineView outlineView { get; private set; }

		public StatusWindow (IntPtr handle) : base(handle)
		{

		}

		public StatusWindow (CGRect frame) : base (frame, NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false)
		{
			ShowsToolbarButton = false;
			Title = InspectorManager.Title;
			MovableByWindowBackground = false;

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
			ContentView = contentView;

			var stackView = NativeViewHelpers.CreateVerticalStackView(margin);
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

			//Method list view
			methodListView = new MethodListView();
			methodListView.AddColumn(new NSTableColumn("col") { Title = "Methods" });
			methodListView.DoubleClick += MethodListView_DoubleClick;

			var scrollView = new ScrollContainerView (methodListView);

			stackView.AddArrangedSubview(scrollView);
			scrollView.HeightAnchor.ConstraintEqualToConstant(ScrollViewSize).Active = true;

			var titleContainter = NativeViewHelpers.CreateHorizontalStackView();
			stackView.AddArrangedSubview(titleContainter);

			var invokeButton = new ImageButton(NSImage.ImageNamed("execute-16"));
			invokeButton.ToolTip = "Invoke Method!";
			invokeButton.WidthAnchor.ConstraintEqualToConstant(ButtonWidth).Active = true;

			titleContainter.AddArrangedSubview(invokeButton);
			invokeButton.Activated += (s, e) => InvokeSelectedView();

			titleContainter.AddArrangedSubview(NativeViewHelpers.CreateLabel ("Result: "));

			resultMessage = NativeViewHelpers.CreateLabel ("");
			resultMessage.LineBreakMode = NSLineBreakMode.ByWordWrapping;
			resultMessage.SetContentCompressionResistancePriority (250, NSLayoutConstraintOrientation.Vertical);
			resultMessage.SetContentHuggingPriorityForOrientation (250, NSLayoutConstraintOrientation.Vertical);

			titleContainter.AddArrangedSubview(resultMessage);

			//add property panel
			stackView.AddArrangedSubview (propertyEditorPanel);

			methodListView.SelectionChanged += (s, e) =>
			{
				if (methodListView.SelectedItem is MethodTableViewItem itm) {
					invokeButton.Enabled = itm.MethodInfo.GetParameters().Count() == 0;
				}
			};

			DidResize += Handle_DidResize;
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

				if (parameters.Count() == 0)
				{
					try
					{
						var response = method.Invoke(viewSelected, null);
						resultMessage.StringValue = response?.ToString() ?? "<null>";
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
			};
		}

		void MethodListView_DoubleClick(object sender, EventArgs e) => InvokeSelectedView();

		void Handle_DidResize(object sender, EventArgs e)
		{
			constraint.Constant = contentView.Frame.Height - margin * 2;
		}

		NodeView data;
		public void SetOutlineData (NodeView data)
		{
			this.data = data;
			outlineView.SetData (data);
		}

		NSView viewSelected;

		public void GenerateStatusView (NSView view, NSView nextKeyView, NSView previousKeyView)
		{
			viewSelected = view;
			propertyEditorPanel.Select(new ViewWrapper[] { viewSelected.GetWrapper () });
			methodListView.SetObject (view);
			if (data != null) {
				var found = data.Recursively (view);
				if (found != null) {
					outlineView.FocusNode (found);
				}
			}
		}


		protected override void Dispose(bool disposing)
		{
			methodListView.DoubleClick -= MethodListView_DoubleClick;
		
			DidResize -= Handle_DidResize;
			base.Dispose(disposing);
		}
	}
}
