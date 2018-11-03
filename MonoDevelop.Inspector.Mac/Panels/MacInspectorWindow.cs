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
        const int ScrollViewSize = 150;
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
            outlineViewScrollView.HeightAnchor.ConstraintEqualToConstant (ScrollViewSize).Active = true;

            stackView.AddArrangedSubview(outlineViewScrollView);

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

            //===================

            //Method list view
            methodListView = new MethodListView();
            methodListView.AddColumn(new NSTableColumn("col") { Title = "Methods" });
            methodListView.DoubleClick += MethodListView_DoubleClick;
           
            scrollView = new ScrollContainerView (methodListView);
            stackView.AddArrangedSubview(scrollView);

            var titleContainter = NativeViewHelper.CreateHorizontalStackView();
            stackView.AddArrangedSubview(titleContainter);

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

            var tabPropertyPanel = new NSTabViewItem();
            tabPropertyPanel.View = propertyEditorPanel;
            tabPropertyPanel.Label = "Properties";

            var tabMethod = new NSTabViewItem();
            tabMethod.View = scrollView;
            tabMethod.Label = "Methods";

            tabView = new NSTabView();
            tabView.Add(tabPropertyPanel);
            tabView.Add(tabMethod);
            stackView.AddArrangedSubview(tabView as NSView);
        }

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
            methodListView.SetObject (view.NativeView);
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
