using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct Point
{
    public int X;
    public int Y;

    public void MoveRight()
    {
        X += 1;
    }
}

public class Program
{
    public static void Main()
    {
        Point p = new Point { X = 0, Y = 0 };
        MovePoint(p);
        Console.WriteLine($"X = {p.X}");
    }

    static void MovePoint(Point point)
    {
        point.MoveRight();
    }
}
