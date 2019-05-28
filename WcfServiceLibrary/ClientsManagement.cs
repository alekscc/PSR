using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace WcfServiceLibrary
{
    public enum CLIENT_STATUS
    {
        NONE,
        WORKING,
        TIMEOUT,
        DONE
    }

    class ClientInfo
    {
        private Timer timer;
        private ClientData data;
        private bool isDone;
        private CLIENT_STATUS status;
        

        public ClientInfo(ClientData data)
        {
            timer = new Timer();
            this.Data = data;
            status = CLIENT_STATUS.NONE;
        }
        private void setStatus(CLIENT_STATUS status)
        {
            this.status = status;
            switch (this.status)
            {
                case CLIENT_STATUS.DONE:
                    timer.Enabled = false;
                    break;
                case CLIENT_STATUS.TIMEOUT:
                    break;
                case CLIENT_STATUS.WORKING:
                    break;
            }
        }

        public ClientData Data { get => data; set => data = value; }

        public void SendTask(int[] arr, double interval)
        {
            Data.listOfVertices = arr;
            Data.Callback.SendData(Data);
            setStatus(CLIENT_STATUS.WORKING);
            timer.Interval = interval;
            timer.Elapsed += new ElapsedEventHandler(OnTimeEvent);
            timer.Enabled = true;
           
           
        }
        public void OnTimeEvent(object source,ElapsedEventArgs args)
        {
            //Timer t = source as Timer;

            timer.Enabled = false;
            setStatus(CLIENT_STATUS.TIMEOUT);
            Console.WriteLine("Klient {0} - koniec czasu", Data.Identifier);

        }
        public void SetDone()
        {
            //this.data = data;
            setStatus(CLIENT_STATUS.DONE);
            
        }
       
        
        
    }

    class ClientsManagement
    {
        private List<ClientInfo> listOfClients = new List<ClientInfo>();
        private VerticesManagement vertsMgmt = new VerticesManagement();

        public ClientsManagement()
        {

        }
        public void AddClient(ClientData clientData)
        {
            listOfClients.Add(new ClientInfo(clientData));
        }
        public ClientData GetClient(int id)
        {
            foreach (var item in listOfClients)
            {
                if(item.Data.Identifier ==id)
                {
                    return item.Data;
                }
            }
            return null;
        }
        public void DeleteClient(int id)
        {
            foreach(var item in listOfClients)
            {
                if(item.Data.Identifier==id)
                {
                    listOfClients.Remove(item);
                }
            }
        }
        public void SetTask(int id,int[] verts,double interval)
        {
            foreach (var item in listOfClients)
            {
                if (item.Data.Identifier == id)
                {
                    item.SendTask(verts, interval);
                }
            }

        }
        public void SetTaskAll(int vertsPerClient,double interval)
        {
            foreach(var item in listOfClients)
            {
                item.SendTask(vertsMgmt.GetVertices(vertsPerClient),interval);
            }
        }
        public List<ClientInfo> GetList()
        {
            return listOfClients;
        }
       // public void SetTaskAll(int vertsPerClient,int )
        public void SyncMatrixData(int[][] arr)
        {
            
            vertsMgmt.GenerateVertices(arr.Length);
            Printer.PrintInfo("Generowanie macierzy, ilość wierzchołków="+ arr.Length);
            foreach (var item in listOfClients)
            {

                try
                {
                    item.Data.Callback.DataSync(arr);
                
                }catch (TimeoutException ce)
                {
                    Console.WriteLine("Timeout exception");
                    Console.WriteLine("Błąd połączenia z {0} ID={1}, usunięcie klienta.", item.Data.Name, item.Data.Identifier);
                    listOfClients.Remove(item);
                }
            }
         
        }
        public void GetResult(int id)
        {
            foreach (var item in listOfClients)
            {
                if (item.Data.Identifier == id)
                {
                    item.SetDone();
                }
            }
        }

    }
}
