using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Common;
using Xamarin.PropertyEditing.Reflection;

namespace MonoDevelop.Mac.Debug
{
	public class MockEditorProvider
		: IEditorProvider
	{
		public static readonly TargetPlatform MockPlatform = new TargetPlatform(new MockEditorProvider());

		public MockEditorProvider(IResourceProvider resources = null)
		{
			this.resources = resources;
		}

		public MockEditorProvider(IObjectEditor editor)
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
			return ReflectionObjectEditor.GetAssignableTypes(type, childTypes);
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