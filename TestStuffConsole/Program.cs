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
            OuterClass outerClass = new OuterClass();
            outerClass.FireOuterEvent();
            System.Console.ReadLine();

        }
    }
    public class OuterClass
    {
        public InnerClass _innerClass;

        public delegate void OuterEvent();
        public event OuterEvent outerEvent;

        public OuterClass()
        {
            _innerClass = new InnerClass(this);
        }

        public void FireOuterEvent()
        {
            outerEvent.Invoke();
        }
    }
    public class InnerClass
    {
        private OuterClass _caller;

        public InnerClass(OuterClass caller)
        {
            _caller = caller;
            _caller.outerEvent += _caller_outerEvent;
        }

        private void _caller_outerEvent()
        {
            Console.WriteLine("It worked!");
        }
    }
}
