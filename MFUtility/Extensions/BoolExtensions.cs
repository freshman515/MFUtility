using System;
using System.Diagnostics;

namespace MFUtility.Extensions;

public static class BoolExtensions {
    public static string ToYesNo(this bool value) => value ? "Yes" : "No";

    public static string ToChineseYesNo(this bool value) => value ? "是" : "否";

    public static void IfTrue(this bool condition, Action action)
    {
        if (condition)
        {
            try { action?.Invoke(); }
            catch (Exception ex) { Debug.WriteLine($"[BoolExtensions.IfTrue] {ex.Message}"); }
        }
    }

    public static void IfFalse(this bool condition, Action action)
    {
        if (!condition)
        {
            try { action?.Invoke(); }
            catch (Exception ex) { Debug.WriteLine($"[BoolExtensions.IfFalse] {ex.Message}"); }
        }
    }

    public static bool Toggle(ref this bool value)
    {
        value = !value;
        return value;
    }
}
