﻿// This file has been autogenerated from a class added in the UI designer.

namespace VisualStudio.ViewInspector.Abstractions
{
    public interface IMenu : INativeObject
    {
        void InsertItem(IMenuItem item, int v);
    }

    public interface IMenuItem : INativeObject
    {
        void SetTitle(string v);
    }
}