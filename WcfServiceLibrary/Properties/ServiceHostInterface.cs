using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HostApp
{
    public sealed class ServiceHostInterface
    {
        private static ServiceHostInterface instance = null;

        private ServiceHostInterface()
        {

        }
        public static ServiceHostInterface GetInstance()
        {
            if (instance == null)
            {
                instance = new ServiceHostInterface();
            }
            return instance;
        }
    }
}
