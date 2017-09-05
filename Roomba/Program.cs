using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Roomba roomba = new Roomba();
                Temp roomba = new Temp();
                roomba.Auto();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
