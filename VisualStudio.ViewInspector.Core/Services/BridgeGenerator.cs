using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace VisualStudio.ViewInspector.Services
{
    internal class BridgeParameters
    {
        public Type[] Arguments { get; private set; }
        public Type ReturnType { get; private set; }

        public BridgeParameters(Type[] arguments, Type returnType)
        {
            this.Arguments = arguments;
            this.ReturnType = returnType;
        }
    }

    internal static class BridgeGenerator
    {
        internal const string ClassName = "BridgeClass";
        internal const string PropertyName = "Handler";
        internal const string MethodName = "ListenerEvent";

        public static object CreateInstance(object handlerObject, string handlerEventHandlerName)
        {
            var bridgeParameters = GetBridgeParametersFromFieldName(handlerObject, handlerEventHandlerName);
            return CreateInstance(bridgeParameters);
        }

        public static object CreateInstanceAndAttach(object handlerObject, string handlerEventHandlerName, Action actionHandler)
        {
            var bridgeInstance = CreateInstance(handlerObject, handlerEventHandlerName);

            //ATTACHEO DE HANDLE DE PROXY ===================================================

            SetObject(bridgeInstance, PropertyName, actionHandler);
            AttachEvent(handlerObject, handlerEventHandlerName, bridgeInstance, MethodName);

            return bridgeInstance;
        }

        public static object CreateInstance(BridgeParameters parameters = null)
        {
            if (parameters == null)
            {
                parameters = new BridgeParameters(new Type[] { typeof(object), typeof(EventArgs) }, null);
            }
            var customType = CompileBridgeClassTypeInfo(parameters).AsType();
            var myObject = Activator.CreateInstance(customType);
            return myObject;
        }

        public static void SetObject(object sender, string property, Action actionHandler)
        {
            var delegateProperty = sender.GetType().GetProperty(property);
            delegateProperty.SetValue(sender, actionHandler);
        }

        public static void AttachEvent(object handlerObject, string handlerEventHandlerName, object targetObject, string targetMethodName)
        {
            //target method info
            var targetMethodInfo = targetObject.GetType().GetMethod(targetMethodName, BindingFlags.Instance | BindingFlags.Public);

            var sourceType = handlerObject.GetType();

            var sourceFieldInfo = GetFieldInfoByName(sourceType, handlerEventHandlerName);

            EventInfo targetMethodEventInfo = sourceType.GetEvent(handlerEventHandlerName);

            Delegate handler;
            //obtenemos el eventhandler
            if (targetMethodEventInfo != null)
            {
                Type type = targetMethodEventInfo.EventHandlerType;

                //creamos el delegado y attacheamos
                handler = Delegate.CreateDelegate(type, targetObject, targetMethodInfo);
                targetMethodEventInfo.AddEventHandler(handlerObject, handler);
            }
            else
            {
                handler = Delegate.CreateDelegate(sourceFieldInfo.FieldType, targetObject, targetMethodInfo);
            }

            sourceFieldInfo.SetValue(handlerObject, handler);
        }

        static FieldInfo GetFieldInfoByName(Type para, string handlerEventHandlerName)
        {
            foreach (var runtime in para.GetRuntimeFields())
            {
                if (runtime.Name == handlerEventHandlerName)
                    return runtime;
            }

            return null;
        }

        static BridgeParameters GetBridgeParametersFromFieldName(object handlerObject, string handlerEventHandlerName)
        {
            //EventInfo superMethodEventInfo = handlerObject.GetType().GetEvent(handlerEventHandlerName);

            FieldInfo type = GetFieldInfoByName(handlerObject.GetType(), handlerEventHandlerName);

            if (EventsHelper.IsFunc(type.FieldType))
            {
                var genericTypeArguments = type.FieldType.GenericTypeArguments.ToList();
                var returnType = genericTypeArguments.LastOrDefault();
                genericTypeArguments.Remove(returnType);
                return new BridgeParameters(genericTypeArguments.ToArray(), returnType);
            }
            else if (EventsHelper.IsAction(type.FieldType))
            {
                var arguments = new List<Type>();
                arguments.AddRange(type.FieldType.GenericTypeArguments);
                return new BridgeParameters(arguments.ToArray(), null);
            }
            else
            {
                var arguments = new List<Type>();
                arguments.Add(typeof(Object));

                if (type.FieldType.GenericTypeArguments.Length > 0)
                    arguments.AddRange(type.FieldType.GenericTypeArguments);
                else
                    arguments.Add(typeof(System.EventArgs));
                return new BridgeParameters(arguments.ToArray(), null);
            }
        }

        static TypeInfo CompileBridgeClassTypeInfo(BridgeParameters parameters)
        {
            // class BridgeClass {
            //
            //    Action Handler { get; set; }
            //
            //    void ListenerEvent (object, EventArgs) { Handler?.Invoke() }
            //
            // }

            var typeSignature = ClassName;

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);

            //Property: Action Handler { get; set; } ================================

            var propertyName = PropertyName;
            var propertyType = typeof(System.Action);

            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private | FieldAttributes.InitOnly);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            //Method: void ListenerEvent (object, EventArgs) ========================================================

            var mthdName = MethodName;
            MethodBuilder myMthdBld = typeBuilder.DefineMethod(
                                                  mthdName,
                                                  MethodAttributes.Public,
                                                  parameters.ReturnType,
                                                  parameters.Arguments);

            ILGenerator methodGen = myMthdBld.GetILGenerator();
            MethodInfo actionInvoke = propertyType.GetMethod("Invoke", new Type[] { });
            methodGen.Emit(OpCodes.Ldarg_0);
            methodGen.Emit(OpCodes.Call, propertyBuilder.GetMethod);
            methodGen.Emit(OpCodes.Call, actionInvoke);
            methodGen.Emit(OpCodes.Ret);

            var objectTypeInfo = typeBuilder.CreateTypeInfo();
            return objectTypeInfo;
        }
    }

}

