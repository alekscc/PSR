using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfServiceLibrary
{
    class Printer
    {
        static ConsoleColor infoCol = ConsoleColor.White;
        static ConsoleColor warnCol = ConsoleColor.DarkYellow;
        static ConsoleColor errCol = ConsoleColor.DarkRed;
        static ConsoleColor recCol = ConsoleColor.Green;

        static public void PrintInfo(string msg)
        {
            Console.ForegroundColor = infoCol;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        static public void PrintWarn(string msg)
        {
            Console.ForegroundColor = warnCol;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        static public void PrintErr(string msg)
        {
            Console.ForegroundColor = errCol;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        static public void PrintRecord(int vert,int dist)
        {
            Console.ForegroundColor = recCol;
            Console.WriteLine("Nowy rekord! Wierzchołek:{0}, Dystans:{1}",vert,dist);
            Console.ResetColor();
        }
    }
}
