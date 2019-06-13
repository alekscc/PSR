using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfServiceLibrary
{
    public enum VERTICE_STATUS
    {
        FREE,
        IN_WORK,
        DONE
    }

    class Vertice
    {
        private VERTICE_STATUS status;
        private int number;


        public Vertice(int num)
        {
            Status = VERTICE_STATUS.FREE;
            number = num;
        }
        public bool Take()
        {
            if (status == VERTICE_STATUS.FREE)
            {
                status = VERTICE_STATUS.IN_WORK;
                return true;
            }
            return false;
        }
        public bool Free()
        {
            if(status==VERTICE_STATUS.IN_WORK)
            {
                status = VERTICE_STATUS.FREE;
                return true;
            }
            return false;
        }
        public int Number
        {
            get
            {
                return number;
            }
        }
        public VERTICE_STATUS Status { get => status; set => status = value; }
 
    }
    class VerticesManagement
    {
        private List<Vertice> listOfVertices = new List<Vertice>();

        public VerticesManagement()
        {

        }
        public void GenerateVertices(int numberOfVertices)
        {
            for (int i = 0; i < numberOfVertices; i++)
            {
                listOfVertices.Add(new Vertice(i));
            }
        }
        public int FreeVertices(int[] arr)
        {
            int arrSize = arr.Length,
                free = 0;

            for (int j = 0; j < arrSize; j++)
            {
                for (int i = 0; i < listOfVertices.Count; i++)
                {
                    if (listOfVertices[i].Number == arr[j])
                    {
                        if(listOfVertices[i].Free())
                        {
                            free++;
                        }
                    }
                }
            }

            return free;

        }
        public bool SubmitVertices(int[] arr)
        {
            int arrSize = arr.Length;
           
            for(int j = 0; j < arrSize; j++)
            {
                for (int i = 0; i < listOfVertices.Count; i++)
                {
                    if(listOfVertices[i].Number==arr[j])
                    {
                        listOfVertices.RemoveAt(i);
                    }
                }
            }
            return true;
        }
        public bool IsListEmpty()
        {
            int count = 0;

            foreach (var vert in listOfVertices)
                if (vert.Status == VERTICE_STATUS.FREE)
                    count++;

            if (count > 0) return false;
            return true;
        }
        public int GetNumberOfVertices()
        {
            return listOfVertices.Count;
        }
        public int GetNumberOfFreeVertices()
        {
            int n = 0;

            foreach (Vertice v in listOfVertices)
                if (v.Free()) n++;

            return n;
        }
        
        public int[] GetVertices(int n)
        {
       
            int[] arr = new int[n];
            int j=0;
            for(int i=0;i<listOfVertices.Count && j < n;i++)
            {
                if (listOfVertices[i].Take())
                {
                    arr[j++] = listOfVertices[i].Number;
                }
            }
            if(j==n) return arr;
            else
            {
                int[] _arr = new int[j];
                for (int i = 0; i < j; i++)
                    _arr[i] = arr[i];

                return _arr;
            }

            
        }
    }
}
