using System;

namespace System.Reflection {
	public static class Ext_Type {
		public static FieldInfo GetFieldPrivate(this Type t, string fieldName, BindingFlags flags){
			flags |= BindingFlags.NonPublic;
			FieldInfo result = null;
			Type type = t;
			Type bt = typeof(object);
			while (type != bt){
				result = type.GetField(fieldName, flags);
				if (result != null) break;
				type = type.BaseType;
			}
			return result;
		}
		public static PropertyInfo GetPropertyPrivate(this Type t, string propName, BindingFlags flags){
			flags |= BindingFlags.NonPublic;
			PropertyInfo result = null;
			Type type = t;
			Type bt = typeof(object);
			while (type != bt){
				result = type.GetProperty(propName, flags);
				if (result != null) break;
				type = type.BaseType;
			}
			return result;
		}
		public static MethodInfo GetMethodPrivate(this Type t, string methodName, BindingFlags flags){
			flags |= BindingFlags.NonPublic;
			MethodInfo result = null;
			Type type = t;
			Type bt = typeof(object);
			while (type != bt){
				result = type.GetMethod(methodName, flags);
				if (result != null) break;
				type = type.BaseType;
			}
			return result;
		}
	}
}