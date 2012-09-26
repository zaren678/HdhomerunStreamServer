using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace WpfApplication1
{
    public sealed class RandomPortGenerator
    {
        private static int MIN_PORT_NUMBER = 4096;
        private static int MAX_PORT_NUMBER = 65535;

        private static volatile RandomPortGenerator instance;
        private static object syncRoot = new Object();

        private ArrayList portList = new ArrayList();

        private Random randomizer = new Random();

        private RandomPortGenerator()
        {
        }

        public static RandomPortGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new RandomPortGenerator();
                        }
                    }
                }

                return instance;
            }
        }

        public int getNextPort()
        {
            //generate the port number
            int port = randomizer.Next(MIN_PORT_NUMBER, MAX_PORT_NUMBER);

            while (portList.Contains(port))
            {
                port = randomizer.Next(MIN_PORT_NUMBER, MAX_PORT_NUMBER);
            }

            portList.Add(port);

            return port;
        }

        public void deletePort(int port)
        {
            portList.Remove(port);
        }

        public void addControlPort(int port)
        {
            if(!portList.Contains(port))
            {
                portList.Add(port);
            }
        }
    }
}
