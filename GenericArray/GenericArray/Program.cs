using System;
using GenericArray;

class Program
{
    static void Main(string[] args)
    {
        GArray<int> ga1 = new GArray<int>(1,2,3,4,5);
        GArray<double> ga2 = new GArray<double>(1.1, 2.2, 3.3, 4.4, 5.5);

        Console.WriteLine(ga1 + ga2); // [2.1, 4.2, 6.3, 8.4, 10.5]
    }
}