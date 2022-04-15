using System;
using System.Collections;
using System.Collections.Generic;
using AppKit;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.DesignerSupport;
using MonoDevelop.Ide;

namespace VisualStudio.ViewInspector.Extension
{
    //public class PropertyProvider : IPropertyProvider
    //{
    //    public bool SupportsObject(object obj)
    //    {
    //        return obj is NSView;
    //    }

    //    public object CreateProvider(object obj)
    //    {
    //        return obj;
    //    }
    //}

    class PadViewCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Visible = info.Enabled = true;

            var started = InspectorContext.Current.Started;
            info.Text = started ? "Stop View Inspection" : "Start View Inspection";
        }

        protected override void Run()
        {
            var started = InspectorContext.Current.Started;
            if (started)
                InspectorContext.Current.StopWatcher();
            else
                InspectorContext.Current.StartWatcher();
        }
    }

    //class SearchChildenViewCommandHandler : CommandHandler
    //{
    //    protected override void Update(CommandInfo info)
    //    {
    //        info.Visible = info.Enabled = true;
    //    }

    //    protected override void Run()
    //    {
    //        var rootWindow = IdeApp.Workbench.Window;
    //        var widget = rootWindow.Focus;
    //        var builder = new System.Text.StringBuilder();
    //        builder.AppendLine("Current Gtk Brothers states: ({widget} {IsFocus}|{HasFocus}|{Sensitive})");
    //        builder.AppendLine("==================================================================");

    //        var nWidget = widget.GetNativeWidget<NSView>();

    //        if (nWidget == null)
    //        {
    //            builder.AppendLine("No selected widget");
    //            LoggingService.LogInfo(builder.ToString());
    //            MonoDevelop.Ide.MessageService.ShowError("No selected widget");
    //            return;
    //        }
    //        if (nWidget.Superview == null)
    //        {
    //            builder.AppendLine("Widget has no parent");
    //            LoggingService.LogInfo(builder.ToString());
    //            MonoDevelop.Ide.MessageService.ShowError("Widget has no parent");
    //            return;
    //        }

    //        SearchSelectedViewCommandHandler.AppendChildStatus(builder, nWidget.Superview, 2);
    //        foreach (var item in nWidget.Superview.Subviews)
    //        {
    //            SearchSelectedViewCommandHandler.AppendChildStatus(builder, item, 2);
    //        }
    //        LoggingService.LogInfo(builder.ToString());
    //    }
    //}

    //class SearchSelectedViewCommandHandler : CommandHandler
    //{
    //    protected override void Update(CommandInfo info)
    //    {
    //        info.Visible = info.Enabled = true;
    //    }

    //    public static void AppendChildStatus(System.Text.StringBuilder builder, NSView current, int level)
    //    {
    //        var line = new string('-', level);
    //        var spaces = $"{line}> {current} ({current.Window.FirstResponder == current}";//|{current.HasFocus}|{current.Sensitive})";
    //        builder.AppendLine(spaces);
    //    }

    //    protected override void Run()
    //    {
    //        var rootWindow = IdeApp.Workbench.Window;
    //        var widget = rootWindow.Focus;
    //        var builder = new System.Text.StringBuilder();
    //        builder.AppendLine("Current Gtk Hierarchy states: ({widget} {IsFocus}|{HasFocus}|{Sensitive})");
    //        builder.AppendLine("==================================================================");

    //        if (widget == null)
    //        {
    //            builder.AppendLine("No selected widget");
    //            LoggingService.LogInfo(builder.ToString());
    //            return;
    //        }

    //        var hierarchy = new List<NSView>();
    //        GoParent(widget, hierarchy);

    //        for (int i = hierarchy.Count - 1; i >= 0; i--)
    //        {
    //            var current = hierarchy[i];
    //            AppendChildStatus(builder, current, hierarchy.Count - i + 1);
    //        }
    //        LoggingService.LogInfo(builder.ToString());
    //    }

    //    void GoParent(AppKit.NSView widget, List<NSView> widgets)
    //    {
    //        widgets.Add(widget);

    //        if (widget.Superview != null)
    //        {
    //            GoParent(widget.Superview, widgets);
    //        }
    //    }
    //}

}
