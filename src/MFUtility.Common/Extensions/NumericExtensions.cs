namespace MFUtility.Common.Extensions;

public static class NumericExtensions
{
    public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
        => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;

    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        => value.CompareTo(min) < 0 ? min : (value.CompareTo(max) > 0 ? max : value);

    public static double Map(this double value, double inMin, double inMax, double outMin, double outMax)
        => (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;

    public static double RoundTo(this double value, int decimals)
        => Math.Round(value, decimals);

    public static bool NearlyEquals(this double a, double b, double epsilon = 1e-6)
        => Math.Abs(a - b) < epsilon;
}