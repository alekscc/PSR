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
        BRIEFING,
        EXECUTE,
        
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
        private string fileName = "test.csv";
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
        private float timeoutInterval = 60000;//240000;
        private const int MaxTimeout = 10;

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
                IDuplexCallback callback = OperationContext
                    .Current.GetCallbackChannel<IDuplexCallback>();
                ClientData data = new ClientData(name,
                                            listOfClients.Count,
                                            2,
                                            null);
                data.Callback = callback;
                data.recordDist = -1;
                data.recordVert = -1;
                listOfClients.Add(new Client(data));


                callback.Message("Dołączyłeś do hosta.");
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
        public void SetTimeout(int mins)
        {
            int to = (mins > MaxTimeout) ? MaxTimeout : (mins < 0) ? 0 : mins;

            Printer.PrintInfo("Ustawiono timeout na " + to + " minut");

            timeoutInterval = 60000 * to;

        }


        public int[][] GetMatrixData()
        {
            return matrix;
        }
        public void SyncAllClientsData()
        {
            int len = this.matrix.Length;

            Printer.PrintInfo("Synchronizacja macierzy: " + len);

            for (int i = 0; i < listOfClients.Count; i++)
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
                    Console.WriteLine("Błąd połączenia z {0} ID={1}," +
                        " usunięcie klienta.", c.Name, c.Identifier);
                    listOfClients.Remove(listOfClients[i]);
                }


            }

        }
        public void Stats()
        {
            Printer.PrintStats(listOfClients, recordVert, recordDist);
        }
        public void BriefAllClients(int numberOfthreads, int numberOfVertsInPacket, string fileName)
        {
            Printer.PrintInfo("Odprawa klientów");
            this.numberOfVertsPerClient = numberOfVertsInPacket;
            this.numberOfThreads = numberOfThreads;
            this.fileName = fileName;

            if (stage == STAGE_TYPE.DATA_SYNC)
            {
                _Brief(numberOfthreads, numberOfVertsPerClient);
               
            }
            else Console.WriteLine("Dane nie są zsynchronizowane");


        }
        private int[] generateArrayOfVertices(int size)
        {
            int[] arr = new int[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = i;
            }
            return arr;
        }
        private int[] getSubArrayOfVertices(int[] arr, int beg, int number)
        {

            int[] sub = new int[number];

            for (int i = 0; i < number; i++)
            {
                sub[i] = arr[beg + i];
            }
            return sub;
        }
        private void _Brief(int numberOfthreads, int vertsPerClients)
        {

            recordDist = -1;
            recordVert = -1;
            clientsDone = 0;

            int numberOfClients = listOfClients.Count;
            int matrixSize = matrix.Length;

            verticesMgmt.GenerateVertices(matrixSize);


            if (numberOfClients > matrixSize)
            {
                Printer.PrintWarn("Za dużo kientów!");
                return;
            }
            else if (numberOfClients < 1)
            {
                Printer.PrintWarn("Brak klientów");
                return;
            }

            for (int i = 0; i < listOfClients.Count; i++)
            {
                listOfClients[i].Data.Callback.Reset();
                listOfClients[i].ClearRecord();
                listOfClients[i].Data.ListOfVertices = verticesMgmt
                    .GetVertices(numberOfVertsPerClient);
                listOfClients[i].Data.numberOfThreads = numberOfthreads;
                listOfClients[i].Data.commTime = 0;
                listOfClients[i].Data.time = 0;
            }

            Printer.PrintInfo("Wszystko gotowe. Wpisz start, aby rozpocząc");

        }
        private void OnTimeEvent(object source, System.Timers.ElapsedEventArgs e)
        {

            Printer.PrintWarn("Timeout");

            var timer = source as Timer;

            foreach (var c in listOfClients)
            {
                if (c.GetTimer().Equals(timer))
                {
                    Printer.PrintWarn("Timer:Usunołem klienta " + c.Data.Identifier);
                    //Console.WriteLine("ilosc wolnych wierzcholkow: {0}", verticesMgmt.GetNumberOfFreeVertices());
                    verticesMgmt.FreeVertices(c.Data.ListOfVertices);
                    //Console.WriteLine("ilosc wolnych wierzcholkow po free: {0}", verticesMgmt.GetNumberOfFreeVertices());
                    c.StopTimer();
                    listOfClients.Remove(c);

                    for (int i = 0; i < listOfClients.Count; i++)
                    {
                        if (listOfClients[i].IsFree)
                        {
                            Printer.PrintInfo("Klient " + listOfClients[i].Data.Identifier + " przejmuje zadanie");
                            listOfClients[i].IsFree = false;
                            listOfClients[i].Data.listOfVertices = verticesMgmt.GetVertices(numberOfVertsPerClient);
                            listOfClients[i].Data.date = DateTime.Now;
                            listOfClients[i].Data.Callback.SendData(listOfClients[i].Data);
                            listOfClients[i].SetTimer(OnTimeEvent, timeoutInterval);
                            listOfClients[i].LifeTime.Unpause();
                            break;
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

            if (ii == listOfClients.Count)
            {
                foreach (var c in listOfClients)
                    c.Data.Callback.JoinAccept();
            }
        }
        public void Execute()
        {

            if (stage != STAGE_TYPE.BRIEFING && stage != STAGE_TYPE.EXECUTE)
            {
                Printer.PrintInfo("Użyj polecenia brief do przygotowania klientów");
            }

            Printer.PrintInfo("Liczba klientów:"+ listOfClients.Count);

            startTotalTime = DateTime.Now;
            

            for (int i=0;i<listOfClients.Count;i++)
            {
                 
                ClientData c = listOfClients[i].Data;
                

                try
                {
                    listOfClients[i].LifeTime.Start();
                    listOfClients[i].StartTimeCounting();
                    listOfClients[i].Data.date = DateTime.Now;
                    Console.WriteLine("Wysłano do {0}", c.Identifier);
                    listOfClients[i].SetTimer(OnTimeEvent, timeoutInterval);
                    c.Callback.SendData(c);
     
                    
                }
                catch (TimeoutException toe)
                {
                    Console.WriteLine("{0}: Utracono połączenie, usunięcie z listy.", c.Name);
                }
                catch (CommunicationException cex)
                {
                    Printer.PrintWarn("Błąd komunikacji");
                }

                listOfClients[i].PauseTimeCounting();

            }
            Printer.PrintInfo("Uruchomiono algorytm");

        }
    
        public void OpenForClients()
        {

            Printer.PrintInfo("Nowi klienci mogą się teraz dołączyć");
            listOfClients.Clear();
            stage = STAGE_TYPE.JOIN;
        }
        public void SyncClientsData()
        {
            Printer.PrintInfo("Synchronizacja danych");
            if (isMatrixReady)
            {
                stage = STAGE_TYPE.DATA_SYNC;
                SyncAllClientsData();
            }
            else Console.WriteLine("Błąd macierzy");
        }

        public void SendResult(ClientData clientData)
        {

            for (int i = 0; i < listOfClients.Count; i++)
            {
               
                if(listOfClients[i].Data.Identifier ==clientData.id)
                {
                    if (clientData.recordDist == 0) continue;
                    TimeSpan inter = DateTime.Now - clientData.date;
                    listOfClients[i].Data.commTime += inter.Ticks * 100 + clientData.commTime;
                    listOfClients[i].UnPauseTimeCounting();
                    verticesMgmt.SubmitVertices(listOfClients[i].Data.listOfVertices);

                    listOfClients[i].AddTotalTime(clientData.time);
                    listOfClients[i].SetRecord(clientData.recordVert, clientData.recordDist);

                    if(recordVert==-1)
                    {
                        recordVert = clientData.recordVert;
                        recordDist = clientData.recordDist;
                        Printer.PrintRecord(recordVert, recordDist);
                    }
                    else if (recordDist > clientData.recordDist)
                    {
                        recordVert = clientData.recordVert;
                        recordDist = clientData.recordDist;
                        Printer.PrintRecord(recordVert, recordDist);
                    }
                   

                    if (!verticesMgmt.IsListEmpty())
                    {

                        listOfClients[i].SetTimer(OnTimeEvent, timeoutInterval);
                        listOfClients[i].Data.listOfVertices = verticesMgmt.GetVertices(numberOfVertsPerClient);
                        listOfClients[i].Data.date = DateTime.Now;
                        listOfClients[i].Data.Callback.SendData(listOfClients[i].Data);
                        
                            listOfClients[i].IsFree = false;
                            //Console.WriteLine("Wysłałem wierzcholek do klienta {0}", listOfClients[i].Data.Identifier);
                            listOfClients[i].PauseTimeCounting();
                        

                    }
                    else
                    {
                        listOfClients[i].StopTimer();
                        listOfClients[i].IsFree = true;
                        listOfClients[i].StopTimeCounting();
                        //listOfClients[i].Data.Callback.JoinAccept();
                        
                        Console.WriteLine("Brak wolnych wierzchołków dla klienta:{0}",listOfClients[i].Data.Identifier);
                        //Console.WriteLine("Liczba skonczonych klientow:{0}, wszyscy klienci:{1}", clientsDone, listOfClients.Count);
                        bool isAllDone = true;
                        if (!listOfClients[i].IsDone)
                        {
                            listOfClients[i].IsDone = true;
                            listOfClients[i].LifeTime.Pause();

                            foreach(Client cli in listOfClients)
                            {
                                if (!cli.IsDone)
                                {
                                    isAllDone = false;
                                    Console.WriteLine("Liczba klientow zakonczonych:{0}", ++clientsDone);
                                    break;
                                }
                                    
                            }
                            

                        }



                        if (isAllDone)
                        {
                    

                            long clientsTotalTime = 0;
                            long clientsMaintainTime = 0;
                            long clientsCommTime = 0;
                            foreach (Client c in listOfClients)
                            {
                                c.LifeTime.Stop();
                                clientsMaintainTime += c.MaintainTime;
                                clientsTotalTime += c.TotalTime;
                                clientsCommTime += c.Data.commTime;
                            }
                            Console.WriteLine("Progam zakonczony");
                            stopTotalTime = DateTime.Now;
                            intervalTotalTime = stopTotalTime - startTotalTime;
                            totalTime = intervalTotalTime.Ticks * 100;

                            int numberOfClients = clientsDone + 1;
                            clientsTotalTime /= numberOfClients;
                            clientsCommTime /= numberOfClients;
                            //clientsTotalTime = totalTime - clientsComTime;
                            Console.WriteLine("Średni czas algorytmu:" + clientsTotalTime);
                           // long commTotalTime = clientsMaintainTime;
                            clientsMaintainTime /= numberOfClients;
                            Console.WriteLine("Średni czas obsługi:" + clientsMaintainTime);
                            Console.WriteLine("Średni czas kom:" + clientsCommTime);
                            //totalTime = commTotalTime + clientsTotalTime;
                            Console.WriteLine("Czas całkowity:" + totalTime);
                            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
                            StreamWriter w = new StreamWriter(fileStream, Encoding.UTF8);
                            
                            w.WriteLine("Ilosc wierzchołków;ilość watkow;ilosc klientow;wielkosc pakietu;czas calk.;czas kom.;śr. czas alg.; śr. czas obsługi;rekord nr.;rekord dystans");
                            w.WriteLine(matrix.Length + ";" + numberOfThreads + ";" + numberOfClients + ";" + numberOfVertsPerClient + ";" + totalTime + ";" + clientsCommTime + ";" + clientsTotalTime + ";"+clientsMaintainTime +";"+ recordVert + ";" + recordDist);
 
                            w.Close();
                            fileStream.Close();

                            fileStream = new FileStream("workers_" + fileName, FileMode.Create, FileAccess.ReadWrite);
                            w = new StreamWriter(fileStream, Encoding.UTF8);
                            w.WriteLine("klient id.;klient nazwa;czas zycia;czas synch. danych;czas całk.;czas obsl.;czas kom.;czas alg.;wierzcholek nr.;wierzcholek dystans");
                            foreach (Client c in listOfClients)
                            {
                                long time = c.TotalTime + c.MaintainTime+c.Data.commTime;
                                Console.WriteLine("Czas całk. dla {0} to {1}", c.Data.Identifier, time);
                                w.WriteLine(c.Data.Identifier + ";" + c.Data.Name + ";" + c.LifeTime.GetTime() +";"+ c.DataSyncTime + ";" + time + ";" + c.MaintainTime + ";" + c.Data.commTime + ";" + c.TotalTime + ";" + c.RecordVertice + ";" + c.RecordDistance);
                            }

                            w.Close();
                            fileStream.Close();
                            Console.WriteLine("Dane zapisane do {0} i {1}", fileName, "workers_" + fileName);

                        }
                        else Console.WriteLine("nie wszyscy klienci gotowi");
                    }
                }
            }
            
        }

            }
}
