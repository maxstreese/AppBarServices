using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStuffConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MyClass.AnInt = 2;
            Console.WriteLine(MyClass.AnInt);
            MyClass.AnInt = 4;
            Console.WriteLine(MyClass.AnInt);
        }
    }
    public static class MyClass
    {
        public static int AnInt { get; set; }
    }
}
