using System;
using System.IO;
using System.Text.RegularExpressions;

namespace HostApp
{
    public class Graph
    {

        public int[][] matrix;
    
        public Graph(string path)
        {
            ReadFile readFile = new ReadFile();
            matrix = readFile.ReadData(path);
        }

        public Graph()
        {

        }

        public void generateMatrix()
        {
            string rozmiarS, zakres;
            int[,] matrix;
            //String nazwaPliku;
            Regex regex = new Regex("^[0-9]+$");
            Random random = new Random();

            do
            {
                Console.Write("Podaj rozmiar macierzy: ");
                rozmiarS = Console.ReadLine();
            } while (!regex.IsMatch(rozmiarS));

            int rozmiar = int.Parse(rozmiarS);
            matrix = new int[rozmiar, rozmiar];

            do
            {
                Console.Write("Podaj zakres losowanych liczb (0 - ???): ");
                zakres = Console.ReadLine();
            } while (!regex.IsMatch(zakres));

            for (int i = 0; i < rozmiar; i++)
                for (int j = 0; j < rozmiar; j++)
                    matrix[i, j] = random.Next(int.Parse(zakres));

            /*for (int i = 0; i < rozmiar; i++)
            {
                for (int j = 0; j < rozmiar; j++)
                    Console.Write(matrix[i, j] + " ");
                Console.WriteLine();
            }*/

            /*Console.Write("Podaj nazwe pliku: ");
            nazwaPliku = Console.ReadLine();
            if (!File.Exists(nazwaPliku + ".txt"))
            {*/
                FileStream fileStream = new FileStream("macierz.txt", FileMode.Create, FileAccess.ReadWrite);
                StreamWriter w = new StreamWriter(fileStream);
                for (int i = 0; i < rozmiar; i++)
                {
                    for (int j = 0; j < rozmiar; j++)
                    {
                        if (j < rozmiar - 1)
                            w.Write(matrix[i, j] + " ");
                        else
                            w.WriteLine(matrix[i, j]);
                    }
                }
                w.Close();
                fileStream.Close();
            /*}
            else
            {
                Console.WriteLine("Plik o takiej nazwie już istnieje!");
            }*/
        }
    }
}