using System;
using starsky;
using starsky.Controllers;
using starsky.Data;
using starsky.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace starskyCli
{
    class Program
    {

        static void Main(string[] args)
        {
            var q = new SyncDatabase().SyncFiles();
            Console.WriteLine("Done!");
        }

    }
}
