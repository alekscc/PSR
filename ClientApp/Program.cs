using ClientApp.DuplexServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace ClientApp
{

    public struct BestResult
    {
        public int vertice;
        public int distance;
    }

    class Program
    {
        public static ManualResetEvent mre = new ManualResetEvent(false);

        public class CallbackHandler : IDuplexCallback
        {
            private bool isCalled;
            private int[][] matrix;
            private ClientData data = null;
            private BestResult bestResult;
            private bool isBestResult = false;
            private bool mutStatus = false;
            
  

            public CallbackHandler()
            {
              
            }
            public void Message(string message)
            {
                Console.WriteLine(message);
            }

            public void SendData(ClientData clientData)
            {
                Console.WriteLine("Odebrano dane od hosta");
                data = clientData;
                StartWork();
                
            }

            public int Test()
            {
                Console.WriteLine("Testing conncetion");
                return 1;
            }

            public int DataSync(int[][] matrix)
            {
                this.matrix = matrix;

                Console.WriteLine("Matrix length:{0}", matrix.Length);
                return matrix.Length;
            }
            public bool IsBestResult
            {
                get
                {
                    return isBestResult;
                }
            }

          

            public void StartWork()
            {

                if(this.matrix==null)
                {
                    Console.WriteLine("matrix is empty, quitting");
                    return;
                }
                
                if (data != null)
                {
    
                    Console.WriteLine("Algorytm uruchomiony");
                    ConcurrentProgram concurrent = new ConcurrentProgram(this.data.numberOfThreads, this.matrix,this.data.listOfVertices);
                    concurrent.Start();
                    this.bestResult.distance = concurrent.BestResult;
                    this.data.bestDistance = bestResult.distance;
                    isBestResult = true;

                }
                else Console.WriteLine("Brak danych o kliencie.");

                //SecuredSingleton.GetInstance().Release();
                mre.Set();
                mre.Reset();
                Console.WriteLine("Praca skończona");
            }

            public void JoinAccept()
            {
                //SecuredSingleton.GetInstance().Wait();

                //mut.WaitOne();
                //SecuredSingleton.GetInstance().Wait();
                //mut.WaitOne();
                Console.WriteLine("Join Accepted");

            }

            public bool IsCalled
            {
                get
                {
                    return isCalled;
                }
            }
            public ClientData ClientData
            {
                get
                {
                    return data;
                }
                set
                {
                    data = value;
                }
            }

        }
        
        static void Main(string[] args)
        {
            CallbackHandler callbackHandler = new CallbackHandler();
            InstanceContext instanceContext = new InstanceContext(callbackHandler);

            Console.WriteLine("Trwa nawiązywanie połączenia z hostem.");
            DuplexClient client = new DuplexClient(instanceContext);
            
            client.Open();
           
            //int[][] matrix = client.GetMatrixData();

            
           // concurrent.Start();

            Console.WriteLine("Aplikacja klienta działa.");
            
            ClientData data = client.Join(Environment.MachineName);
            if (data != null)
            {
                Console.WriteLine("Otrzymałem id: {0} jako {1}",data.id, data.name);
                Thread.Sleep(1000);
            }
            else Console.WriteLine("Błąd.");


            callbackHandler.ClientData = data;
            Console.WriteLine("Oczekiwanie na zadanie.");
            //callbackHandler.WaitForTask();
            //if (!callbackHandler.WaitForTask()) Console.WriteLine("mutex błąd");

            //SecuredSingleton.GetInstance().Wait();
            mre.WaitOne();
            //mut.WaitOne();
            //EventWaitHandle.SignalAndWait()
            if (callbackHandler.IsBestResult)
            {
                client.SendResult(callbackHandler.ClientData);
            }
            Console.WriteLine("Koncze zadanie");

            // callbackHandler.TaskDone();

            // SecuredSingleton.GetInstance().Release();
            //mut.ReleaseMutex();
            Console.WriteLine("Wciśnij <ENTER> aby zakończyć");

            
           // if (!client.Join(Environment.UserName)) Console.WriteLine("Błąd. Nie udało się dołączyć do hosta.");

            Console.ReadLine();
            client.Close();
        }
    }
}
