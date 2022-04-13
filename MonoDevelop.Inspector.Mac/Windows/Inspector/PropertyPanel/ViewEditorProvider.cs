using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Common;

namespace MonoDevelop.Inspector.Mac
{
	public class PropertyEditorProvider
		: IEditorProvider
	{
		public PropertyEditorProvider(IResourceProvider resources = null)
		{
			this.resources = resources;
		}

		public PropertyEditorProvider(IObjectEditor editor)
		{
			editorCache.Add(editor.Target, editor);
		}

		public IReadOnlyDictionary<Type, ITypeInfo> KnownTypes
		{
			get;
		} = new Dictionary<Type, ITypeInfo>();

		public Task<IObjectEditor> GetObjectEditorAsync(object item)
		{
			if (editorCache.TryGetValue(item, out IObjectEditor cachedEditor))
				return Task.FromResult(cachedEditor);
			IObjectEditor editor = ChooseEditor(item);
			editorCache.Add(item, editor);
			return Task.FromResult(editor);
		}

		public async Task<IReadOnlyCollection<IPropertyInfo>> GetPropertiesForTypeAsync(ITypeInfo type)
		{
			Type realType = ReflectionEditorProvider.GetRealType(type);
			if (realType == null)
				return Array.Empty<IPropertyInfo>();
			return ReflectionEditorProvider.GetPropertiesForType(realType);
		}

		public Task<AssignableTypesResult> GetAssignableTypesAsync(ITypeInfo type, bool childTypes)
		{
			if (type == KnownTypes[typeof(CommonValueConverter)])
				return Task.FromResult(new AssignableTypesResult(new[] { type }));

			return Task.Run (() => {
				var types = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (a => a.GetTypes ()).AsParallel ()
					.Where (t => t.Namespace != null && !t.IsAbstract && !t.IsInterface && t.IsPublic && t.GetConstructor (Type.EmptyTypes) != null);

				Type realType = ReflectionEditorProvider.GetRealType (type);
				if (childTypes) {
					var generic = realType.GetInterface ("ICollection`1");
					if (generic != null) {
						realType = generic.GetGenericArguments ()[0];
					} else {
						realType = typeof (object);
					}
				}

				types = types.Where (t => realType.IsAssignableFrom (t));

				return new AssignableTypesResult (types.Select (t => {
					string asmName = t.Assembly.GetName ().Name;
					return new TypeInfo (new AssemblyInfo (asmName, isRelevant: asmName.StartsWith ("Xamarin")), t.Namespace, t.Name);
				}).ToList ());
			});
		}

		IObjectEditor ChooseEditor(object item)
		 => new ReflectionObjectEditor(item);

		public Task<object> CreateObjectAsync(ITypeInfo type)
		{
			var realType = Type.GetType($"{type.NameSpace}.{type.Name}, {type.Assembly.Name}");
			if (realType == null)
				return Task.FromResult<object>(null);

			return Task.FromResult(Activator.CreateInstance(realType));
		}

		public Task<IReadOnlyList<object>> GetChildrenAsync(object item)
		{
			return Task.FromResult<IReadOnlyList<object>>(Array.Empty<object>());
		}

		public Task<IReadOnlyDictionary<Type, ITypeInfo>> GetKnownTypesAsync(IReadOnlyCollection<Type> knownTypes)
		{
			return Task.FromResult<IReadOnlyDictionary<Type, ITypeInfo>>(new Dictionary<Type, ITypeInfo>());
		}

		readonly IResourceProvider resources;
		readonly Dictionary<object, IObjectEditor> editorCache = new Dictionary<object, IObjectEditor>();
	}
}