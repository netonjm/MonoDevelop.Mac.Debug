﻿// This file has been autogenerated from a class added in the UI designer.


using VisualStudio.ViewInspector.Abstractions;

namespace VisualStudio.ViewInspector.Modules
{
    public interface IInspectorTabModule
    {
        bool IsEnabled { get; }
        void Load (IInspectorWindow inspectorWindow, ITab tab);
    }
}
