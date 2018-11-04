using System;
using CoreGraphics;
using AppKit;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Tests;
using Xamarin.PropertyEditing.Themes;
using Foundation;

namespace MonoDevelop.Inspector.Mac
{
    class InspectorWindow : MacWindowWrapper, IInspectorWindow
    {
        const ushort DeleteKey = 51;

        public event EventHandler<IViewWrapper> RaiseFirstResponder;
        public event EventHandler<IViewWrapper> RaiseDeleteItem;

        public const int ButtonWidth = 30;
        const int margin = 10;
        const int ScrollViewSize = 240;
        readonly MockEditorProvider editorProvider;
        readonly MockResourceProvider resourceProvider;
        readonly MockBindingProvider bindingProvider;

        PropertyEditorPanel propertyEditorPanel;
        NSLayoutConstraint constraint;

        readonly NSView contentView;
        MethodListView methodListView;
        public OutlineView outlineView { get; private set; }
       
        NSTabView tabView;

        readonly IInspectDelegate inspectorDelegate;

        public InspectorWindow (IInspectDelegate inspectorDelegate, CGRect frame) : base (frame, NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false)
        {
            this.inspectorDelegate = inspectorDelegate;
            ShowsToolbarButton = false;
            MovableByWindowBackground = false;
            
            propertyEditorPanel = new PropertyEditorPanel();
          
            editorProvider = new MockEditorProvider();
            resourceProvider = new MockResourceProvider();
            bindingProvider = new MockBindingProvider();

            propertyEditorPanel.TargetPlatform = new TargetPlatform(editorProvider, resourceProvider, bindingProvider)
            {
                SupportsCustomExpressions = true,
                SupportsMaterialDesign = true,
            };

            var currentThemeStyle = NSUserDefaults.StandardUserDefaults.StringForKey ("AppleInterfaceStyle") ?? "Light";
            PropertyEditorPanel.ThemeManager.Theme = currentThemeStyle == "Dark" ? PropertyEditorTheme.Dark :  PropertyEditorTheme.Light;

            contentView = ContentView;

            var stackView = NativeViewHelper.CreateVerticalStackView(margin);
            contentView.AddSubview (stackView);

            stackView.LeftAnchor.ConstraintEqualToAnchor(contentView.LeftAnchor, margin).Active = true;
            stackView.RightAnchor.ConstraintEqualToAnchor(contentView.RightAnchor, -margin).Active = true;
            stackView.TopAnchor.ConstraintEqualToAnchor(contentView.TopAnchor, margin).Active = true;

            constraint = stackView.HeightAnchor.ConstraintEqualToConstant(contentView.Frame.Height-margin * 2);
            constraint.Active = true;
            outlineView = new OutlineView ();
            var outlineViewScrollView = new ScrollContainerView (outlineView);
           
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

            var toolbarTab = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false };
            toolbarTab.WantsLayer = true;
            toolbarTab.Layer.BackgroundColor = NSColor.Red.CGColor;

            stackView.AddArrangedSubview(toolbarTab);

            toolbarTab.LeftAnchor.ConstraintEqualToAnchor(contentView.LeftAnchor, margin).Active = true;
            toolbarTab.RightAnchor.ConstraintEqualToAnchor(contentView.RightAnchor, -margin).Active = true;
            toolbarTab.TopAnchor.ConstraintEqualToAnchor(contentView.TopAnchor, margin).Active = true;
            toolbarTab.HeightAnchor.ConstraintEqualToConstant(ScrollViewSize).Active = true;

            var outlineTabItem = new NSTabViewItem();
            outlineTabItem.Label = "View Hierarchy";
            outlineTabItem.View.AddSubview(outlineViewScrollView);
            outlineViewScrollView.LeftAnchor.ConstraintEqualToAnchor(outlineTabItem.View.LeftAnchor, 0).Active = true;
            outlineViewScrollView.RightAnchor.ConstraintEqualToAnchor(outlineTabItem.View.RightAnchor, 0).Active = true;
            outlineViewScrollView.TopAnchor.ConstraintEqualToAnchor(outlineTabItem.View.TopAnchor, 0).Active = true;
            outlineViewScrollView.BottomAnchor.ConstraintEqualToAnchor(outlineTabItem.View.BottomAnchor, 0).Active = true;

            toolbarTab.Add(outlineTabItem);

            //===================

            //Method list view
            methodListView = new MethodListView();
            methodListView.AddColumn(new NSTableColumn("col") { Title = "Methods" });
            methodListView.DoubleClick += MethodListView_DoubleClick;

            scrollView = new ScrollContainerView (methodListView);

            var titleContainter = NativeViewHelper.CreateHorizontalStackView();
            //titleContainter.WantsLayer = true;
            //titleContainter.Layer.BackgroundColor = NSColor.Gray.CGColor;

            methodSearchView = new NSSearchField() { TranslatesAutoresizingMaskIntoConstraints = false };
            titleContainter.AddArrangedSubview(methodSearchView);
            methodSearchView.WidthAnchor.ConstraintEqualToConstant(180).Active =true;


            IImageWrapper invokeImage = inspectorDelegate.GetImageResource("execute-16.png");
            IButtonWrapper invokeButton = inspectorDelegate.GetImageButton(invokeImage);
            invokeButton.SetTooltip("Invoke Method!");
            invokeButton.SetWidth(ButtonWidth);

            titleContainter.AddArrangedSubview((NSView) invokeButton.NativeObject);
            invokeButton.Pressed += (s, e) => InvokeSelectedView();

            titleContainter.AddArrangedSubview(NativeViewHelper.CreateLabel ("Result: "));

            resultMessage = NativeViewHelper.CreateLabel ("");
            resultMessage.LineBreakMode = NSLineBreakMode.ByWordWrapping;

            titleContainter.AddArrangedSubview(resultMessage);

            var methodStackPanel = NativeViewHelper.CreateVerticalStackView();
            methodStackPanel.AddArrangedSubview(titleContainter);
            titleContainter.LeftAnchor.ConstraintEqualToAnchor(methodStackPanel.LeftAnchor, 0).Active = true;
            titleContainter.RightAnchor.ConstraintEqualToAnchor(methodStackPanel.RightAnchor, 0).Active = true;

            methodStackPanel.AddArrangedSubview(scrollView);
            /////

            var tabPropertyPanel = new NSTabViewItem();
            tabPropertyPanel.View = propertyEditorPanel;
            tabPropertyPanel.Label = "Properties";

            var tabMethod = new NSTabViewItem();

            tabMethod.View.AddSubview(methodStackPanel);
            methodStackPanel.LeftAnchor.ConstraintEqualToAnchor(tabMethod.View.LeftAnchor, 0).Active = true;
            methodStackPanel.RightAnchor.ConstraintEqualToAnchor(tabMethod.View.RightAnchor, 0).Active = true;
            methodStackPanel.TopAnchor.ConstraintEqualToAnchor(tabMethod.View.TopAnchor, 0).Active = true;
            methodStackPanel.BottomAnchor.ConstraintEqualToAnchor(tabMethod.View.BottomAnchor, 0).Active = true;

            tabMethod.Label = "Methods";

            tabView = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false } ;
            tabView.Add(tabPropertyPanel);
            tabView.Add(tabMethod);
            stackView.AddArrangedSubview(tabView as NSView);

            tabView.LeftAnchor.ConstraintEqualToAnchor(stackView.LeftAnchor, 0).Active = true;
            tabView.RightAnchor.ConstraintEqualToAnchor(stackView.RightAnchor, 0).Active = true;

            methodSearchView.Activated += (sender, e) =>
            {
                if (viewSelected != null) {
                    methodListView.SetObject(viewSelected.NativeView, methodSearchView.StringValue);
                }
            };
        }
        NSSearchField methodSearchView;
        ScrollContainerView scrollView;
        NodeView data;

        public void GenerateTree(IWindowWrapper window, InspectorViewMode viewMode)
        {
            data = new NodeView(window.ContentView);
            inspectorDelegate.ConvertToNodes(window.ContentView, new MacNodeWrapper (data), viewMode);
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
                    var response = method.Invoke(viewSelected.NativeView, parameters);
                    resultMessage.StringValue = response?.ToString() ?? "<null>";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
        }

        void MethodListView_DoubleClick(object sender, EventArgs e) => InvokeSelectedView();

        IViewWrapper viewSelected;

        public void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate, InspectorViewMode viewMode)
        {
            viewSelected = view;
            propertyEditorPanel.Select(new object[] { inspectDelegate.GetWrapper (viewSelected, viewMode) });
            methodListView.SetObject (view.NativeView, methodSearchView.StringValue);
            if (data != null) {
                var found = data.Search(view);
                if (found != null) {
                    outlineView.FocusNode (found);
                }
            }
        }

        public void RemoveItem()
        {
            if (outlineView.SelectedNode is NodeView nodeView)
            {
                RaiseDeleteItem?.Invoke(this, nodeView.View);
            }
        }

        protected override void Dispose(bool disposing)
        {
            methodListView.DoubleClick -= MethodListView_DoubleClick;
            base.Dispose(disposing);
        }
    }
}
