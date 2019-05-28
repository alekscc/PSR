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

        public int RecordResult
        {
            get
            {
                return sharedGraph.RecordDistance;
            }
        }
        public int RecordVertice
        {
            get
            {
                return sharedGraph.RecordVertice;
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
            

            this.numberOfThreads = (sharedGraph.GetVertices > this.numberOfThreads) ? this.numberOfThreads : (sharedGraph.GetVertices<1) ? 1 : sharedGraph.GetVertices;

 
                Thread[] threads = new Thread[this.numberOfThreads];
                // czas wyknania algorytmu dla klienta
                var watch = System.Diagnostics.Stopwatch.StartNew();

                DateTime start = DateTime.Now;
            DateTime stop;
            TimeSpan interval;

                for (int i = 0; i < numberOfThreads; i++)
                {
                   
                    threads[i] = new Thread(() => { doChildWork(sharedGraph.GetNextVertice); });

                    threads[i].Start();
                }
            //Console.WriteLine("Sizeof matrix=" + sharedGraph.GetNumberOfVertices);

                foreach (Thread t in threads)
                {
                    t.Join();

                }
                watch.Stop();
            stop = DateTime.Now;
            interval = stop - start;
            long czas = interval.Ticks * 100;
                var elapsedMiliseconds = watch.ElapsedMilliseconds;
                Console.WriteLine("Total time:" + elapsedMiliseconds + "ms "+czas);
            // czas wyknania algorytmu dla klienta - koniec

        }
        private int runDijkstraAlghoritm(int vertice)
        {

            
            int[] sum = DijkstraAlgorithm.Dijkstra(sharedGraph.Matrix, vertice, sharedGraph.MatrixSize);
        
            return sum.Sum();
        }
        private void doChildWork(int vertice)
        {

            if (vertice < 0) return;

            //Console.WriteLine("WĄTEK WĄTEK");
            int recordVert = vertice;
            int recordDist = runDijkstraAlghoritm(vertice);
           // Console.WriteLine("Łączna długość najkrótszych ścieżek: " + "wierzchołek:" + vertice + " dystans:" + recordDist);
            vertice = sharedGraph.GetNextVertice;

            while (vertice >= 0)
            {
                int sum = runDijkstraAlghoritm(vertice);
                Console.WriteLine("Łączna długość najkrótszych ścieżek: " + "wierzchołek:" + vertice + " dystans:" + sum);
                if (recordDist > sum)
                {
                    recordDist = sum;
                    recordVert = vertice;
                }

                vertice = sharedGraph.GetNextVertice;
            } 

            sharedGraph.SetRecord(recordVert, recordDist);
        }
        
        /*
        private void doChildWork(int vertice)  // operacje na wątku
        {
            int lowestDist = 99999;
            int verticeNum = -1;

            do
            {
                
                int[] sum = DijkstraAlgorithm.Dijkstra(sharedGraph.Matrix, vertice, sharedGraph.MatrixSize);
                int _sum = sum.Sum();
                

                if(lowestDist > _sum)
                {
                    lowestDist = _sum;
                    verticeNum = vertice;
                }

                vertice = sharedGraph.GetNextVertice; // pobranie kolejnego wierzchołka do obliczeń

            } while (vertice >= 0);

            sharedGraph.SetRecord(verticeNum, lowestDist);
            
            //sharedGraph.Dist = lowestDist;
            //sharedGraph.VerticeNum = vertice;
            
        }*/
    }
}
