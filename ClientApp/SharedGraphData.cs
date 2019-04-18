using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    // klasa do przechowywania danych współdzielonych między wątkami
    class SharedGraphData
    {
        private int[][] matrix;
        private int[] vertices;
        private int curVertice;
        private int size;
        private int lowestDist = 99999;
        private readonly object block = new object();

        public int[][] Matrix
        {
            get
            {
                return matrix;
            }
        }
        public int GetNextVertice
        {
            get
            {
                lock(block)
                {
                    return (curVertice >= 0) ? vertices[curVertice--] : -1;
                }
            }
        }
        public int GetVertices
        {
            get
            {
                return curVertice;
            }
        }
        public int MatrixSize
        {
            get
            {
                return size;
            }
        }  
        public int Dist
        {
            set
            {
                lock (block)
                {
                    lowestDist = (lowestDist > value) ? value : lowestDist;
                }
            }
            get
            {
                return lowestDist;
            }
        }


        public SharedGraphData(int [][] matrix,int[] vertices)
        {
            if (matrix == null) Console.WriteLine("matrix is null");
            this.matrix = matrix;
            this.size = this.matrix.GetLength(0);
            
            this.vertices = vertices;
            this.curVertice = vertices.Length - 1;

            if(this.vertices==null)
            {
                Console.WriteLine("vertices is null");
            }
         
        }
    }
}
