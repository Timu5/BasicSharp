BasicSharp
====
Simple BASIC interpreter written in C#. Language syntax is modernize version of BASIC, see example below.

Example
-------
```BlitzBasic
print "Hello World"

let a = 10
print "Variable a: " + a

let b = 20
print "a+b=" + (a+b)

if a = 10 then
    print "True"
else
    print "False"
endif

for i = 1 to 10
    print i
next i

goto mylabel
print "False"

mylabel:
Print "True"

```

How to use
----------

```cs
using System;
using BasicSharp;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = "print \"Hello World\"";
            Interpreter basic = new Interpreter(code);
            basic.printHandler += Console.WriteLine;
            basic.inputHandler += Console.ReadLine; 
            try
            {
                basic.Exec();
            }
            catch (BasicException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Line);
            }
        }
    }
}
```
