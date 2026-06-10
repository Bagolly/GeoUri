using Bagolly.GeoUri;
using System.Text;

namespace Bagolly.GeoUriDemo;

class Program
{
    static void Main()
    {
        Example5();
    }


    public static void Example1()
    {
        GeoUri<double> loc = new(21.312609459146294, -157.84616380085942);

        Console.WriteLine(loc); // Prints "geo:21.312609459146294,-157.84616380085942"
    }


    public static void Example2()
    {
        GeoUri<double> loc = new(52.2162214851868, 21.040482839710684, uncertainty: 35);

        Console.WriteLine(loc); // Prints "geo:52.2162214851868,21.040482839710684;u=35"
    }


    public static void Example3()
    {
        GeoUri<float> loc = new(37.077732f, 22.428797f, altitude: 170.5f);

        Console.WriteLine(loc); // Prints "geo:37.077732,22.428797,170.5"
    }


    public static void Example4()
    {
        GeoUri<double> loc = new(53.3624433980336, -6.052462644315026);

        Span<char> buffer = stackalloc char[128];

        if (!loc.TryFormat(buffer, out int bytesWritten)) // Write as char into an existing buffer.
            Console.WriteLine("Error.");
        else
            Console.WriteLine($"{buffer[..bytesWritten]}"); // Prints "geo:53.3624433980336,-6.052462644315026"
    }


    public static void Example5()
    {
        GeoUri<double> loc = new(59.103121320590155, 5.720613775297294);

        Span<byte> buffer = stackalloc byte[128];

        if (!loc.TryFormat(buffer, out int bytesWritten)) // Write as UTF-8 into an existing buffer.
            Console.WriteLine("Error.");
        
        else 
        {
            var str = "geo:59.103121320590155,5.720613775297294"u8;
            Console.WriteLine(str.SequenceEqual(buffer[..bytesWritten])); // Prints "True"
        }
    }
}
