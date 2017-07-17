using Mono.Cecil;
using System.Linq;

namespace MonoDevelop.Mac.Injector
{
	partial class MainClass
	{
		public class AppDebuggerContext
		{
			readonly AssemblyDefinition assemblyDefinition;
			readonly ModuleDefinition mainModule;
			readonly Mono.Collections.Generic.Collection<MethodDefinition> methods;
			public AppDebuggerContext (string assemblyPath)
			{
				assemblyDefinition = AssemblyDefinition.ReadAssembly (assemblyPath);
				mainModule = assemblyDefinition.MainModule;

				var types = mainModule.Types;
				var appDelegateOverride = types.FirstOrDefault (s => s.Name == "MagicDebug");
				methods = appDelegateOverride.Methods;
			}

			public MethodDefinition GetMethodDefinition ()
			{
				return methods.FirstOrDefault (s => s.Name == "Add");
			}

		}

	}
}
