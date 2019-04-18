using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClientApp
{
    public sealed class SecuredSingleton
    {
        private static SecuredSingleton instance = null;
        private static Mutex mut = new Mutex(true,"securedMut");

        public static SecuredSingleton GetInstance()
        {
            if (instance == null) instance = new SecuredSingleton();

            return instance;
        }
        public void Wait()
        {
            try
            {
                mut.WaitOne(10000);
            }
            catch(AbandonedMutexException amex)
            {
                Console.WriteLine("THROWN AMEX");
               
            }
            
        }
        public void Release()
        {
            mut.ReleaseMutex();
        }
    }
}
