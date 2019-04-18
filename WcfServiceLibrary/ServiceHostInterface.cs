using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WcfServiceLibrary
{
    public sealed class ServiceHostInterface
    {
        private static ServiceHostInterface instance = null;
        private static readonly object blocker = false;
        private int[][] matrix;
        private bool isDataReady = false;
        private Duplex duplexInstance;
        private bool isDuplexInstanceReady = false;

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
        public int[][] MatrixData
        {
            get
            {
                lock(blocker)
                {
                    return matrix;
                }
            }
            set
            {
                lock(blocker)
                {
                    isDataReady = true;
                    matrix = value;
                }
            }
        }
        public bool IsDataUpdated
        {
            get
            {
                return isDataReady;
            }

        }
        public Duplex SingletonInstance
        {
            get
            {
                return duplexInstance;
            }
            set
            {
                isDuplexInstanceReady = true;
                duplexInstance = value;
            }
        }
        public bool IsDuplexInstanceReady
        {
            get
            {
                return isDuplexInstanceReady;
            }
        }



        



    }
}
