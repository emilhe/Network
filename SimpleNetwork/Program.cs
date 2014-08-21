using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataItems;
using ProtoBuf;
using ProtoBuf.Meta;
using SimpleImporter;

namespace SimpleNetwork
{
    internal class Program
    {

        /// <summary>
        /// Console test entry point. Not really prette that it's here...
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            EcnImporter.Parse();
        }

    }
}
