using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using System.Linq;
using System.Collections.Generic;

namespace MonoDevelop.Mac.Injector
{
	partial class MainClass
	{
		public class TargetAppContext
		{
			readonly AssemblyDefinition assemblyDefinition;
			public readonly ModuleDefinition mainModule;
			public readonly ModuleDefinition debugModule;

			const string XamarinMacDirectory = "/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono/Xamarin.Mac";

			string macDebugDll;

			public TargetAppContext (string assemblyPath)
			{

				if (!Directory.Exists (XamarinMacDirectory)) {
					throw new DirectoryNotFoundException (XamarinMacDirectory);
				}

				var currentPath = Path.GetDirectoryName (GetType ().Assembly.Location);
				macDebugDll = Path.Combine (currentPath, "MonoDevelop.Mac.Debug.dll");

				var resolver = new DefaultAssemblyResolver ();
				resolver.AddSearchDirectory (XamarinMacDirectory);
				resolver.AddSearchDirectory (currentPath);
				assemblyDefinition = AssemblyDefinition.ReadAssembly (assemblyPath, new ReaderParameters { AssemblyResolver = resolver });

				mainModule = assemblyDefinition.MainModule;

				var assDebug = AssemblyDefinition.ReadAssembly (macDebugDll);
				debugModule = assDebug.MainModule;
				foreach (var item in assDebug.Modules) {
					assemblyDefinition.Modules.Add (item);
				}

			}

			public MethodDefinition GetInitializeMethodDefinition ()
			{
				var types = mainModule.Types;
				var appDelegateOverride = types.FirstOrDefault (s => s.Name == "AppDelegate");
				var methods = appDelegateOverride.Methods;
				return methods.FirstOrDefault (s => s.Name == "DidFinishLaunching");
			}

			public void Example ()
			{
				
				//var debuggerLibrary = "/Users/josemedranojimenez/CloudStation/Projects/profiler/external/MonoDevelop.Mac.Debug/MonoDevelop.Mac.Debug/bin/Debug/MonoDevelop.Mac.Debug.dll";
				//var assembly = Assembly.LoadFile (debuggerLibrary);
				//var magicDebugType = assembly.GetType ("MonoDevelop.Mac.Debug.ViewDebuggerContext");
				//var methodInfo = magicDebugType.GetMethod ("Attach", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				Inject (0);

				//methodInfo = magicDebugType.GetMethod ("Write", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				//Inject (methodInfo, 3);
			}

			public List<MemberReference> GetMemberReferenceByFullname (string fullname) {
				List<MemberReference> elements = new List<MemberReference> ();
				foreach (var mosule in assemblyDefinition.Modules) {
					elements.AddRange (mosule.GetMemberReferences ().Where (s => s.FullName == fullname));
				};
				return elements;
			}

			public List<TypeDefinition> GetTypeRecu (string fullname)
			{
				List<TypeDefinition> elements = new List<TypeDefinition> ();
				foreach (var mosule in assemblyDefinition.Modules) {
					var types = mosule.GetTypes ().ToArray ();

					elements.AddRange (types.Where (s => s.FullName == fullname));
				};
				return elements;
			}

			public void Inject (int line)
			{
				var initializeMethod = GetInitializeMethodDefinition ();
				var initProc = initializeMethod.Body.GetILProcessor ();
				var instructions = initProc.Body.Instructions;

				var sharedApplication = GetMemberReferenceByFullname ("AppKit.NSApplication AppKit.NSApplication::get_SharedApplication()")
					.FirstOrDefault () as MethodReference;

				//Get Windows
				TypeReference NsapplicationType;
				assemblyDefinition.MainModule.TryGetTypeReference ("AppKit.NSApplication", out NsapplicationType);

				var NsapplicationTypeResolved = NsapplicationType.Resolve ();
				var nsapplimethos = NsapplicationTypeResolved.Methods;
				var firstOrDefaultWindows = NsapplicationTypeResolved.Methods.FirstOrDefault (s => s.Name == "get_Windows");
				var resolved = firstOrDefaultWindows.Resolve ();

				var instanceMethod = new GenericInstanceMethod (firstOrDefaultWindows);

				//var getWindows = GetMemberReferenceByFullname ("AppKit.NSWindow[] AppKit.NSApplication::get_Windows()")
					//.FirstOrDefault () as MethodReference;

				// IEnumerable OfType

				TypeReference nsWindow;
				assemblyDefinition.MainModule.TryGetTypeReference ("AppKit.NSWindow", out nsWindow);
			

				var collection = GetMemberReferenceByFullname ("System.Collections.Generic.IEnumerable`1<!!0> System.Linq.Enumerable::OfType(System.Collections.IEnumerable)")
					.FirstOrDefault () as MethodReference;
				var genericInstanceMethod = new GenericInstanceMethod (collection);
				genericInstanceMethod.GenericArguments.Add (nsWindow);

				////////////////////// First of Default

				TypeReference IEnumea;
				assemblyDefinition.MainModule.TryGetTypeReference ("System.Linq.Enumerable", out IEnumea);
				var definition = IEnumea.Resolve ();
				var firstOrDefaultMethod = definition.Methods.Where (s => s.Name == "FirstOrDefault").FirstOrDefault ();

				var genericInstanceMethodFirst = new GenericInstanceMethod (firstOrDefaultMethod);
				genericInstanceMethodFirst.GenericArguments.Add (nsWindow);

				// all
				TypeReference viewDebuggerContext = GetTypeRecu ("MonoDevelop.Mac.Debug.ViewDebuggerContext").FirstOrDefault ();

				TypeReference attachCallTypeReference = assemblyDefinition.MainModule.Types.FirstOrDefault (s => s.Name == "ViewDebuggerContext");

				var attachCallTypeReferenceResolved = viewDebuggerContext.Resolve ();
				var attach = attachCallTypeReferenceResolved.Methods.Where (s => s.Name == "Attach").FirstOrDefault ();

				var attachInstance = new GenericInstanceMethod (attach);
				attachInstance.GenericArguments.Add (nsWindow);

				//var call = GetMemberReferenceByFullname ("System.Void MonoDevelop.Mac.Debug.ViewDebuggerContext::Attach(AppKit.NSWindow)")
				//.FirstOrDefault () as MethodReference;

				//First of default
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Nop));
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Call,sharedApplication));
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Callvirt, instanceMethod));
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Call, genericInstanceMethod));
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Call, genericInstanceMethodFirst));
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Stloc_0));
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Ldloc_0));
				initProc.InsertBefore (initProc.Body.Instructions[line++], initProc.Create (Mono.Cecil.Cil.OpCodes.Call, attachInstance));

				foreach (var module in assemblyDefinition.Modules) {
					
					module.Import (NsapplicationType);
					module.Import (attachInstance);
					module.Import (firstOrDefaultWindows);
				}

					
				Console.WriteLine ("Injected");
			}

			public Assembly GenerateAssembly ()
			{
				try {
					// Validate we've loaded the main executable first..
					if (this.assemblyDefinition == null)
						return null;
					
					using (MemoryStream mStream = new MemoryStream ()) {
						// Write the edited data to the memory stream..
						this.assemblyDefinition.Write (mStream);

						// Load the new assembly from the memory stream buffer..
						return Assembly.Load (mStream.GetBuffer ());
					}
				}
				catch {
					return null;
				}
			}

			public void Run ()
			{
				//var elements = loadFile (macDebugDll);

				var generateAssembly = GenerateAssembly ();


				//AppDomain.CurrentDomain.Load (generateAssembly);
				//generateAssembly.LoadModule ("", elements);
				if (generateAssembly != null) {
					try {
						// Get the main class type..
						var m_vMainType = generateAssembly.GetType ("XamarinProfiler.Mac.MainClass");

						// Create the constructor call..
						var constructor = m_vMainType.GetConstructor (new Type[] { }).Invoke (null);

						// Obtain the main run method and invoke it..
						var method = m_vMainType.GetMethod ("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
						object[] parametersArray = new object[] { new string[0] };

					
						method.Invoke (constructor, parametersArray);

						return;
					}
					catch (Exception ex) {
						Console.WriteLine (ex.Message);
					}
				}
				// Loads the content of a file to a byte array. 
			
				byte[] loadFile (string filename)
				{
					FileStream fs = new FileStream (filename, FileMode.Open);
					byte[] buffer = new byte[(int)fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					fs.Close ();

					return buffer;
				}
			}
		}
	}
}
