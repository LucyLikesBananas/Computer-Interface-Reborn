using System;
using HarmonyLib;

namespace ComputerInterface.Extensions;

internal static class ReflectionEx {
    public static void InvokeMethod(this object obj, string name, params object[] parameters) {
        var methodInfo = AccessTools.Method(obj.GetType(), name);
        // FIX: AccessTools.Method returns null when the member isn't found (e.g. after a game
        // update renames it). Surface a clear error instead of a NullReferenceException.
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