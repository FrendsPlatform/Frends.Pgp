using System;

namespace Frends.Pgp.SignFile.Definitions;

internal static class Extensions
{
    /// <summary>
    /// Convert enum to given type
    /// </summary>
    /// <typeparam name="TEnum">Enum to convert to</typeparam>
    /// <param name="source">Enum to convert from</param>
    /// <returns>TEnum</returns>
    /// <exception cref="ArgumentException">Thrown when the source enum value cannot be converted to TEnum</exception>
    internal static TEnum ConvertEnum<TEnum>(this Enum source)
    {
        return (TEnum)Enum.Parse(typeof(TEnum), source.ToString(), true);
    }
}