using System;

namespace Roomba
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Roomba roomba = new Roomba();
                //Temp roomba = new Temp();
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
