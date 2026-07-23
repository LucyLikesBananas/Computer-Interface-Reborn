using System;
using HarmonyLib;

namespace ComputerInterface.Extensions;

internal static class ReflectionEx {
    public static void InvokeMethod(this object obj, string name, params object[] parameters) {
        var methodInfo = AccessTools.Method(obj.GetType(), name);
        
        
        if (methodInfo == null)
            throw new MissingMethodException(obj.GetType().Name, name);
        methodInfo.Invoke(obj, parameters);
    }

    public static void SetField(this object obj, string name, object value) {
        var fieldInfo = AccessTools.Field(obj.GetType(), name);
        if (fieldInfo == null)
            throw new MissingFieldException(obj.GetType().Name, name);
        fieldInfo.SetValue(obj, value);
    }

    public static T GetField<T>(this object obj, string name) {
        var fieldInfo = AccessTools.Field(obj.GetType(), name);
        if (fieldInfo == null)
            throw new MissingFieldException(obj.GetType().Name, name);
        return (T)fieldInfo.GetValue(obj);
    }
}