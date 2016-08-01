using System;
using System.IO;
using Basic;

namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*.bas"))
            {
                Interpreter b = new Interpreter(File.ReadAllText(file));
                try
                {
                    b.Exec();
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
