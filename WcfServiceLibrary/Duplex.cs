﻿using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Timers;

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



    class Client
    {
        private ClientData data;
        private Timer timer = new Timer();
        private bool isFree = false;
        private int locRecordVert = -1;
        private int locRecordDist = 1;
        private long totalTime=0;
        private long dataSyncTime = 0;
        private long comTime = 0;
        private DateTime timeStart;
        private DateTime timeStop;
        private TimeSpan interval;
        private bool isDone=false;
        
        public Client(ClientData data)
        {
            this.data = data;
        }
        public void StartTimeCounting()
        {
            timeStart = DateTime.Now;
        }
        public void StopTimeCounting()
        {
            timeStop = DateTime.Now;
            interval = timeStop - timeStart;
            comTime= (interval.Ticks * 100)-totalTime;
        }
        public long CommunicationTime
        {
            get
            {
                return comTime;
            }
        }
        public ClientData Data
        {
            get
            {
                return data;
            }
            set
            {
                value = data;
            }
        }

        public bool IsFree { get => isFree; set => isFree = value; }

        public delegate void delHandler(object source, ElapsedEventArgs e);
        public void SetTimer(delHandler handler,double interval)
        {
            timer.Elapsed += new ElapsedEventHandler(handler);
            timer.Interval = interval;
            timer.Enabled = true;
           
        }
        public void StopTimer()
        {
            timer.Enabled = false;
        }
        public Timer GetTimer()
        {
            return timer;
        }
        public void SetRecord(int vert, int dist)
        {
            if(locRecordDist> dist || locRecordVert == -1)
            {
                locRecordDist = dist;
                locRecordVert = vert;
            }
           

        }
        public void ClearRecord()
        {
            locRecordVert = -1;
            locRecordDist = -1;
            totalTime = 0;
            comTime = 0;
            isDone = false;
        }
        public int RecordVertice
        {
            get
            {
                return locRecordVert;
            }
        }
        public int RecordDistance
        {
            get
            {
                return locRecordDist;
            }
        }
      
        public long TotalTime { get => totalTime; set => totalTime = value; }
        public long DataSyncTime { get => dataSyncTime; set => dataSyncTime = value; }
        public bool IsDone { get => isDone; set => isDone = value; }
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class Duplex : IDuplex
    {
        private STAGE_TYPE stage;
        //private IDuplexCallback callback;
        private int[][] matrix;
        private bool isMatrixReady = false;
        private List<Client> listOfClients = new List<Client>();
        private System.Timers.Timer timer;
        private int numberOfClientsDone;
        private bool isWaiting = false;
        private VerticesManagement verticesMgmt = new VerticesManagement();
        private int recordVert = -1;
        private int recordDist = -1;
        private string fileName="test.csv";
        // czas calkowity
        DateTime startTotalTime;
        DateTime stopTotalTime;
        TimeSpan intervalTotalTime;
        long totalTime;
        int clientsDone = 0;

        //private ClientsManagement clientsMgmt = new ClientsManagement();

        private int numberOfVertsPerClient = 1;
        private int numberOfThreads = 1;
        private System.Timers.Timer[] timers;
        private float timeoutInterval = 240000;

        public Duplex()
        {
            //TestService();
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
                Printer.PrintInfo("Dołączył nowy klient - " + name);
                IDuplexCallback callback = OperationContext.Current.GetCallbackChannel<IDuplexCallback>();
                ClientData data = new ClientData(name, listOfClients.Count, 2, null);
                data.Callback = callback;
                data.bestDistance = -1;
                data.bestVertice = -1;
                listOfClients.Add(new Client(data));
                //clientsMgmt.AddClient(data);


                callback.Message("Dołączyłeś do hosta.");
                //callback.JoinAccept();
                return data;
            }
            Printer.PrintErr("Błąd połączenia z klientem");
            return null;
        }
            
        public void SetMatrix(int[][] matrix)
        {
            this.matrix = matrix;
            isMatrixReady = true;
        }
        

        public int[][] GetMatrixData()
        {
            return matrix;
        }
        public void SyncAllClientsData()
        {
            int len = this.matrix.Length;
         
            Printer.PrintInfo("Synchronizacja macierzy: "+ len);
            //clienMgmt.SyncMatrixData(matrix);

            //clientsMgmt.SyncMatrixData(this.matrix);
            
            for(int i=0;i<listOfClients.Count;i++)
            {

                 ClientData c = listOfClients[i].Data;
                 try
                 {
                    Console.Write("{0}: ", c.name);

                    DateTime start = DateTime.Now;
                    DateTime stop;
                    TimeSpan interval;

                    if (len == c.Callback.DataSync(this.matrix))
                    {
                        stop = DateTime.Now;
                        interval = stop - start;
                        long time = interval.Ticks * 100;

                        Console.Write(" " + time + " nanosec ");
                        Console.WriteLine("OK");
                        listOfClients[i].DataSyncTime = time;
                        c.IsDataReady = true;

                    }
                    else Console.WriteLine("BŁĄD");
                }
                catch (TimeoutException ce)
                {
                    Console.WriteLine("Timeout exception");
                    Console.WriteLine("Błąd połączenia z {0} ID={1}, usunięcie klienta.", c.Name, c.Identifier);
                    listOfClients.Remove(listOfClients[i]);
                }
                

            }
            
        }
        public void Stats()
        {
            Console.WriteLine("NAME \t\t\t ID \t DATASYNC \t THREADS \t VERTICEID \t DISTANCE");
            foreach(Client cc in listOfClients)
            {
                ClientData c = cc.Data;
                Console.WriteLine("{0} \t {1} \t {2} \t\t {3} \t\t {4} \t\t {5}", c.Name,c.Identifier,c.IsDataReady,c.NumberOfThreads,cc.RecordVertice,cc.RecordDistance);
            }
            Console.WriteLine("Najlepszy wynik to, wierzchołek:{0} dystans:{1}", recordVert, recordDist);
        }
        public void BriefAllClients(int numberOfthreads,int numberOfVertsInPacket,string fileName)
        {
            this.numberOfVertsPerClient = numberOfVertsInPacket;
            this.numberOfThreads = numberOfThreads;
            this.fileName = fileName;

            if (stage == STAGE_TYPE.DATA_SYNC)
            {
                _Brief(numberOfthreads, numberOfVertsPerClient);
                //Brief(numberOfthreads, matrix.Length,generateArrayOfVertices(matrix.Length));
            }
            else Console.WriteLine("Dane nie są synchronizowane");

            
        }
        private int[] generateArrayOfVertices(int size)
        {
            int[] arr = new int[size];
            for(int i=0;i<size;i++)
            {
                arr[i] = i;
            }
            return arr;
        }
        private int[] getSubArrayOfVertices(int[] arr,int beg, int number)
        {

            int[] sub = new int[number];

            for(int i=0;i<number;i++)
            {
                sub[i] = arr[beg+i];
            }
            return sub;
        }
        private void _Brief(int numberOfthreads,int vertsPerClients)
        {
            Printer.PrintInfo("Odprawa klientów");
            recordDist = -1;
            recordVert = -1;
            


            int numberOfClients = listOfClients.Count;
            int matrixSize = matrix.Length;

            verticesMgmt.GenerateVertices(matrixSize);


            if (numberOfClients > matrixSize)
            {
                Printer.PrintWarn("Za dużo kientów!");
                return;
            }
            else if(numberOfClients < 1)
            {
                Printer.PrintWarn("Brak klientów");
                return;
            }
            /*
            foreach(var item in clientsMgmt.GetList())
            {
                clientsMgmt.SetTask(item.Data.Identifier,verticesMgmt.GetVertices(numberOfVertsPerClient),2000);
            }
            */

            
            for(int i=0;i<listOfClients.Count;i++)
            {
                listOfClients[i].Data.Callback.Reset();
                listOfClients[i].ClearRecord();
                listOfClients[i].Data.ListOfVertices = verticesMgmt.GetVertices(numberOfVertsPerClient);
                listOfClients[i].Data.numberOfThreads = numberOfthreads;
                //listOfClients[i].Callback.SendData(c);
            }
            
        }
        private void Brief(int numberOfthreads,int matrixSize,int[] arrSource)
        {
            Console.WriteLine("Briefing");
            int numberOfClients = listOfClients.Count;
            //int matrixSize = matrix.Length;
            int[] verticesPerClient = new int[numberOfClients];

            double ratio = (double)arrSource.Length / numberOfClients;
            bool extraVertice = false;
            int vertices = (int)ratio;
            if (ratio % 1 != 0) extraVertice = true;
            Console.WriteLine("Numer wierzchołka na klienta:");
            for (int i=0;i<numberOfClients;i++)
            {
                verticesPerClient[i] = vertices;

                if(i+1 == numberOfClients && extraVertice)
                {
                    verticesPerClient[i]+=1;
                }
                Console.WriteLine("{0};{1}",i, verticesPerClient[i]);
            }

            for (int i=0;i< numberOfClients; i++)
            {
                ClientData c = listOfClients[i].Data;
                c.NumberOfThreads = numberOfthreads;
                c.listOfVertices = getSubArrayOfVertices(arrSource,(i==0) ? 0: verticesPerClient[i - 1] * i,verticesPerClient[i]);//getListOfVertices((i == 0) ? 0 : verticesPerClient[i - 1]*i, verticesPerClient[i]);

                int iip = 0;
                Console.Write("Wątek {0}: ", c.id);
                foreach (int ii in c.listOfVertices)
                { 
                    Console.Write("{0};", ii);
                }
                Console.WriteLine();
                try
                {
                    c.Callback.Test();
                }
                catch(TimeoutException toe)
                {
                    Console.WriteLine("{0}: Utracono połączenie, usunięcie z listy.",c.Name);
                    listOfClients.Remove(listOfClients[i]);
                    continue;
                }

                listOfClients[i].Data.listOfVertices = c.listOfVertices;
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
        private void OnTimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            
            Console.WriteLine("Timeout - start ");

            var timer = source as Timer;
         
            foreach(var c in listOfClients)
            {
                if (c.GetTimer().Equals(timer))
                {
                    Console.WriteLine("Timer:Usunołem klienta {0}\n",c.Data.Identifier);
                    verticesMgmt.FreeVertices(c.Data.ListOfVertices);
                    c.StopTimer();
                    listOfClients.Remove(c);


                    for(int i = 0; i < listOfClients.Count; i++)
                    {
                        if (listOfClients[i].IsFree)
                        {
                            listOfClients[i].IsFree = false;
                            listOfClients[i].Data.listOfVertices = verticesMgmt.GetVertices(numberOfVertsPerClient);
                            listOfClients[i].Data.Callback.SendData(listOfClients[i].Data);
                            listOfClients[i].SetTimer(OnTimeEvent, timeoutInterval);
                        }
                    }


                    return;
                }
            }
            Console.WriteLine("nie odnalazlem tego alarmu");

            timer.Enabled = false;

            int ii = 0;
            foreach (var c in listOfClients)
                if (c.IsFree)
                    ii++;
                else return;

            if(ii==listOfClients.Count)
            {
                foreach (var c in listOfClients)
                    c.Data.Callback.JoinAccept();
            }

            /*
            System.Timers.Timer tm = source as System.Timers.Timer;
            tm.Enabled = false;
            List<int> listOfLostVertices = new List<int>();
            for(int i=0;i<listOfClients.Count;i++)
            {
                if(listOfClients[i].Data.bestDistance<0)
                {
                    //Console.WriteLine("found lost {0}",listOfClients[i].Identifier);
                    listOfLostVertices.AddRange(listOfClients[i].Data.listOfVertices);
                    listOfClients.Remove(listOfClients[i]);
                }
            }

            Console.WriteLine("Lista utraconych wierzchokłów [{0}]:",listOfLostVertices.Count);
            foreach(int v in listOfLostVertices){
                Console.Write("{0};", v);
            }
            Console.WriteLine();
           // _Brief(1, 2);
            //Brief(1,matrix.Length,listOfLostVertices.ToArray());
            Execute();
            Console.WriteLine("Timeout - end");
            */
        }
        public void Execute()
        {
            Console.WriteLine("Liczba klientów:{0}", listOfClients.Count);
            /*
            try
            {
                foreach (var item in clientsMgmt.GetList())
                {
                    clientsMgmt.SetTask(item.Data.Identifier, verticesMgmt.GetVertices(numberOfVertsPerClient), timeoutInterval);
                }
            }
            catch (TimeoutException toe)
            {
                Console.WriteLine("Utracono połączenie, usunięcie z listy.");
            }
            catch (CommunicationException cex)
            {
                Printer.PrintWarn("Błąd komunikacji");
            }
            */

            // start

            startTotalTime = DateTime.Now;
            

            for (int i=0;i<listOfClients.Count;i++)
            {
                 
                ClientData c = listOfClients[i].Data;
                listOfClients[i].StartTimeCounting();

                try
                {
                    c.Callback.SendData(c);
                    //listOfClients[i].SetTimer(OnTimeEvent, timeoutInterval);
                    Console.WriteLine("Wysłano do {0}", c.Identifier);
                    listOfClients[i].SetTimer(OnTimeEvent, timeoutInterval);
                }
                catch (TimeoutException toe)
                {
                    Console.WriteLine("{0}: Utracono połączenie, usunięcie z listy.", c.Name);
                }
                catch (CommunicationException cex)
                {
                    Printer.PrintWarn("Błąd komunikacji");
                }
                
            }
            /*
             timer = new System.Timers.Timer();
             timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeEvent);
             timer.Interval = 3000;
             timer.Enabled = true;
             numberOfClientsDone = 0;*/
        }
    
        public void JoinClients()
        {
            Console.WriteLine("Service: Waiting for clients");
            listOfClients.Clear();
            stage = STAGE_TYPE.JOIN;
        }
        public void SyncClientsData()
        {
            Printer.PrintInfo("Service: Data synchronized");
            if (isMatrixReady)
            {
                stage = STAGE_TYPE.DATA_SYNC;
                SyncAllClientsData();
            }
            else Console.WriteLine("Matrix fault");
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
            /*
            clientsMgmt.GetResult(clientData.Identifier);
            verticesMgmt.SubmitVertices(clientData.ListOfVertices);
            Console.WriteLine("Klient: {0}; Wierzchołek: {1}; Dystans: {2};", clientData.Identifier, clientData.bestVertice, clientData.bestDistance);
            if(!verticesMgmt.IsListEmpty())
            {
                clientsMgmt.SetTask(clientData.Identifier, verticesMgmt.GetVertices(numberOfVertsPerClient), timeoutInterval);
            }
            */


            for (int i = 0; i < listOfClients.Count; i++)
            {
               
                if(listOfClients[i].Data.Identifier ==clientData.id)
                {
                    if (clientData.bestDistance == 0) continue;

                    //Console.WriteLine("Otrzymałem wynik od {0} najkrótszy dystans to:{1} dla wierzchołka {2}", clientData.id, clientData.bestDistance,clientData.bestVertice);
                    verticesMgmt.SubmitVertices(listOfClients[i].Data.listOfVertices);

                    listOfClients[i].TotalTime += clientData.time;

                    listOfClients[i].SetRecord(clientData.bestVertice, clientData.bestDistance);

                    if(recordVert==-1)
                    {
                        recordVert = clientData.bestVertice;
                        recordDist = clientData.bestDistance;
                        Printer.PrintRecord(recordVert, recordDist);
                    }
                    else if (recordDist > clientData.bestDistance)
                    {
                        recordVert = clientData.bestVertice;
                        recordDist = clientData.bestDistance;
                        Printer.PrintRecord(recordVert, recordDist);
                    }
                   

                    if (!verticesMgmt.IsListEmpty())
                    {

                        listOfClients[i].SetTimer(OnTimeEvent, timeoutInterval);
                        listOfClients[i].Data.listOfVertices = verticesMgmt.GetVertices(numberOfVertsPerClient);

                        listOfClients[i].Data.Callback.SendData(listOfClients[i].Data);
                        listOfClients[i].IsFree = false;

                    }
                    else
                    {
                        listOfClients[i].StopTimer();
                        listOfClients[i].IsFree = true;
                        listOfClients[i].StopTimeCounting();
                        //listOfClients[i].Data.Callback.JoinAccept();
                        
                        Console.WriteLine("Lista jest pusta");
                        Console.WriteLine("Liczba skonczonych klientow:{0}, wszyscy klienci:{1}", clientsDone, listOfClients.Count);
                        bool isAllDone = true;
                        if (!listOfClients[i].IsDone)
                        {
                            listOfClients[i].IsDone = true;

                            ++clientsDone;
                            Console.WriteLine("Liczba klientow skonczonych:{0}", clientsDone);

                            foreach(Client cli in listOfClients)
                            {
                                if (!cli.IsDone)
                                {
                                    isAllDone = false;
                                    break;
                                }
                                    
                            }

                        }



                        if (isAllDone)
                        {
                            Console.WriteLine("Progam zakonczony");
                            stopTotalTime = DateTime.Now;
                            intervalTotalTime = stopTotalTime - startTotalTime;
                            totalTime = intervalTotalTime.Ticks * 100;

                            long clientsTotalTime = 0;
                            long clientsComTime = 0;
                            foreach (Client c in listOfClients)
                            {
                                clientsTotalTime += c.TotalTime;
                                clientsComTime += c.CommunicationTime;
                            }
                            Console.WriteLine("Czas algorytmu:" + clientsTotalTime);
                            long commTotalTime = clientsComTime;
                            Console.WriteLine("czas komunikacji:" + commTotalTime);
                            totalTime = commTotalTime + clientsTotalTime;
                            Console.WriteLine("Czas całkowiy:" + totalTime);
                            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
                            StreamWriter w = new StreamWriter(fileStream, Encoding.UTF8);
                            w.WriteLine("Ilosc wierzchołków;ilość watkow;ilosc klientow;wielkosc pakietu;czas calk.;czas kom.;czas alg.;rekord nr.;rekord dystans");
                            w.WriteLine(matrix.Length + ";" + numberOfThreads + ";" + clientsDone + ";" + numberOfVertsPerClient + ";" + totalTime + ";" + commTotalTime + ";" + clientsTotalTime + ";" + recordVert + ";" + recordDist);
                            /*            for (int i = 0; i < rozmiar; i++)
                                        {
                                            for (int j = 0; j < rozmiar; j++)
                                            {
                                                if (j < rozmiar - 1)
                                                    w.Write(matrix[i, j] + " ");
                                                else
                                                    w.WriteLine(matrix[i, j]);
                                            }
                                        }
                                        */
                            w.Close();
                            fileStream.Close();

                            fileStream = new FileStream("workers_" + fileName, FileMode.Create, FileAccess.ReadWrite);
                            w = new StreamWriter(fileStream, Encoding.UTF8);
                            w.WriteLine("klient id.;klient nazwa;czas synch. danych;czas alg.;czas kom.;wierzcholek nr.;wierzcholek dystans");
                            foreach (Client c in listOfClients)
                            {
                                w.WriteLine(c.Data.Identifier + ";" + c.Data.Name + ";" + c.DataSyncTime + ";" + c.TotalTime + ";" + c.CommunicationTime + ";" + c.RecordVertice + ";" + c.RecordDistance);
                            }
                            /*            for (int i = 0; i < rozmiar; i++)
                                        {
                                            for (int j = 0; j < rozmiar; j++)
                                            {
                                                if (j < rozmiar - 1)
                                                    w.Write(matrix[i, j] + " ");
                                                else
                                                    w.WriteLine(matrix[i, j]);
                                            }
                                        }
                                        */
                            w.Close();
                            fileStream.Close();

                        }
                        else Console.WriteLine("nie wszyscy klienci gotowi");
                    }
                }
            }
            
        }
        public void FreeAll()
        {

        }
                /*
                public void SendResult(ClientData clientData)
                {
                    for(int i=0;i<listOfClients.Count;i++)
                    {
                        if(listOfClients[i].Identifier == clientData.Identifier)
                        {
                            Console.WriteLine("Otrzymałem wynik od {0} najkrótszy dystans to:{1}", clientData.id, clientData.bestDistance);
                            listOfClients[i].bestDistance = clientData.bestDistance;
                            listOfClients[i].bestVertice = clientData.bestVertice;
                            if (++numberOfClientsDone == listOfClients.Count)
                            
                                timer.Enabled = false;
                                foreach(ClientData c in listOfClients)
                                {
                                    c.Callback.JoinAccept();
                                }
                                // koniec algorytmu
                            }
                            Console.WriteLine(numberOfClientsDone + "/" + listOfClients.Count);

                            return;
                        }
                    }
                    Console.WriteLine("Nie rozpoznano nadawcy wiadomosci.");

                } */
            }
}
