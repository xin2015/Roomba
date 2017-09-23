using System;

namespace Roomba
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Roomba roomba = new Roomba();
                Test roomba = new Test();
                roomba.Auto();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
