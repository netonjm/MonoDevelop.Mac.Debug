using System.Reflection;
using AppKit;

namespace MonoDevelop.Mac.Debug.Services
{
	public static class ResourceService
	{
		public static NSImage GetNSImage (string resource)
		{
			try
			{
				var assembly = Assembly.GetAssembly(typeof(InspectorManager));
				var resources = assembly.GetManifestResourceNames();
				using (var stream = assembly.GetManifestResourceStream(resource))
				{
					return NSImage.FromStream(stream);
				}
			}
			catch (System.Exception ex)
			{
				return null;
			}
		}
	}
}
