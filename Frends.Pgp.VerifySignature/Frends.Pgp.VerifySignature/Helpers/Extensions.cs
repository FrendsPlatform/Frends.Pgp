using System;

namespace Frends.Pgp.VerifySignature.Helpers;

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
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!Enum.TryParse<TEnum>(source.ToString(), ignoreCase: true, out var result))
            throw new ArgumentException($"Cannot convert '{source}' to {typeof(TEnum).Name}.", nameof(source));

        return result;
    }
}