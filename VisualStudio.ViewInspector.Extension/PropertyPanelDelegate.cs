using System.Collections.Generic;
using AppKit;
using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Mac.Windows.Inspector;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Mac;

namespace VisualStudio.ViewInspector.Extension
{
    class CustomPropertyPanel : PropertyEditorPanel, IPropertyPanel
    {
        readonly IHostResourceProvider provider;
        readonly PropertyEditorProvider editorProvider;

        public CustomPropertyPanel(IHostResourceProvider provider) : base(provider)
        {
            this.provider = provider;
            editorProvider = new PropertyEditorProvider();

            TargetPlatform = new TargetPlatform(editorProvider)
            {
                SupportsCustomExpressions = true,
                SupportsMaterialDesign = true,
            };
        }

        public object NativeObject => this;
    }

    class PropertyPanelDelegate : IPropertyPanelDelegate
    {
        public IPropertyPanel CreatePropertyPanelView()
        {
            var propertyEditorPanel = new CustomPropertyPanel(new HostResource());
            return propertyEditorPanel;
        }

        public object GetNativePropertyPanelWrapper(INativeObject viewSelected)
        {
            object nativeObject = viewSelected.NativeObject;

            if (nativeObject is NSComboBox comboBox)
                return new PropertyPanelNSComboBox(comboBox);

            if (nativeObject is NSScrollView scrollView)
                return new PropertyPanelNSScrollView(scrollView);

            if (nativeObject is NSOutlineView outlineView)
                return new PropertyPanelNSOutlineView(outlineView);

            if (nativeObject is NSTableView tableView)
                return new PropertyPanelNSTableView(tableView);

            if (nativeObject is NSPopUpButton popUpButton)
                return new PropertyPanelNSPopupButton(popUpButton);

            if (nativeObject is NSStackView stackView)
                return new PropertyPanelNSStackView(stackView);

            if (nativeObject is NSImageView img)
                return new PropertyPanelNSImageView(img);

            if (nativeObject is NSBox box)
                return new PropertyPanelNSBox(box);

            if (nativeObject is NSButton btn)
                return new PropertyPanelNSButton(btn);

            if (nativeObject is NSTextView text)
                return new PropertyPanelNSTextView(text);

            if (nativeObject is NSTextField textfield)
                return new PropertyPanelNSTextField(textfield);

            if (nativeObject is NSWindow window)
                return new PropertyPanelNSWindow(window);

            if (nativeObject is NSView view)
                return new PropertyPanelNSView(view);

            if (nativeObject is NSLayoutConstraint constraint)
                return new PropertyPanelNSLayoutConstraint(constraint);

            //return nativeObject;
            return new PropertyPanelNSResponder(nativeObject as NSResponder);
        }
    }
}

