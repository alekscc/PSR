using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using WcfServiceLibrary;

namespace HostApp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Uri baseAddress = new Uri("http://localhost:8001/DuplexService/");

            ServiceHost selfHost = new ServiceHost(typeof(Duplex),
                                                    baseAddress);

            Console.WriteLine("Adres hosta:{0}", selfHost.BaseAddresses[0]);

            try
            {

                WSDualHttpBinding binding = new WSDualHttpBinding();
               
                binding.Security.Mode = WSDualHttpSecurityMode.None;

                selfHost.AddServiceEndpoint(typeof(IDuplex),
                    binding, "DuplexService");

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;

                selfHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
                selfHost.Description.Behaviors.Add(smb);
 
                selfHost.Open();
                Console.WriteLine("Stan hosta:{0}",selfHost.State);

                WcfServiceLibrary.ServiceHostInterface serviceHostInterface = WcfServiceLibrary.ServiceHostInterface.GetInstance();
                //serviceHostInterface.MatrixData = graph.matrix;
                Duplex instance = serviceHostInterface.SingletonInstance;


                string cmd;
                
                do
                {
                    cmd = Console.ReadLine();

                    if(cmd.CompareTo("test")==0)
                    {
                        instance.TestService();
                    }
                    else if(cmd.CompareTo("join")==0)
                    {
                        //instance.SetStage(STAGE_TYPE.JOIN);
                        instance.JoinClients();
                    }
                    else if(cmd.CompareTo("datasync")==0)
                    {
                        Graph graph = new Graph(@"../../macierz.txt");
                        instance.SetMatrix(graph.matrix);
                        //instance.SetStage(STAGE_TYPE.DATA_SYNC);
                        instance.SyncClientsData();
                    }
                    else if(cmd.CompareTo("brief")==0)
                    {
                        Console.WriteLine("Number of threads?");
                        string line = Console.ReadLine();
                        int num = int.Parse(line);

                        instance.BriefAllClients(num);
                    }
                    else if(cmd.CompareTo("start")==0)
                    {
                        Console.WriteLine("Uruchamiam algorytm u klientów");
                        instance.Execute();
                    }
                    else if(cmd.CompareTo("testbroadcast")==0)
                    {
                        instance.BroadcastMessage("testowa wiadomosc do wszystkich klientow");
                    }
                    else if(cmd.CompareTo("stats")==0)
                    {
                        instance.Stats();
                    }
                   

                } while (cmd.CompareTo("exit") != 0);

                Console.WriteLine("Wyłączam serwis...");
                selfHost.Close();
                Console.WriteLine("Naciśnij <ENTER> by zakoczyć");
                Console.ReadLine();
                

            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("Przechwyciłem wyjątek: {0}", ce.Message);
                selfHost.Abort();

            }
        }
    }
}
