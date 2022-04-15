using System;
using AppKit;
using Foundation;

namespace VisualStudio.ViewInspector.Mac.TouchBar
{
    public class ColorPickerDelegate : TouchBarBaseDelegate
    {
        static readonly string BaseId = "ColorPickerDelegate";
        readonly string AppleColorBarId = GetId ("AppleColorBarId");
        readonly string SystemColorBarId = GetId("SystemColorBarId");
        readonly string CrayonsColorBarId = GetId("CrayonsColorBarId");
        readonly string TextColorBarId = GetId("TextColorBarId");
        readonly string StrokeColorBarId = GetId("StrokeColorBarId");

        static string GetId (string id)
        {
            return string.Format("{0}.{1}", BaseId, id);
        }

        public ColorPickerDelegate()
        {
        }

        public bool AllowCustomization { get; internal set; } = true;
        public override string[] Identifiers {
            get {
                return new string[] {
                    AppleColorBarId, SystemColorBarId, CrayonsColorBarId, TextColorBarId, StrokeColorBarId
                };
            }
        }

        public override NSTouchBarItem MakeItem(NSTouchBar touchBar, string identifier)
        {
            string pickerName = "Apple";

            if (identifier == SystemColorBarId)
            {
                pickerName = "System";
            }
            else if (identifier == CrayonsColorBarId)
            {
                pickerName = "Crayons";
            }
            else if (identifier == TextColorBarId)
            {
                pickerName = "Text";
            }
            else if (identifier == StrokeColorBarId)
            {
                pickerName = "Stroke";
            }
            return CreateColorPicker(identifier, pickerName);
        }

        NSColorPickerTouchBarItem CreateColorPicker(string identifier, string listName)
        {
            NSColorPickerTouchBarItem item;
            switch (listName)
            {
                case "Text":
                    item = NSColorPickerTouchBarItem.CreateTextColorPicker(identifier);
                    break;
                case "Stroke":
                    item = NSColorPickerTouchBarItem.CreateStrokeColorPicker(identifier);
                    break;
                default:
                    item = NSColorPickerTouchBarItem.CreateColorPicker(identifier, NSImage.ImageNamed(NSImageName.TouchBarColorPickerFill));
                    item.ColorList = NSColorList.ColorListNamed(listName);
                    item.ShowsAlpha = true;
                    break;
            }
            item.Activated += (sender, e) => OnColorChange(item.CustomizationLabel, item.Color);
            return item;
        }

        void OnColorChange(string name, NSColor color)
        {
            mainView.WantsLayer = true;

            // Prevent crashes when asking for components
            color = color.UsingColorSpace(NSColorSpace.CalibratedRGB);

            mainView.Layer.BackgroundColor = color.CGColor;

            Console.WriteLine("Color changed on {0} ({1}, {2}, {3}, {4})", name, color.RedComponent, color.GreenComponent, color.BlueComponent, color.AlphaComponent);
        }
    }
}
