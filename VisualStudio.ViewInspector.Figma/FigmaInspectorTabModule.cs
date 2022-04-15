using System;
using AppKit;
using FigmaSharp;
using MonoDevelop.Inspector.Mac;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using FigmaSharp.Views.Cocoa;
using VisualStudio;
using VisualStudio.ViewInspector;
using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Modules;

namespace VisualStudio.ViewInspector.Figma
{
    class FigmaConfig
    {
        public string Token { get; set; } = "TOKEN";
        public string File { get; set; } = "FILE_ID";
        public string NodeName { get; set; } = "FILE WINDOW NAME";
        public string ViewName { get; set; } = "FILE CONTAINER NAME";
    }

    public class FigmaInspectorTabModule : IInspectorTabModule, IDisposable
    {
        public FigmaInspectorTabModule() 
        {
        }

        public bool IsEnabled => true;

        NSTextField tokenTextField, fileTextField, nodeTextField, viewTextField;

        IInspectorWindow inspectorWindow;

        FigmaConfig ReadConfig (string filePath)
        {
            Console.WriteLine("Loading config from: {0}", filePath);
            if (File.Exists(filePath))
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<FigmaConfig>(File.ReadAllText(filePath));
                    Console.WriteLine("DONE.");
                    return config;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return new FigmaConfig();
        }

        void WriteConfig(FigmaConfig figmaConfig, string filePath)
        {
            Console.WriteLine("Writting config in: {0}", filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            try
            {
                File.WriteAllText (filePath, JsonConvert.SerializeObject(figmaConfig));
                Console.WriteLine("DONE.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        string configFilePath;
        FigmaConfig config;

        void IInspectorTabModule.Load(IInspectorWindow inspectorWindow, ITab tab)
        {
            this.inspectorWindow = inspectorWindow;
            var path = Path.GetDirectoryName(GetType().Assembly.Location);
            configFilePath = Path.Combine(path, "user.cfg");
            config = ReadConfig(configFilePath);

            var toolbarTab = tab.NativeObject as NSTabView;
            var figmaStackView = NativeViewHelper.CreateVerticalStackView(translatesAutoresizingMaskIntoConstraints: true);

            var figmaTokenStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaTokenStackView);

            figmaTokenStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("Your personal access token:", translatesAutoresizingMaskIntoConstraints: true));
            tokenTextField = NativeViewHelper.CreateTextEntry(config.Token, translatesAutoresizingMaskIntoConstraints: true);
            figmaTokenStackView.AddArrangedSubview(tokenTextField);
            tokenTextField.Activated += DataChanged ;

            var figmaDocumentStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaDocumentStackView);
            figmaDocumentStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("File:", translatesAutoresizingMaskIntoConstraints: true));
            fileTextField = NativeViewHelper.CreateTextEntry(config.File, translatesAutoresizingMaskIntoConstraints: true);
            fileTextField.Activated += DataChanged ;
            figmaDocumentStackView.AddArrangedSubview(fileTextField);

            var figmaWindowNameStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaWindowNameStackView);

            figmaWindowNameStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("Node Name:", translatesAutoresizingMaskIntoConstraints: true));
            nodeTextField = NativeViewHelper.CreateTextEntry(config.NodeName, translatesAutoresizingMaskIntoConstraints: true);
            nodeTextField.Activated += DataChanged;

            figmaWindowNameStackView.AddArrangedSubview(nodeTextField);

            var figmaWindowContainerIdStackView = NativeViewHelper.CreateHorizontalStackView(translatesAutoresizingMaskIntoConstraints: true);
            figmaStackView.AddArrangedSubview(figmaWindowContainerIdStackView);

            figmaWindowContainerIdStackView.AddArrangedSubview(NativeViewHelper.CreateLabel("View Name:", translatesAutoresizingMaskIntoConstraints: true));
            viewTextField = NativeViewHelper.CreateTextEntry(config.ViewName, translatesAutoresizingMaskIntoConstraints: true);
            viewTextField.Activated += DataChanged ;

            figmaWindowContainerIdStackView.AddArrangedSubview(viewTextField);

            figmaCompute = new NSButton() { TranslatesAutoresizingMaskIntoConstraints = true };
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

        void DataChanged (object sender, EventArgs e)
        {
            config.File = fileTextField.StringValue;
            config.Token = tokenTextField.StringValue;
            config.ViewName = viewTextField.StringValue;
            config.NodeName = nodeTextField.StringValue;
            WriteConfig(config, configFilePath);
        }

        NSButton figmaCompute;

        void FigmaCompute_Activated(object sender, EventArgs e)
        {
            FigmaSharp.AppContext.Current.SetAccessToken(tokenTextField.StringValue);

            if (InspectorContext.Current.SelectedView?.NativeObject is NSView currentView)
            {
                var children = currentView.Subviews.ToList();
                foreach (var item in children)
                {
                    item.RemoveFromSuperview();
                }

				var list = new List<ImageView> ();

    //            var service = 

    //            currentView.LoadFigmaFromUrlFile(fileTextField.StringValue, out list, viewTextField.StringValue, nodeTextField.StringValue);
				//list.Load (fileTextField.StringValue);
			}
        }

        public void Dispose()
        {
            figmaCompute.Activated -= FigmaCompute_Activated;
        }
    }
}
