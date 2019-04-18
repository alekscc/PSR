using System;

namespace ClientApp
{
    public class DijkstraAlgorithm
    {
 
        public static int[] Dijkstra(int[][] graph, int src, int numberOfVertex)
        {
            int[] dist = new int[numberOfVertex];
            bool[] sptSet = new bool[numberOfVertex];

            for (int i = 0; i < numberOfVertex; i++)
            {
                dist[i] = int.MaxValue;
                sptSet[i] = false;
            }

            dist[src] = 0;

            for (int count = 0; count < numberOfVertex - 1; count++)
            {
                int u = minDistance(dist, sptSet, numberOfVertex);
                sptSet[u] = true;

                for (int v = 0; v < numberOfVertex; v++) 
                    if (!sptSet[v] && graph[u][v] != 0 && dist[u] != int.MaxValue && dist[u] + graph[u][v] < dist[v])
                        dist[v] = dist[u] + graph[u][v];
            }

            //printSolution(dist, numberOfVertex);

            return dist;
        }

        static int minDistance(int[] distance, bool[] sptSet, int numberOfVertex)
        {
            int minimumDistance = int.MaxValue, minIndex = -1;

            for (int v = 0; v < numberOfVertex; v++)
                if (sptSet[v] == false && distance[v] <= minimumDistance)
                {
                    minimumDistance = distance[v];
                    minIndex = v;
                }
            return minIndex;
        }

        static void printSolution(int[] dist, int numberOfVertex)
        {
            Console.Write("Vertex     Distance " + "from Source\n");
            for (int i = 0; i < numberOfVertex; i++)
                Console.Write(i + " \t\t " + dist[i] + "\n");
        }

    }
}