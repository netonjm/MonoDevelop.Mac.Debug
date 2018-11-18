using System;
using AppKit;
using FigmaSharp;
using MonoDevelop.Inspector.Mac;

namespace MonoDevelop.Inspector.Figma
{
    public class FigmaInspectorTabModule : IInspectorTabModule, IDisposable
    {
        const string DefaultToken = "TOKEN";
        const string DefaultFileId = "FILE_ID";
        const string DefaultWindowName = "FILE WINDOW NAME";
        const string DefaultContainerID = "FILE CONTAINER NAME";

        public FigmaInspectorTabModule() 
        {
        }

        public bool IsEnabled => true;

        NSTextField tokenTextField, documentTextField, windowTextField, windowContainerIdTextField;

        IInspectorWindow inspectorWindow;

        public void Load (IInspectorWindow inspectorWindow, ITabWrapper tab)
        {
            this.inspectorWindow = inspectorWindow;

            var toolbarTab = tab.NativeObject as NSTabView;
            var figmaStackView = NativeViewHelper.CreateVerticalStackView(translatesAutoresizingMaskIntoConstraints: true);

            var figmaTokenStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaTokenStackView);

            figmaTokenStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("Your personal access token:", translatesAutoresizingMaskIntoConstraints: true));
            tokenTextField = NativeViewHelper.CreateTextEntry(DefaultToken, translatesAutoresizingMaskIntoConstraints: true);
            figmaTokenStackView.AddArrangedSubview(tokenTextField);

            var figmaDocumentStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaDocumentStackView);
            figmaDocumentStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("Your File:", translatesAutoresizingMaskIntoConstraints: true));
            documentTextField = NativeViewHelper.CreateTextEntry(DefaultFileId, translatesAutoresizingMaskIntoConstraints: true);
            figmaDocumentStackView.AddArrangedSubview(documentTextField);

            var figmaWindowNameStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaWindowNameStackView);

            figmaWindowNameStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("Window Name:", translatesAutoresizingMaskIntoConstraints: true));
            windowTextField = NativeViewHelper.CreateTextEntry(DefaultWindowName, translatesAutoresizingMaskIntoConstraints: true);
            figmaWindowNameStackView.AddArrangedSubview(windowTextField);

            var figmaWindowContainerIdStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaWindowContainerIdStackView);


            figmaWindowContainerIdStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("Window Container Id:", translatesAutoresizingMaskIntoConstraints: true));
            windowContainerIdTextField = NativeViewHelper.CreateTextEntry(DefaultContainerID, translatesAutoresizingMaskIntoConstraints: true);
            figmaWindowContainerIdStackView.AddArrangedSubview(windowContainerIdTextField);

            var figmaCompute = new NSButton() { TranslatesAutoresizingMaskIntoConstraints = true };
            figmaStackView.AddArrangedSubview(figmaCompute);
            figmaCompute.Title = "Load in selected view";

            //Separator
            figmaStackView.AddArrangedSubview(new NSView());

            //////
            var tabFigmaPanel = new NSTabViewItem();
            tabFigmaPanel.View = figmaStackView;
            tabFigmaPanel.Label = "Figma";

            toolbarTab.Add(tabFigmaPanel);

            figmaCompute.Activated += FigmaCompute_Activated;
        }

        void FigmaCompute_Activated(object sender, EventArgs e)
        {
            FigmaEnvirontment.SetAccessToken(tokenTextField.StringValue);

            //InspectorContext.

            //if (selectedView.NativeObject is NSView currentView)
            //{
            //    var children = currentView.Subviews.ToList();
            //    foreach (var item in children)
            //    {
            //        item.RemoveFromSuperview();
            //    }

            //    currentView.LoadFigma(file, viewName, nodeName);
            //}
        }

        public void Dispose()
        {
            //figmaCompute.Activated -= FigmaCompute_Activated;
        }
    }
}
