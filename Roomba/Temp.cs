using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    public class Temp
    {
        private static int _x;
        private static int _y;
        private static int _X;
        private static int _Y;
        private static Stack<char> _mapArray;
        private static bool[][] _map;
        private static int _rest;
        private static bool _done;
        private static Stack<char> _path;
        private static int _a;
        private static int _b;
        private static Stack<int[]> _pointStack;

        public static string Do(int x, int y, string mapStr)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            _mapArray = new Stack<char>(mapStr);
            _map = new bool[_X][];
            _pointStack = new Stack<int[]>();
            for (int i = x; i > 0; i--)
            {
                _map[i] = new bool[_Y];
                for (int j = y; j > 0; j--)
                {
                    if (_mapArray.Pop() == '0')
                    {
                        _map[i][j] = true;
                        _pointStack.Push(new int[] { i, j });
                    }
                }
            }
            _rest = _pointStack.Count;
            #endregion
            #region 解题
            _done = false;
            _path = new Stack<char>();
            int[] point;
            Stack<int[]> road = new Stack<int[]>(), list = new Stack<int[]>();
            while (_pointStack.Any())
            {
                Console.WriteLine("{0}：{1} points rest", DateTime.Now.ToString("HH:mm:ss"), _pointStack.Count);
                point = _pointStack.Pop();
                _map[point[0]][point[1]] = false;
                road.Push(point);
                if ((_map[point[0] - 1][point[1]] && TestU(_map, _path, road, list)) || (_map[point[0] + 1, point[1]] && TestD(_map, _path, road, list)) || (_map[point[0], point[1] - 1] && TestL(_map, _path, road, list)) || (_map[point[0], point[1] + 1] && TestR(_map, _path, road, list)))
                {
                    _done = true;
                    _a = point[0];
                    _b = point[1];
                    _path = new Stack<char>(_path);
                    break;
                }
                else
                {
                    _map[point[0]][point[1]] = true;
                    road.Pop();
                }
            }
            #endregion
            return "";
        }

        private static bool HorizontalTest(int move, bool[][] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
        {
            int pathCount = path.Count, roadCount = road.Count;
            int[] currentPoint = road.Peek();
            int a = currentPoint[0], b = currentPoint[1];
            while (map[a][b + move])
            {
                b += move;
                map[a][b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a + 1][b])
            {
                if (map[a - 1][b])
                {

                }
                else
                {
                    return VerticalTest(1, map, path, road, list);
                }
            }
        }

        private static bool VerticalTest(int move, bool[][] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
        {
            int pathCount = path.Count, roadCount = road.Count;
            int[] currentPoint = road.Peek();
            int a = currentPoint[0], b = currentPoint[1];
            while (map[a][b + move])
            {
                b += move;
                map[a][b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a + 1][b])
            {
                if (map[a - 1][b])
                {

                }
                else
                {

                }
            }
        }

        private static void HorizontalGo(int move, bool[][] map, Stack<char> path, Stack<int[]> road)
        {

        }

        private static bool HorizontalConnect(int move)
        {

        }

        private static bool VerticalConnect(int move)
        {

        }
    }
}
