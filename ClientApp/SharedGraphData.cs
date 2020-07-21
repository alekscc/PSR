using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public struct Record
    {
        public int vertice;
        public int distance;
    }

    // klasa do przechowywania danych współdzielonych między wątkami
    class SharedGraphData
    {
        private int[][] matrix;
        private int[] vertices;
        private int curVertice;
        private int size;
        //private int lowestDist = 99999;
       // private int verticeNum = -1;
        private Record record;
        private bool isNoRecord = true;
        private readonly object block = new object();

        public int[][] Matrix
        {
            get
            {
                return matrix;
            }
        }
        public int GetNumberOfVertices
        {
            get
            {
                return vertices.Length;
            }
        }
        public int GetNextVertice
        {
            get
            {
                lock(block)
                {
                    return (curVertice >=0) ? vertices[curVertice--] : -1;
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
        public void SetRecord(int vert,int dist)
        {

            lock (block)
            {
                if (isNoRecord)
                {
                    isNoRecord = false;
                }
                else if (dist >= record.distance) return;

                record.vertice = vert;
                record.distance = dist;
            }
        }
        public int RecordVertice
        {
            get
            {
                return record.vertice;
            }
        }
        public int RecordDistance
        {
            get
            {
                return record.distance;
            }
        }

        public SharedGraphData(int [][] matrix,int[] vertices)
        {
            if (matrix == null) Console.WriteLine("matrix is null");
            this.matrix = matrix;
            this.size = this.matrix.GetLength(0);
            
            this.vertices = vertices;
            this.curVertice = vertices.Length-1;

            if(this.vertices==null)
            {
                Console.WriteLine("vertices are null");
            }
         
        }
    }
}
