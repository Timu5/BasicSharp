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
                basic.AddFunction("assert", Assert);
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

        public static Value Assert(Interpreter interpreter, List<Value> args)
        {
            if ( args.Count != 1)
                throw new ArgumentException();

            Console.WriteLine("DA duck? " + args[0].Real);

            if (args[0].Real != 0)
                return Value.Zero;

            throw new Exception("assert(" + interpreter.GetLine() + ")");
        }
    }
}
