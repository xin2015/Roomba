using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    public class Point
    {
        public int x { get; set; }
        public int y { get; set; }
        public Stack<char> directionStack { get; set; }

        public Point(int px, int py)
        {
            x = px;
            y = py;
            directionStack = new Stack<char>();
        }

        public Point(int px, int py, char[] directions)
        {
            x = px;
            y = py;
            directionStack = new Stack<char>(directions);
        }
    }
}
