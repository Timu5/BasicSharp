using System;
using System.IO;

namespace BasicSharp.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*.bas"))
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
