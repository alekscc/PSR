using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WcfServiceLibrary
{


    [DataContract]
    public class ClientData
    {
        [DataMember]
        public string name;
        [DataMember]
        public int id;
        [DataMember]
        public int numberOfThreads;
        [DataMember]
        public int[] listOfVertices;
        [DataMember]
        public int recordVert;
        [DataMember]
        public int recordDist;
        [DataMember]
        public long time;
        [DataMember]
        public long commTime;
        [DataMember]
        public DateTime date;
    

        private IDuplexCallback callback;
        private bool isDataReady;

        

        public ClientData()
        {

        }

        public ClientData(string name,int id,int numberOfThreads,int[] listOfVertices)
        {
            this.name = name;
            this.id = id;
            this.numberOfThreads = numberOfThreads;
            this.listOfVertices = listOfVertices;
           
        }
        public int VerticeId
        {
            get { return recordVert; }
        }
        public int VerticeDist
        {
            get { return recordDist; }
        }
        public int[] ListOfVertices
        {
            get
            {
                return listOfVertices;
            }
            set
            {
                listOfVertices = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
   
        public int NumberOfThreads
        {
            set
            {
                numberOfThreads = value;
            }
            get
            {
                return numberOfThreads;
            }
        }
  
        public int Identifier
        {
            get
            {
                return id;
            }
        }

        public IDuplexCallback Callback { get => callback; set => callback = value; }
        public bool IsDataReady { get => isDataReady; set => isDataReady = value; }
    }
}
