﻿// This file has been autogenerated from a class added in the UI designer.

using AppKit;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class MenuItem : IMenuItem
    {
        private NSMenuItem submenu;

        public MenuItem(NSMenuItem submenu)
        {
            this.submenu = submenu;
        }

        public void SetTitle(string title)
        {
            submenu.Title = title;
        }

        public object NativeObject => submenu;
    }
}