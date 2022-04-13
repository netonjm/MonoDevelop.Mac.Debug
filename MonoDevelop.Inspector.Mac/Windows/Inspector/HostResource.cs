using AppKit;
using Xamarin.PropertyEditing.Mac;

namespace MonoDevelop.Inspector.Mac
{
    class HostResource : IHostResourceProvider
    {
        public static bool IsLightAppearance(NSAppearance appearance)
        {
            var appearanceName = appearance?.Name;
            if (appearanceName == NSAppearance.NameVibrantDark ||
                appearanceName == NSAppearance.NameAccessibilityHighContrastVibrantDark ||
                appearanceName == NSAppearance.NameDarkAqua ||
                appearanceName == NSAppearance.NameAccessibilityHighContrastDarkAqua)
                return false;

            return true;
        }

        public static bool IsDarkAppearance(NSAppearance appearance)
            => !IsLightAppearance(appearance);

        public static bool IsLightAppearance(INSAppearanceCustomization appearanceCustomization)
            => IsLightAppearance(appearanceCustomization?.EffectiveAppearance);

        public static bool IsDarkAppearance(INSAppearanceCustomization appearanceCustomization)
            => !IsLightAppearance(appearanceCustomization);

        public NSColor GetNamedColor(string name)
        {
            switch (name)
            {
                case NamedResources.Checkerboard0Color:
                    return IsDarkAppearance(NSApplication.SharedApplication.EffectiveAppearance) ?
                         NSColor.FromRgb(38, 38, 38) :
                         NSColor.FromRgb(255, 255, 255);

                case NamedResources.Checkerboard1Color:
                    return IsDarkAppearance(NSApplication.SharedApplication.EffectiveAppearance) ?
                         NSColor.FromRgb(0, 0, 0) :
                         NSColor.FromRgb(217, 217, 217);
                case NamedResources.DescriptionLabelColor:
                    return NSColor.SecondaryLabelColor;
                case NamedResources.ForegroundColor:
                    return NSColor.LabelColor;
                case NamedResources.PadBackgroundColor:
                    return IsDarkAppearance(NSApplication.SharedApplication.EffectiveAppearance) ?
                    NSColor.FromRgba(red: 0.12f, green: 0.12f, blue: 0.12f, alpha: 1.00f) :
                    NSColor.White;
                case NamedResources.PanelTabBackground:
                    return IsDarkAppearance(NSApplication.SharedApplication.EffectiveAppearance) ?
                       NSColor.FromRgba(red: 0.16f, green: 0.16f, blue: 0.16f, alpha: 1.00f) :
                       NSColor.FromRgba(red: 0.98f, green: 0.98f, blue: 0.98f, alpha: 1.00f);
                case NamedResources.TabBorderColor:
                    return IsDarkAppearance(NSApplication.SharedApplication.EffectiveAppearance) ?
                        NSColor.FromRgba(255, 255, 255, 0) :
                         NSColor.FromRgba(0, 0, 0, 25);

                case NamedResources.ValueBlockBackgroundColor:
                    return IsDarkAppearance(NSApplication.SharedApplication.EffectiveAppearance) ?
                       NSColor.FromRgba(255, 255, 255, 25) :
                        NSColor.FromRgba(0, 0, 0, 20);
                case NamedResources.FrameBoxButtonBackgroundColor:
                    return IsDarkAppearance(NSApplication.SharedApplication.EffectiveAppearance) ?
                      NSColor.FromRgb(0.38f, 0.55f, 0.91f) :
                        NSColor.FromRgb(0.36f, 0.54f, 0.90f);
            }
            return NSColor.FromName(name);
        }

        public NSFont GetNamedFont(string name, ObjCRuntime.nfloat fontSize)
        {
            return NSFont.SystemFontOfSize(NSFont.SystemFontSize);
        }

        public NSImage GetNamedImage(string name)
        {
            return new NSImage();
        }

        public NSAppearance GetVibrantAppearance(NSAppearance appearance)
        {
            return appearance;
        }
    }
}
