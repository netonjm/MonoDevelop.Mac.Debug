using System;
using CoreGraphics;
using AppKit;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Themes;
using Foundation;
using System.Linq;

namespace MonoDevelop.Inspector.Mac
{
	class InspectorWindow : MacInspectorManagerWindow, IInspectorWindow
	{
		const ushort DeleteKey = 51;

		public event EventHandler<INativeObject> RaiseFirstResponder;
		public event EventHandler<INativeObject> RaiseDeleteItem;

		public const int ButtonWidth = 30;
		const int margin = 10;
		const int ScrollViewSize = 240;
		readonly PropertyEditorProvider editorProvider;

		PropertyEditorPanel propertyEditorPanel;

		readonly NSView contentView;
		MethodListView methodListView;
		public OutlineView outlineView { get; private set; }

		NSTabView tabView;

		readonly IInspectDelegate inspectorDelegate;
		MacInspectorToolbarView toolbarView;

		public event EventHandler<ToolbarView> RaiseInsertItem;

		MacTabWrapper toolbarTabViewWrapper;

		public InspectorWindow (IInspectDelegate inspectorDelegate, CGRect frame) : base (frame, NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false)
		{
			this.inspectorDelegate = inspectorDelegate;
			ShowsToolbarButton = false;
			MovableByWindowBackground = false;

			propertyEditorPanel = new PropertyEditorPanel ();

			editorProvider = new PropertyEditorProvider ();

			propertyEditorPanel.TargetPlatform = new TargetPlatform (editorProvider) {
				SupportsCustomExpressions = true,
				SupportsMaterialDesign = true,
			};

			var currentThemeStyle = NSUserDefaults.StandardUserDefaults.StringForKey ("AppleInterfaceStyle") ?? "Light";
			PropertyEditorPanel.ThemeManager.Theme = currentThemeStyle == "Dark" ? PropertyEditorTheme.Dark : PropertyEditorTheme.Light;

			contentView = ContentView;

			var stackView = NativeViewHelper.CreateVerticalStackView (margin);
			contentView.AddSubview (stackView);

			stackView.LeftAnchor.ConstraintEqualToAnchor (contentView.LeftAnchor, margin).Active = true;
			stackView.RightAnchor.ConstraintEqualToAnchor (contentView.RightAnchor, -margin).Active = true;
			stackView.TopAnchor.ConstraintEqualToAnchor (contentView.TopAnchor, margin).Active = true;

			var bottom = stackView.BottomAnchor.ConstraintEqualToAnchor (contentView.BottomAnchor, -margin);
			bottom.Priority = (uint)NSLayoutPriority.DefaultLow;
			bottom.Active = true;
			outlineView = new OutlineView ();
			var outlineViewScrollView = new ScrollContainerView (outlineView);

			outlineView.SelectionNodeChanged += (s, e) => {
				if (outlineView.SelectedNode is NodeView nodeView) {
					RaiseFirstResponder?.Invoke (this, nodeView.Wrapper);
				}
			};

			outlineView.KeyPress += (sender, e) => {
				if (e == DeleteKey) {
					if (outlineView.SelectedNode is NodeView nodeView) {
						RaiseDeleteItem?.Invoke (this, nodeView.Wrapper);
					}
				}
			};

			//TOOLBAR
			var toolbarTab = new NSTabView () { TranslatesAutoresizingMaskIntoConstraints = false };
			toolbarTabViewWrapper = new MacTabWrapper (toolbarTab);

			toolbarTab.WantsLayer = true;
			toolbarTab.Layer.BackgroundColor = NSColor.Red.CGColor;

			stackView.AddArrangedSubview (toolbarTab);

			toolbarTab.LeftAnchor.ConstraintEqualToAnchor (contentView.LeftAnchor, margin).Active = true;
			toolbarTab.RightAnchor.ConstraintEqualToAnchor (contentView.RightAnchor, -margin).Active = true;
			toolbarTab.TopAnchor.ConstraintEqualToAnchor (contentView.TopAnchor, margin).Active = true;
			toolbarTab.HeightAnchor.ConstraintEqualToConstant (ScrollViewSize).Active = true;

			/////////////////

			var toolbarTabItem = new NSTabViewItem ();
			toolbarTabItem.Label = "Toolbar";

			var toolbarStackView = NativeViewHelper.CreateVerticalStackView ();
			toolbarStackView.TranslatesAutoresizingMaskIntoConstraints = true;
			var toolbarHorizontalStackView = NativeViewHelper.CreateHorizontalStackView ();
			toolbarHorizontalStackView.TranslatesAutoresizingMaskIntoConstraints = true;

			toolbarSearchTextField = new NSSearchField ();
			toolbarSearchTextField.Changed += (object sender, EventArgs e) => {
				Search ();
			};

			toolbarHorizontalStackView.AddArrangedSubview (toolbarSearchTextField);

			var compactModeToggleButton = new ToggleButton ();
			compactModeToggleButton.TranslatesAutoresizingMaskIntoConstraints = true;
			compactModeToggleButton.Image = inspectorDelegate.GetImageResource ("compact-display-16.png").NativeObject as NSImage;
			compactModeToggleButton.ToolTip = "Use compact display";
			toolbarHorizontalStackView.AddArrangedSubview (compactModeToggleButton);

			toolbarStackView.AddArrangedSubview (toolbarHorizontalStackView);

			toolbarView = new MacInspectorToolbarView ();
			var toolbarViewScrollView = new ScrollContainerView (toolbarView);
			toolbarStackView.AddArrangedSubview (toolbarViewScrollView);

			toolbarTabItem.View = toolbarStackView;
			toolbarView.ActivateSelectedItem += (sender, e) => {
				RaiseInsertItem?.Invoke (this, toolbarView.SelectedItem.TypeOfView);
			};

			var outlineTabItem = new NSTabViewItem ();
			outlineTabItem.Label = "View Hierarchy";
			outlineTabItem.View = outlineViewScrollView;

			toolbarTab.Add (outlineTabItem);
			toolbarTab.Add (toolbarTabItem);

			//===================

			//Method list view
			methodListView = new MethodListView ();
			methodListView.AddColumn (new NSTableColumn ("col") { Title = "Methods" });
			methodListView.DoubleClick += MethodListView_DoubleClick;

			scrollView = new ScrollContainerView (methodListView);

			var titleContainter = NativeViewHelper.CreateHorizontalStackView ();
			//titleContainter.WantsLayer = true;
			//titleContainter.Layer.BackgroundColor = NSColor.Gray.CGColor;

			methodSearchView = new NSSearchField () { TranslatesAutoresizingMaskIntoConstraints = false };
			titleContainter.AddArrangedSubview (methodSearchView);
			methodSearchView.WidthAnchor.ConstraintEqualToConstant (180).Active = true;

			IImageWrapper invokeImage = inspectorDelegate.GetImageResource ("execute-16.png");
			IButtonWrapper invokeButton = inspectorDelegate.GetImageButton (invokeImage);
			invokeButton.SetTooltip ("Invoke Method!");
			invokeButton.SetWidth (ButtonWidth);

			titleContainter.AddArrangedSubview ((NSView)invokeButton.NativeObject);
			invokeButton.Pressed += (s, e) => InvokeSelectedView ();

			titleContainter.AddArrangedSubview (NativeViewHelper.CreateLabel ("Result: "));

			resultMessage = NativeViewHelper.CreateLabel ("");
			resultMessage.LineBreakMode = NSLineBreakMode.ByWordWrapping;

			titleContainter.AddArrangedSubview (resultMessage);

			var methodStackPanel = NativeViewHelper.CreateVerticalStackView ();
			methodStackPanel.AddArrangedSubview (titleContainter);
			titleContainter.LeftAnchor.ConstraintEqualToAnchor (methodStackPanel.LeftAnchor, 0).Active = true;
			titleContainter.RightAnchor.ConstraintEqualToAnchor (methodStackPanel.RightAnchor, 0).Active = true;

			methodStackPanel.AddArrangedSubview (scrollView);
			/////

			var tabPropertyPanel = new NSTabViewItem ();
			tabPropertyPanel.View = propertyEditorPanel;
			tabPropertyPanel.Label = "Properties";

			var tabMethod = new NSTabViewItem ();

			tabMethod.View.AddSubview (methodStackPanel);
			methodStackPanel.LeftAnchor.ConstraintEqualToAnchor (tabMethod.View.LeftAnchor, 0).Active = true;
			methodStackPanel.RightAnchor.ConstraintEqualToAnchor (tabMethod.View.RightAnchor, 0).Active = true;
			methodStackPanel.TopAnchor.ConstraintEqualToAnchor (tabMethod.View.TopAnchor, 0).Active = true;
			methodStackPanel.BottomAnchor.ConstraintEqualToAnchor (tabMethod.View.BottomAnchor, 0).Active = true;

			tabMethod.Label = "Methods";

			tabView = new NSTabView () { TranslatesAutoresizingMaskIntoConstraints = false };
			tabView.Add (tabPropertyPanel);
			tabView.Add (tabMethod);
			tabView.SetContentCompressionResistancePriority ((uint)NSLayoutPriority.WindowSizeStayPut, NSLayoutConstraintOrientation.Horizontal);
			tabView.SetContentCompressionResistancePriority ((uint)NSLayoutPriority.WindowSizeStayPut, NSLayoutConstraintOrientation.Vertical);
			stackView.AddArrangedSubview (tabView as NSView);

			tabView.LeftAnchor.ConstraintEqualToAnchor (stackView.LeftAnchor, 0).Active = true;
			tabView.RightAnchor.ConstraintEqualToAnchor (stackView.RightAnchor, 0).Active = true;

			methodSearchView.Activated += (sender, e) => {
				if (viewSelected != null) {
					methodListView.SetObject (viewSelected.NativeObject, methodSearchView.StringValue);
				}
			};

			compactModeToggleButton.Activated += (sender, e) => {
				toolbarView.ShowOnlyImages (!toolbarView.IsImageMode);
			};
		}

		NSSearchField methodSearchView;
		ScrollContainerView scrollView;
		NodeView data;

		List<CollectionHeaderItem> toolbarData = new List<CollectionHeaderItem> ();

		public override void Initialize ()
		{
			base.Initialize ();
			toolbarView.RegisterClassForItem (typeof (MacInspectorToolbarHeaderCollectionViewItem), MacInspectorToolbarHeaderCollectionViewItem.Name);
			toolbarView.RegisterClassForItem (typeof (MacInspectorToolbarCollectionViewItem), MacInspectorToolbarCollectionViewItem.Name);
			toolbarView.RegisterClassForItem (typeof (MacInspectorToolbarImageCollectionViewItem), MacInspectorToolbarImageCollectionViewItem.Name);

			var toolbarItem = new CollectionHeaderItem () { Label = "Main" };

			const string defaultImage = "view_image.png";

			AddToolbarItem (toolbarItem, ToolbarView.DatePicker, "view_dateView.png", "Date Picker", "Provides for visually display and editing an NSDate instance.");
			AddToolbarItem (toolbarItem, ToolbarView.WrappingLabel, "view_multiline.png", "Wrapping Label", "Display static text that line wraps as needed.");
			AddToolbarItem (toolbarItem, ToolbarView.ScrollableTextView, "view_scrollable.png", "Scrollable Text View", "A text view enclosed in a scroll view. This configuration is suitable for UI elements typically used in inspectors.");
			AddToolbarItem (toolbarItem, ToolbarView.TextField, "view_textView.png", "Text Field", "Displays editable text");
			AddToolbarItem (toolbarItem, ToolbarView.PushButton, "view_button.png", "Push Button", "For use un window content areas. Use text, not images.");
			AddToolbarItem (toolbarItem, ToolbarView.Label, "view_label.png", "Label", "Display a static text.");
			AddToolbarItem (toolbarItem, ToolbarView.Search, "view_search.png", "Search Field", "A text field that is optimized for performing text-based searches");
			AddToolbarItem (toolbarItem, ToolbarView.ComboBox, "view_combo.png", "Combo Box", "Allows you to either enter text directly (as you would with NSTextField) or click the attached arrow at the right of the combo box to select from a displayed pop-up");
			AddToolbarItem (toolbarItem, ToolbarView.ImageBox, "view_image.png", "Image Box Control", "For use in window content areas or toolbar");

			AddToolbarItem (toolbarItem, ToolbarView.ComboBoxItem, defaultImage, "Combo Box Item", "Adds a sample item to a ComboBox view");
			AddToolbarItem (toolbarItem, ToolbarView.ScrollView, defaultImage, "Scroll View Control");
			AddToolbarItem (toolbarItem, ToolbarView.CustomView, defaultImage, "CustomView Control");
			AddToolbarItem (toolbarItem, ToolbarView.SegmentedControl, defaultImage, "Segmented Control");
			AddToolbarItem (toolbarItem, ToolbarView.Box, defaultImage, "Box Control");
			AddToolbarItem (toolbarItem, ToolbarView.TabView, defaultImage, "Tab View Control");
			AddToolbarItem (toolbarItem, ToolbarView.TabViewItem, defaultImage, "Tab View Item Control");


			toolbarData.Add (toolbarItem);

			Search ();

			foreach (var mod in InspectorContext.Current.Modules) {
				if (!mod.IsEnabled) {
					continue;
				}
				mod.Load (toolbarTabViewWrapper);
			}
		}

		void AddToolbarItem (CollectionHeaderItem item, ToolbarView type, string image, string label, string description = null)
		{
			item.Items.Add (new CollectionItem () { TypeOfView = type, Image = inspectorDelegate.GetImageResource (image), Label = label, Description = description ?? label });
		}

		NSSearchField toolbarSearchTextField;

		public void Search ()
		{
			if (string.IsNullOrEmpty (toolbarSearchTextField.StringValue)) {
				toolbarView.SetData (toolbarData);
			} else {
				List<CollectionHeaderItem> collectionHeaderItems = new List<CollectionHeaderItem> ();
				for (int i = 0; i < toolbarData.Count; i++) {
					var headerItem = new CollectionHeaderItem ();

					for (int j = 0; j < toolbarData[i].Items.Count; j++) {
						if (toolbarData[i].Items[j].Label.IndexOf (toolbarSearchTextField.StringValue, StringComparison.OrdinalIgnoreCase) > -1) {
							headerItem.Items.Add (toolbarData[i].Items[j]);
						}
					}

					if (headerItem.Items.Count > 0) {
						collectionHeaderItems.Add (headerItem);
					}
				}

				toolbarView.SetData (collectionHeaderItems);
			}
			toolbarView.ReloadData ();
		}

		public void GenerateTree (IWindowWrapper window, InspectorViewMode viewMode)
		{
			var nodeView = outlineView.SelectedNode as NodeView; 			var selectedRow = outlineView.RowForItem (nodeView);  			data = new NodeView (window.ContentView);
			inspectorDelegate.ConvertToNodes (window.ContentView, new MacNodeWrapper (data), viewMode);
			outlineView.SetData (data);  			var newPosition = outlineView.RowForItem (nodeView); 			if (newPosition == -1) { 				outlineView.SelectRow (selectedRow, true); 			} else { 				outlineView.SelectRow (newPosition, true); 			} 		}

		NSTextField resultMessage;

		void InvokeSelectedView ()
		{
			if (viewSelected == null) {
				return;
			}

			if (methodListView.SelectedItem is MethodTableViewItem itm) {
				//itm.MethodInfo
				var method = itm.MethodInfo;
				var parameters = method.GetParameters ();

				List<object> arguments = null;
				if (parameters.Count () > 0) {
					arguments = new List<object> ();
					foreach (var item in parameters) {
						arguments.Add (null);
					}
				}

				try {
					var response = method.Invoke (viewSelected.NativeObject, parameters);
					resultMessage.StringValue = response?.ToString () ?? "<null>";
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			};
		}

		void MethodListView_DoubleClick (object sender, EventArgs e) => InvokeSelectedView ();

		IViewWrapper viewSelected;

		public void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate, InspectorViewMode viewMode)
		{
			viewSelected = view;
			if (viewSelected != null) {
				propertyEditorPanel.Select (new object[] { inspectDelegate.GetWrapper (viewSelected, viewMode) });
			} else {
				propertyEditorPanel.Select (new object[0]);
			}

			methodListView.SetObject (view?.NativeObject, methodSearchView.StringValue);
			if (data != null && view != null) {
				var found = data.Search (view);
				if (found != null) {
					outlineView.FocusNode (found);
				}
			}
		}

		public void RemoveItem ()
		{
			if (outlineView.SelectedNode is NodeView nodeView) {
				RaiseDeleteItem?.Invoke (this, nodeView.Wrapper);
			}
		}

		protected override void Dispose (bool disposing)
		{
			methodListView.DoubleClick -= MethodListView_DoubleClick;
			base.Dispose (disposing);
		}
	}
}
