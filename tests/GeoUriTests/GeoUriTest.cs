using System.Numerics;
using Bagolly.GeoUri;


namespace GeoUriTests;



[TestFixture]
public sealed class Tests
{
    [TestCase(TypeArgs = [typeof(Half)])]
    [TestCase(TypeArgs = [typeof(float)])]
    [TestCase(TypeArgs = [typeof(double)])]
    public void TestNaNThrows<T>() where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.NaN, T.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, altitude: T.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: T.NaN));
    }



    [TestCase(TypeArgs = [typeof(Half)])]
    [TestCase(TypeArgs = [typeof(float)])]
    [TestCase(TypeArgs = [typeof(double)])]
    public void TestInfinityThrows<T>() where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.PositiveInfinity, T.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.NegativeInfinity, T.Zero));

        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.NegativeInfinity));

        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, altitude: T.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, altitude: T.NegativeInfinity));

        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: T.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: T.NegativeInfinity));
    }


    // Attribute creation expressions must be constant, this is a workaround.
    private static IEnumerable<TestCaseData> TestLatHalf => [new((Half)90.1, (Half)90.01, (Half)90)];

    [TestCase([90.00000000000001d, 90.000000000000001d, 90d], TypeArgs = [typeof(double)])]
    [TestCase([90.00001f, 90.000001f, 90f], TypeArgs = [typeof(float)])]
    [TestCaseSource(nameof(TestLatHalf))]
    public void TestLatitudeRange<T>(T case1, T case2, T case3) where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        // Case 1: bad
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(+case1, T.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(-case1, T.Zero));

        // Case 2: bad, but the precision is out of range
        Assert.DoesNotThrow(() => new GeoUri<T>(+case2, T.Zero));
        Assert.DoesNotThrow(() => new GeoUri<T>(-case2, T.Zero));

        // Case 3: good
        Assert.DoesNotThrow(() => new GeoUri<T>(+case3, T.Zero));
        Assert.DoesNotThrow(() => new GeoUri<T>(-case3, T.Zero));
    }


    // Attribute creation expressions must be constant, this is a workaround.
    private static IEnumerable<TestCaseData> TestLotHalf => [new((Half)180.1, (Half)180.01, (Half)180)];

    [TestCase([180.0000000000001d, 180.00000000000001d, 180d], TypeArgs = [typeof(double)])]
    [TestCase([-180.00001f, -180.000001f, 180f], TypeArgs = [typeof(float)])]
    [TestCaseSource(nameof(TestLotHalf))]
    public void TestLongitudeRange<T>(T case1, T case2, T case3) where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        // Case 1: bad
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, +case1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, -case1));

        // Case 2: bad, but the precision is out of range
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, +case2));
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, -case2));

        // Case 3: good
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, +case3));
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, -case3));
    }


    [TestCase(TypeArgs = [typeof(Half)])]
    [TestCase(TypeArgs = [typeof(float)])]
    [TestCase(TypeArgs = [typeof(double)])]
    public void TestAltitudeRange<T>() where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        // Case 1: bad
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, altitude: T.Zero - T.Epsilon));

        // Case 3: good
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, altitude: T.One));

        // Case 4: good (+0, -0)
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, altitude: T.Zero));
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, altitude: T.NegativeZero));
    }


    [TestCase(TypeArgs = [typeof(Half)])]
    [TestCase(TypeArgs = [typeof(float)])]
    [TestCase(TypeArgs = [typeof(double)])]
    public void TestUncertaintyRange<T>() where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        // Case 1: bad
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: T.Zero - T.Epsilon));

        // Case 3: good
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: T.One));

        // Case 4: good (+0, -0)
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: T.Zero));
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: T.NegativeZero));
    }


    [TestCase(TypeArgs = [typeof(Half)])]
    [TestCase(TypeArgs = [typeof(float)])]
    [TestCase(TypeArgs = [typeof(double)])]
    public void TestNullHandling<T>() where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, uncertainty: null));
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, altitude: null));
        Assert.DoesNotThrow(() => new GeoUri<T>(T.Zero, T.Zero, null, null));
    }


    [TestCase(TypeArgs = [typeof(Half)])]
    [TestCase(TypeArgs = [typeof(float)])]
    [TestCase(TypeArgs = [typeof(double)])]
    public void TestTryFormatBadBuffers<T>() where T : struct, IBinaryFloatingPointIeee754<T>, ISpanFormattable, IUtf8SpanFormattable
    {
        GeoUri<T> g = new(default, default);

        Assert.DoesNotThrow(() => g.TryFormat(Span<byte>.Empty, out _, default));

        var case1B = g.TryFormat(Span<byte>.Empty, out int case1BWritten, default);
        var case2B = g.TryFormat(stackalloc byte[1], out int case2BWritten, default);
        var case3B = g.TryFormat(default(Span<byte>), out int case3BWritten, default);

        var case1C = g.TryFormat(Span<char>.Empty, out int case1CWritten, default);
        var case2C = g.TryFormat(stackalloc char[1], out int case2CWritten, default);
        var case3C = g.TryFormat(default(Span<char>), out int case3CWritten, default);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(case1B, Is.False);
            Assert.That(case1BWritten, Is.Zero);
            Assert.That(case2B, Is.False);
            Assert.That(case2BWritten, Is.Zero);
            Assert.That(case3B, Is.False);
            Assert.That(case3BWritten, Is.Zero);

            Assert.That(case1C, Is.False);
            Assert.That(case1CWritten, Is.Zero);
            Assert.That(case2C, Is.False);
            Assert.That(case2CWritten, Is.Zero);
            Assert.That(case3C, Is.False);
            Assert.That(case3CWritten, Is.Zero);
        }
    }

    [Test]
    public void TestToStringLongOutput()
    {
        // We only test this with double, as it has the highest precision, and thus largest output.
        double e = double.Pow(double.E, 2);
        GeoUri<double> g = new(-e, -e, double.MaxValue, double.MaxValue);
        Assert.DoesNotThrow(() => g.ToString());
    }
}
