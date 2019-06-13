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
        static public void PrintStats(List<Client> listOfClients,int recordVert,int recordDist)
        {
            Console.WriteLine("NAME \t\t\t ID \t DATASYNC \t THREADS \t VERTICEID \t DISTANCE");
            foreach (Client cc in listOfClients)
            {
                ClientData c = cc.Data;
                Console.WriteLine("{0} \t {1} \t {2} \t\t {3} \t\t {4} \t\t {5}",
                    c.Name,
                    c.Identifier,
                    c.IsDataReady,
                    c.NumberOfThreads,
                    (cc.RecordVertice >= 0) ? cc.RecordVertice.ToString() : "BRAK",
                    (cc.RecordDistance >= 0) ? cc.RecordDistance.ToString() : "BRAK");
            }
            Console.WriteLine("Najlepszy wynik to, wierzchołek:{0} dystans:{1}", (recordVert >= 0) ? recordVert.ToString() : "BRAK",
                                                                                  (recordDist >= 0) ? recordDist.ToString() : "BRAK");
        }
    }
}
