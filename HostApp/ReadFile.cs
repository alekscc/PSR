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
           
            /*String report = String.Join(Environment.NewLine, matrix
                .Select(line => String.Join(" ", line)));

            Console.Write(report);*/

            return matrix;
        }

        public T[,] JaggedToMultidimensional<T>(T[][] jaggedArray)
        {
            T[,] array = null;
            try
            {
                int rows = jaggedArray.Length;
                int cols = jaggedArray.Max(subArray => subArray.Length);
                array = new T[rows, cols];
                for (int i = 0; i < rows; i++)
                {
                    cols = jaggedArray[i].Length;
                    for (int j = 0; j < cols; j++)
                    {
                        array[i, j] = jaggedArray[i][j];
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Błąd zamiany macierzy!", e);
            }
            return array;
        }
    }
}