using System;
using System.IO;
using System.Collections.Generic;

namespace BasicSharp.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Tests"), "*.bas"))
            {
                Interpreter basic = new Interpreter(File.ReadAllText(file));
                try
                {
                    basic.Exec();
                }
                catch (Exception e)
                {
                    Console.WriteLine("BAD");
                    Console.WriteLine(e.Message);
                    continue;
                }
                Console.WriteLine("OK");
            }
            Console.Read();
        }
    }
}
