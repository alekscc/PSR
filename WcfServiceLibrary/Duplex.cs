using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace WcfServiceLibrary
{
    public enum STAGE_TYPE
    {
        JOIN,
        DATA_SYNC,
        BRIEF,
        EXECUTE,
        STATS
    }
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class Duplex : IDuplex
    {
        private STAGE_TYPE stage;
        //private IDuplexCallback callback;
        private int[][] matrix;
        private bool isMatrixReady = false;
        private List<ClientData> listOfClients = new List<ClientData>();
        

        public Duplex()
        {
            TestService();
            //Console.WriteLine("Duplex start");
            stage = STAGE_TYPE.JOIN;
            ServiceHostInterface serviceHostInterface = ServiceHostInterface.GetInstance();
            serviceHostInterface.SingletonInstance = this;
            Console.WriteLine("Instacja serwisu jest gotowa.");
            //callback = OperationContext.Current.GetCallbackChannel<IDuplexCallback>();

            
            
        }
        public ClientData Join(string name)
        {
            if (stage == STAGE_TYPE.JOIN)
            {
                Console.WriteLine("Dołączył nowy klient - {0}.",name);
                IDuplexCallback callback = OperationContext.Current.GetCallbackChannel<IDuplexCallback>();
                ClientData data = new ClientData(name, listOfClients.Count, 2, null);
                data.Callback = callback;
                listOfClients.Add(data);
                callback.Message("Dołączyłeś do hosta.");
                callback.JoinAccept();
                return data;
            }
            Console.WriteLine("returning null");
            return null;
        }
        public void SetMatrix(int[][] matrix)
        {
            this.matrix = matrix;
            isMatrixReady = true;
        }
        public void BroadcastMessage(string message)
        {
            foreach(ClientData c in listOfClients)
            {
                c.Callback.Message(message);
            }
        }

        public void TestService()
        {
            Console.WriteLine("Service test.");
            //callback.Message("testuje klienta");
        }

        public int[][] GetMatrixData()
        {
            return matrix;
        }

        public void SyncAllClientsData()
        {
            int len = this.matrix.Length;
            Console.WriteLine("Rozmiar macierzy:{0}", len);
            for(int i=0;i<listOfClients.Count;i++)
            {

                ClientData c = listOfClients[i];
                 try
                 {
                    Console.Write("{0}: ", c.name);
                    if (len == c.Callback.DataSync(this.matrix))
                    {
                        Console.WriteLine("OK");
                        c.IsDataReady = true;

                    }
                    else Console.WriteLine("BŁĄD");
                }
                catch (TimeoutException ce)
                {
                    Console.WriteLine("Timeout exception");
                    Console.WriteLine("Błąd połączenia z {0} ID={1}, usunięcie klienta.", c.Name, c.Identifier);
                    listOfClients.Remove(c);
                }


            }

        }
        public void Stats()
        {
            Console.WriteLine("NAME \t\t\t ID \t DATASYNC \t THREADS \t VERTICEID \t VERTICEDIST");
            foreach(ClientData c in listOfClients)
            {
                Console.WriteLine("{0} \t {1} \t {2} \t\t {3} \t\t {4} \t\t {5}", c.Name,c.Identifier,c.IsDataReady,c.NumberOfThreads,c.VerticeId,c.VerticeDist);
            }
        }
        public void Brief(int numberOfthreads)
        {
            int numberOfClients = listOfClients.Count;
            int matrixSize = matrix.Length;
            int[] verticesPerClient = new int[numberOfClients];

            double ratio = (double)matrixSize / numberOfClients;
            bool extraVertice = false;
            int vertices = (int)ratio;
            if (ratio % 1 != 0) extraVertice = true;

            for(int i=0;i<numberOfClients;i++)
            {
                verticesPerClient[i] = vertices;

                if(i+1 == numberOfClients && extraVertice)
                {
                    verticesPerClient[i]+=1;
                }
                Console.WriteLine("{0} num of verts = {1}",i, verticesPerClient[i]);
            }

            for (int i=0;i< numberOfClients; i++)
            {
                ClientData c = listOfClients[i];
                c.NumberOfThreads = numberOfthreads;
                c.listOfVertices = getListOfVertices((i == 0) ? 0 : verticesPerClient[i - 1]*i, verticesPerClient[i]);

                int iip = 0;
                Console.WriteLine("THREAD {0}", c.id);
                foreach (int ii in c.listOfVertices)
                { 
                    Console.WriteLine("{0} vert: {1}", iip++, ii);
                }
                
                try
                {
                    c.Callback.Test();
                }
                catch(TimeoutException toe)
                {
                    Console.WriteLine("{0}: Utracono połączenie, usunięcie z listy.",c.Name);
                    listOfClients.Remove(c);
                    continue;
                }

                listOfClients[i].listOfVertices = c.listOfVertices;
            }
        }
        private int[] getListOfVertices(int beg,int n)
        {
            int[] list = new int[n];
            for(int i=0;i<n;i++)
            {
                list[i] = beg + i;
            }

            return list;
        }
        public void Execute()
        {
            for(int i=0;i<listOfClients.Count;i++)
            {
                ClientData c = listOfClients[i];

                try
                {
                    c.Callback.SendData(c);
                }
                catch(TimeoutException toe)
                {
                    Console.WriteLine("{0}: Utracono połączenie, usunięcie z listy.", c.Name);
                }
                catch(CommunicationException cex)
                {
                    Console.WriteLine("{0}:BłĄD CEX", c.name);
                }
                
            }
        }


        public void SetStage(STAGE_TYPE type)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            switch(type)
            {
                case STAGE_TYPE.JOIN:
                    {
                        Console.WriteLine("Serwis: Oczekiwanie na klientów");
                        listOfClients.Clear();
                        stage = type;
                    }
                    break;
                case STAGE_TYPE.DATA_SYNC:
                    {
                        Console.WriteLine("Serwis: Synchronizacja danych");
                        if (isMatrixReady)
                        {
                            stage = type;
                            SyncAllClientsData();
                        }
                        else Console.WriteLine("Błąd macierz niezaładowana");
                    }
                    break;
                case STAGE_TYPE.EXECUTE:
                    {
                        Console.WriteLine("Serwis: Wykonywanie algorytmu");
                    }
                    break;

            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void SendResult(ClientData clientData)
        {
            Console.WriteLine("Otrzymałem wynik od {0} najkrótszy dystans to:{1}", clientData.id, clientData.bestDistance);
        }
    }
}
