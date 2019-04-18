using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace ClientApp
{
    class ConcurrentProgram
    {
        private int numberOfThreads;
        private SharedGraphData sharedGraph;

        public int BestResult
        {
            get
            {
                return sharedGraph.Dist;
            }
        }


        public ConcurrentProgram(int numberOfThreads,int[][] matrix,int[] vertices)
        {
            this.numberOfThreads = numberOfThreads;
            sharedGraph = new SharedGraphData(matrix,vertices);// klasa do przechowywania danych współdzielonych
        }
        public void Start() // wczytywanie danych tymczasowo w tej metodzie
        {
            //graph = new Graph(@"../../macierz.txt"); // instancja tej klasy będzie tylko na serwerze
            

            this.numberOfThreads = (sharedGraph.GetVertices > this.numberOfThreads) ? this.numberOfThreads : sharedGraph.GetVertices;

 
                Thread[] threads = new Thread[this.numberOfThreads];

                var watch = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < numberOfThreads; i++)
                {

                    threads[i] = new Thread(() => { doChildWork(sharedGraph.GetNextVertice); });

                    threads[i].Start();
                }

                foreach (Thread t in threads)
                {
                    t.Join();

                }
                watch.Stop();
                var elapsedMiliseconds = watch.ElapsedMilliseconds;
                Console.WriteLine("Total time:" + elapsedMiliseconds + "ms");


        }

        private void doChildWork(int vertice)  // operacje na wątku
        {
            int lowestDist = 9999;

            do
            {
                
                int[] sum = DijkstraAlgorithm.Dijkstra(sharedGraph.Matrix, vertice, sharedGraph.MatrixSize);
                int _sum = sum.Sum();
                Console.WriteLine("Łączna długość najkrótszych ścieżek: " + "vert " + vertice+" dist "+_sum);

                lowestDist = (_sum < lowestDist) ? _sum : lowestDist;

                vertice = sharedGraph.GetNextVertice; // pobranie kolejnego wierzchołka do obliczeń

            } while (vertice >= 0);


            sharedGraph.Dist = lowestDist;

        }
    }
}
