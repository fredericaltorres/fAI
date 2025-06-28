using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Program2
{
    public static void Main()
    {
        var results = new List<Func<int>>();

        for (int i = 0; i < 5; i++)
        {
            results.Add(() => i);
        }

        foreach (var f in results)
        {
            Console.WriteLine(f());
        }
    }
}
