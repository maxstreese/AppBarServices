using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.Ticks);
            Thread.Sleep(1000);
            Console.WriteLine(DateTime.Now.Ticks);
        }
    }

    class OuterClass
    {
        public int AnInt { get; set; } = 1;
        public string AString { get; set; } = "Ciao";
        private InnerClass TheInnerClass;

        public void TestThisShit()
        {
            TheInnerClass = new InnerClass(this);
            TheInnerClass.ChangeOuterClassState();
            Console.WriteLine("Int Value: {0}", AnInt);
            Console.WriteLine("String Value: {0}", AString);
        }

    }

    class InnerClass
    {
        private OuterClass TheOuterClass;

        public InnerClass(OuterClass theOuterClass)
        {
            TheOuterClass = theOuterClass;
        }

        public void ChangeOuterClassState()
        {
            TheOuterClass.AnInt = 2;
            TheOuterClass.AString = "Hi";
        }
    }
}
