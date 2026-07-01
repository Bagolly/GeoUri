using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;


namespace Bagolly.GeoUri;


/// <summary> Represents a geographical location using the WGS-84 Coordinate Reference System. </summary>	
/// <remarks> Supported type parameters include <see cref="double"/>, <see cref = "float"/>, and <see cref = "Half"/>. </remarks>
public readonly struct GeoUri<T> : IEquatable<GeoUri<T>>, ISpanFormattable, IUtf8SpanFormattable
    where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
{
    /// <summary> The latitude in decimal degrees. </summary>
    public T Latitude { get; init; }

    /// <summary> The longitude in decimal degrees. </summary> 
    public T Longitude { get; init; }

    /// <summary> The altitude in meters if present, or <see langword="null"/>. </summary>
    public T? Altitude { get; init; }

    /// <summary> The uncertainty amount in meters if present, or <see langword="null"/>. </summary>
    public T? Uncertainty { get; init; }


    /// <summary> Creates a new instance from the provided coordinates. </summary>
    /// <remarks> The longitude of coordinates reflecting the poles (latitude -90 or +90) will be normalized to 0. </remarks>
    /// <param name="latitude">A degree between -90 and +90.</param>
    /// <param name="longitude">A degree between -180 and +180.</param>
    /// <param name="altitude">If specified, a nonnegative value representing the altitude in meters.</param>
    /// <param name="uncertainty">If specified, a nonnegative value representing the uncertainty amount in meters.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public GeoUri(T latitude, T longitude, T? altitude = null, T? uncertainty = null)
    {
        // Clamp will filter NaN and +/- Infinity for the coords.       
        var clampedLat = T.Clamp(latitude, T.CreateChecked(-90), T.CreateChecked(+90));
        var clampedLong = T.Clamp(longitude, T.CreateChecked(-180), T.CreateChecked(+180));

        if (latitude != clampedLat)
            ThrowArgOutOfRange(nameof(latitude));

        if (longitude != clampedLong)
            ThrowArgOutOfRange(nameof(longitude));

        if (altitude.HasValue)
            if ((T.IsNegative(altitude.Value) && !T.IsZero(altitude.Value)) || !T.IsFinite(altitude.Value))
                ThrowArgOutOfRange(nameof(altitude));

        if (uncertainty.HasValue)
            if ((T.IsNegative(uncertainty.Value) && !T.IsZero(uncertainty.Value)) || !T.IsFinite(uncertainty.Value))
                ThrowArgOutOfRange(nameof(uncertainty));

        Latitude = latitude;
        Longitude = T.Abs(latitude) is 90 ? T.Zero : longitude;
        Altitude = altitude;
        Uncertainty = uncertainty;
    }


    /// <inheritdoc/>
    /// <remarks> The arguments for <paramref name="format"/> and <paramref name="provider"/> will not be used. </remarks>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        // All comments from the UTF-8 version apply here as well.
        ReadOnlySpan<char> geo = "geo:";
        ReadOnlySpan<char> unc = ";u=";
        charsWritten = 0;

        if (!geo.TryCopyTo(destination))
            return false;

        charsWritten = geo.Length;

        if (!Latitude.TryFormat(destination[charsWritten..], out int length, "R", CultureInfo.InvariantCulture))
            return false;

        charsWritten += length;
        destination[charsWritten++] = ',';

        if (!Longitude.TryFormat(destination[charsWritten..], out length, "R", CultureInfo.InvariantCulture))
            return false;

        charsWritten += length;

        if (Altitude.HasValue)
        {
            destination[charsWritten++] = ',';

            if (!Altitude.Value.TryFormat(destination[charsWritten..], out length, "R", CultureInfo.InvariantCulture))
                return false;

            charsWritten += length;
        }

        if (Uncertainty.HasValue)
        {
            unc.CopyTo(destination[charsWritten..]);
            charsWritten += unc.Length;

            if (!Uncertainty.Value.TryFormat(destination[charsWritten..], out length, "R", CultureInfo.InvariantCulture))
                return false;

            charsWritten += length;
        }

        return true;
    }


    /// <inheritdoc/>
    /// <remarks> The arguments for <paramref name="format"/> and <paramref name="provider"/> will not be used. </remarks>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        // We ignore the format strings for several reasons.
        // 1. Avoiding some pathological cases, like 0x8000000000000001 with "G758".
        // 2. Not having to filter format strings like B/X/C, which will output non-spec-compliant results.
        // 3. Not having to filter cultures using a comma as the decimal separator, as the period is part of the ABNF.

        // RFC 5870 states that the "number of digits in the <coordinates> values MUST NOT be interpreted
        // as an indication of a certain level of accuracy or uncertainty", so it is legal to output a
        // shorter value (eg. 48.210 instead of 48.20100).

        ReadOnlySpan<byte> geo = "geo:"u8;
        ReadOnlySpan<byte> unc = ";u="u8;
        bytesWritten = 0;

        if (!geo.TryCopyTo(utf8Destination))
            return false;

        bytesWritten = geo.Length;

        if (!Latitude.TryFormat(utf8Destination[bytesWritten..], out int length, "R", CultureInfo.InvariantCulture))
            return false;

        bytesWritten += length;
        utf8Destination[bytesWritten++] = (byte)',';

        if (!Longitude.TryFormat(utf8Destination[bytesWritten..], out length, "R", CultureInfo.InvariantCulture))
            return false;

        bytesWritten += length;

        if (Altitude.HasValue)
        {
            utf8Destination[bytesWritten++] = (byte)',';

            if (!Altitude.Value.TryFormat(utf8Destination[bytesWritten..], out length, "R", CultureInfo.InvariantCulture))
                return false;

            bytesWritten += length;
        }

        if (Uncertainty.HasValue)
        {
            unc.CopyTo(utf8Destination[bytesWritten..]);
            bytesWritten += unc.Length;

            if (!Uncertainty.Value.TryFormat(utf8Destination[bytesWritten..], out length, "R", CultureInfo.InvariantCulture))
                return false;

            bytesWritten += length;
        }

        return true;
    }


    /// <inheritdoc/>
    /// <remarks> The arguments for <paramref name="format"/> and <paramref name="provider"/> will not be used. </remarks>
    string IFormattable.ToString(string? format, IFormatProvider? provider) => ToString();


    /// <inheritdoc/>
    public override string ToString()
    {
        // The standard [1] states that IBinaryFloatingPointIeee754 applies to Double, Single, and Half.
        // Therefore, we can make a worst-case estimate for the required stack size as
        // "geo:[24*char],[24*char],[24*char];u=[24*char]", rounding it up to a 128 character buffer. If this
        // is somehow too small, the method will throw a FormatException, but no buffer overflow will occur.
        // [1]: https://github.com/dotnet/docs/blob/main/docs/standard/generics/math.md#numeric-interfaces

        Span<char> buffer = stackalloc char[128];

        if (!TryFormat(buffer, out int length, "R", CultureInfo.InvariantCulture))
            ThrowFormat();

        return buffer[..length].ToString();
    }


    /// <inheritdoc/>
    /// <remarks>Equality is determined as specified by RFC 5870 §3.3.4.</remarks>
    public bool Equals(GeoUri<T> other) => Latitude == other.Latitude && Longitude == other.Longitude &&
                                           Altitude.Equals(other.Altitude) && Uncertainty.Equals(other.Uncertainty);


    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is GeoUri<T> other && Equals(other);


    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude, Altitude, Uncertainty);


    public static bool operator ==(in GeoUri<T> lhs, in GeoUri<T> rhs) => lhs.Equals(rhs);
    public static bool operator !=(in GeoUri<T> lhs, in GeoUri<T> rhs) => !(lhs == rhs);


    [DoesNotReturn]
    private static void ThrowArgOutOfRange(string? name) => throw new ArgumentOutOfRangeException(name);


    [DoesNotReturn]
    private static void ThrowFormat() => throw new FormatException();
}
