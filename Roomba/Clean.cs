using DotNet4.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    public class Clean
    {
        private static int _x;
        private static int _y;
        private static int _X;
        private static int _Y;
        private static Stack<char> _mapArray;
        private static bool[,] _map;
        private static int _rest;
        private static int _restInit;
        private static bool _done;
        private static Stack<char> _path;
        private static int _a;
        private static int _b;
        private static char[] _lr = new char[] { 'l', 'r' };
        private static char[] _ud = new char[] { 'u', 'd' };
        private static Stack<Point> _pointStack;
        private static int _threadCount;

        static Clean()
        {
            _path = new Stack<char>();
            _pointStack = new Stack<Point>();
            _threadCount = int.Parse(System.Configuration.ConfigurationManager.AppSettings["threadCount"]);
        }

        public static void Auto()
        {
            try
            {
                HttpHelper hh = new HttpHelper();
                HttpItem hi = new HttpItem()
                {
                    Cookie = "laravel_session=eyJpdiI6Ijc1K3I3cFFQWWNjT2tPZTBLTytKeXc9PSIsInZhbHVlIjoibFJBMnJhd1d3b204c3dFWHNUZUFcLzIyeGhlcHNLVCtBbXlwNFIxaTRXQ09pR2srK2hEMmVvRGJlQjFyUUZxOE0ybXl2VWdxdE1vWUN4MTc5eGRBdHFBPT0iLCJtYWMiOiJmMzE5N2Y2MzFmYzg5YTE4MmFhNTcxODM4M2NjZmMyNzc3MGE0MDM4YmU3MGE3MWQwZjIxNmZhNWJlMjdkZGZlIn0%3D"
                };
                HttpResult hr;
                int level = 0, x, y;
                string mapStr;
                int maxLevel = int.Parse(System.Configuration.ConfigurationManager.AppSettings["maxLevel"]);
                bool multi = System.Configuration.ConfigurationManager.AppSettings["multi"] == "true";
                while (level < maxLevel)
                {
                    hi.URL = "http://www.qlcoder.com/train/autocr";
                    hr = hh.GetHtml(hi);
                    string html = hr.Html;
                    html = html.Substring(html.IndexOf("level="));
                    html = html.Substring(0, html.IndexOf("<br>"));
                    string[] paramsArray = html.Split('&');
                    level = int.Parse(paramsArray[0].Replace("level=", string.Empty));
                    x = int.Parse(paramsArray[1].Replace("x=", string.Empty));
                    y = int.Parse(paramsArray[2].Replace("y=", string.Empty));
                    mapStr = paramsArray[3].Replace("map=", string.Empty);
                    Console.WriteLine("level:{0} start,{1}", level, DateTime.Now.ToShortTimeString());
                    hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", DoMultithreading(x, y, mapStr));
                    Console.WriteLine("level:{0} end,{1}", level, DateTime.Now.ToShortTimeString());
                    hr = hh.GetHtml(hi);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static string DoMultithreading(int x, int y, string mapStr)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            _mapArray = new Stack<char>(mapStr);
            _map = new bool[_X, _Y];
            _rest = 0;
            List<Point> pointList = new List<Point>();
            for (int i = x; i > 0; i--)
            {
                for (int j = y; j > 0; j--)
                {
                    if (_mapArray.Pop() == '0')
                    {
                        _map[i, j] = true;
                        _rest++;
                        pointList.Add(new Point(i, j));
                    }
                }
            }
            #endregion
            #region 解题
            List<Task> taskList = new List<Task>();
            _done = false;
            _path.Clear();
            _restInit = _rest - 1;
            _pointStack.Clear();
            Point point;
            Random rand = new Random();
            int index;
            while (pointList.Any())
            {
                index = rand.Next(pointList.Count);
                point = pointList[index];
                if (_map[point.x - 1, point.y])
                {
                    point.directionStack.Push('u');
                }
                if (_map[point.x + 1, point.y])
                {
                    point.directionStack.Push('d');
                }
                if (_map[point.x, point.y - 1])
                {
                    point.directionStack.Push('l');
                }
                if (_map[point.x, point.y + 1])
                {
                    point.directionStack.Push('r');
                }
                _pointStack.Push(point);
                pointList.RemoveAt(index);
            }
            for (int i = 0; i < _threadCount; i++)
            {
                taskList.Add(Task.Factory.StartNew(DoSync));
            }
            Task.WaitAll(taskList.ToArray());
            #endregion
            string result = string.Format("x={0}&y={1}&path=", _a, _b);
            foreach (char item in _path)
            {
                result += item;
            }
            return result;
        }

        private static void DoSync()
        {
            Point point;
            bool[,] copyMap;
            Stack<char> copyPath = new Stack<char>();
            Stack<int[]> road = new Stack<int[]>(), forCount = new Stack<int[]>();
            Queue<int[]> forLoop = new Queue<int[]>();
            while (!_done)
            {
                lock (_pointStack)
                {
                    if (_pointStack.Any())
                    {
                        point = _pointStack.Pop();
                    }
                    else
                    {
                        break;
                    }
                }
                copyMap = CopyMap();
                copyMap[point.x, point.y] = false;
                if (Test(copyMap, _restInit, copyPath, point, road, forCount, forLoop))
                {
                    _done = true;
                    _a = point.x;
                    _b = point.y;
                    lock (_path)
                    {
                        _path = new Stack<char>(copyPath);
                    }
                }
            }
        }

        private static bool[,] CopyMap()
        {
            bool[,] copyMap = new bool[_X, _Y];
            CopyMap(_map, copyMap);
            return copyMap;
        }

        private static void CopyMap(bool[,] source, bool[,] target)
        {
            for (int i = _x; i > 0; i--)
            {
                for (int j = _y; j > 0; j--)
                {
                    target[i, j] = source[i, j];
                }
            }
        }

        private static bool Test(bool[,] map, int rest, Stack<char> path, Point point, Stack<int[]> road, Stack<int[]> forCount, Queue<int[]> forLoop)
        {
            bool result = false;
            char direction;
            int[] currentPoint, tempPoint;
            int pathCount = path.Count, roadCount = road.Count;
            while (point.directionStack.Any())
            {
                direction = point.directionStack.Pop();
                currentPoint = new int[] { point.x, point.y };
                Go(map, ref rest, currentPoint, ref direction, path, road);
                switch (direction)
                {
                    case 'u':
                    case 'd':
                        {
                            if (map[currentPoint[0], currentPoint[1] - 1])
                            {
                                if (Connect(map, currentPoint[0], currentPoint[1] - 1, forCount, forLoop) == rest && Test(map, rest, path, new Point(currentPoint[0], currentPoint[1], _lr), road, forCount, forLoop))
                                {
                                    result = true;
                                }
                            }
                            else
                            {
                                if (rest == 0)
                                {
                                    result = true;
                                }
                            }
                            break;
                        }
                    case 'l':
                    case 'r':
                        {
                            if (map[currentPoint[0] - 1, currentPoint[1]])
                            {
                                if (Connect(map, currentPoint[0] - 1, currentPoint[1], forCount, forLoop) == rest && Test(map, rest, path, new Point(currentPoint[0], currentPoint[1], _ud), road, forCount, forLoop))
                                {
                                    result = true;
                                }
                            }
                            else
                            {
                                if (rest == 0)
                                {
                                    result = true;
                                }
                            }
                            break;
                        }
                }
                if (result)
                {
                    break;
                }
                else
                {
                    for (int i = road.Count; i > roadCount; i--)
                    {
                        tempPoint = road.Pop();
                        map[tempPoint[0], tempPoint[1]] = true;
                        rest++;
                    }
                    for (int i = path.Count; i > pathCount; i--)
                    {
                        path.Pop();
                    }
                }
            }
            return result;
        }

        private static void Go(bool[,] map, ref int rest, int[] point, ref char direction, Stack<char> path, Stack<int[]> road)
        {
            bool go = true;
            while (go)
            {
                path.Push(direction);
                switch (direction)
                {
                    case 'u':
                        {
                            while (map[point[0] - 1, point[1]])
                            {
                                map[--point[0], point[1]] = false;
                                road.Push(new int[] { point[0], point[1] });
                                rest--;
                            }
                            if (map[point[0], point[1] + 1])
                            {
                                if (!map[point[0], point[1] - 1])
                                {
                                    direction = 'r';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            else
                            {
                                if (map[point[0], point[1] - 1])
                                {
                                    direction = 'l';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            break;
                        }
                    case 'r':
                        {
                            while (map[point[0], point[1] + 1])
                            {
                                map[point[0], ++point[1]] = false;
                                road.Push(new int[] { point[0], point[1] });
                                rest--;
                            }
                            if (map[point[0] - 1, point[1]])
                            {
                                if (!map[point[0] + 1, point[1]])
                                {
                                    direction = 'u';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            else
                            {
                                if (map[point[0] + 1, point[1]])
                                {
                                    direction = 'd';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            break;
                        }
                    case 'd':
                        {
                            while (map[point[0] + 1, point[1]])
                            {
                                map[++point[0], point[1]] = false;
                                road.Push(new int[] { point[0], point[1] });
                                rest--;
                            }
                            if (map[point[0], point[1] + 1])
                            {
                                if (!map[point[0], point[1] - 1])
                                {
                                    direction = 'r';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            else
                            {
                                if (map[point[0], point[1] - 1])
                                {
                                    direction = 'l';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            break;
                        }
                    case 'l':
                        {
                            while (map[point[0], point[1] - 1])
                            {
                                map[point[0], --point[1]] = false;
                                road.Push(new int[] { point[0], point[1] });
                                rest--;
                            }
                            if (map[point[0] - 1, point[1]])
                            {
                                if (!map[point[0] + 1, point[1]])
                                {
                                    direction = 'u';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            else
                            {
                                if (map[point[0] + 1, point[1]])
                                {
                                    direction = 'd';
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            break;
                        }
                }
            }
        }

        private static int Connect(bool[,] map, int a, int b, Stack<int[]> forCount, Queue<int[]> forLoop)
        {
            int result;
            forLoop.Enqueue(new int[] { a, b });
            map[a, b] = false;
            int[] point;
            while (forLoop.Any())
            {
                point = forLoop.Dequeue();
                if (map[point[0] - 1, point[1]])
                {
                    forLoop.Enqueue(new int[] { point[0] - 1, point[1] });
                    map[point[0] - 1, point[1]] = false;
                }
                if (map[point[0] + 1, point[1]])
                {
                    forLoop.Enqueue(new int[] { point[0] + 1, point[1] });
                    map[point[0] + 1, point[1]] = false;
                }
                if (map[point[0], point[1] - 1])
                {
                    forLoop.Enqueue(new int[] { point[0], point[1] - 1 });
                    map[point[0], point[1] - 1] = false;
                }
                if (map[point[0], point[1] + 1])
                {
                    forLoop.Enqueue(new int[] { point[0], point[1] + 1 });
                    map[point[0], point[1] + 1] = false;
                }
                forCount.Push(point);
            }
            result = forCount.Count;
            while (forCount.Any())
            {
                point = forCount.Pop();
                map[point[0], point[1]] = true;
            }
            return result;
        }

        private static void Connect(bool[,] map, int a, int b, Stack<int[]> list)
        {
            map[a, b] = false;
            list.Push(new int[] { a, b });
            if (map[a - 1, b])
            {
                Connect(map, a - 1, b, list);
            }
            if (map[a + 1, b])
            {
                Connect(map, a + 1, b, list);
            }
            if (map[a, b - 1])
            {
                Connect(map, a, b - 1, list);
            }
            if (map[a, b + 1])
            {
                Connect(map, a, b + 1, list);
            }
        }

        public static void TestConnect()
        {
            HttpHelper hh = new HttpHelper();
            HttpItem hi = new HttpItem()
            {
                Cookie = "laravel_session=eyJpdiI6Ijc1K3I3cFFQWWNjT2tPZTBLTytKeXc9PSIsInZhbHVlIjoibFJBMnJhd1d3b204c3dFWHNUZUFcLzIyeGhlcHNLVCtBbXlwNFIxaTRXQ09pR2srK2hEMmVvRGJlQjFyUUZxOE0ybXl2VWdxdE1vWUN4MTc5eGRBdHFBPT0iLCJtYWMiOiJmMzE5N2Y2MzFmYzg5YTE4MmFhNTcxODM4M2NjZmMyNzc3MGE0MDM4YmU3MGE3MWQwZjIxNmZhNWJlMjdkZGZlIn0%3D"
            };
            HttpResult hr;
            int level = 0, x, y;
            string mapStr;
            hi.URL = "http://www.qlcoder.com/train/autocr";
            hr = hh.GetHtml(hi);
            string html = hr.Html;
            html = html.Substring(html.IndexOf("level="));
            html = html.Substring(0, html.IndexOf("<br>"));
            string[] paramsArray = html.Split('&');
            level = int.Parse(paramsArray[0].Replace("level=", string.Empty));
            x = int.Parse(paramsArray[1].Replace("x=", string.Empty));
            y = int.Parse(paramsArray[2].Replace("y=", string.Empty));
            mapStr = paramsArray[3].Replace("map=", string.Empty);
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            _mapArray = new Stack<char>(mapStr);
            _map = new bool[_X, _Y];
            _rest = 0;
            List<Point> pointList = new List<Point>();
            for (int i = x; i > 0; i--)
            {
                for (int j = y; j > 0; j--)
                {
                    if (_mapArray.Pop() == '0')
                    {
                        _map[i, j] = true;
                        _rest++;
                        pointList.Add(new Point(i, j));
                    }
                }
            }

            Random rand = new Random();
            Point point = pointList[rand.Next(pointList.Count)];
            Stack<int[]> forCount = new Stack<int[]>(), list = new Stack<int[]>();
            Queue<int[]> forLoop = new Queue<int[]>();
            int count;
            int[] p=new int[2];
            Stopwatch s = new Stopwatch();
            s.Start();
            for (int i = 0; i < 1000000; i++)
            {
                Connect(_map, point.x, point.y, list);
                count = list.Count;
                while (list.Any())
                {
                    p = list.Pop();
                    _map[p[0], p[1]] = true;
                }
            }
            s.Stop();
            Console.WriteLine(s.Elapsed);
            s.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                count = Connect(_map, point.x, point.y, forCount, forLoop);
            }
            s.Stop();
            Console.WriteLine(s.Elapsed);
            #endregion
        }
    }
}
