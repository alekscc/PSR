using System;
using System.IO;
using System.Linq;

namespace HostApp
{
    public class ReadFile
    {
        public int[][] ReadData(string path)
        {
            int[][] matrix = null;
            try
            {
                matrix = File.ReadAllLines(path)
                  .Select(l => l.Split(' ').Select(i => int.Parse(i)).ToArray())
                  .ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("Błąd odczytu pliku!", e);
            }

            return matrix;
        }
    }
}