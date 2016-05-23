using DotNet4.Utilities;
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

        static Temp()
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
                if (multi)
                {
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
                else
                {
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
                        hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", Do(x, y, mapStr));
                        Console.WriteLine("level:{0} end,{1}", level, DateTime.Now.ToShortTimeString());
                        hr = hh.GetHtml(hi);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static string DoMultithreading(int x, int y, string mapStr, bool loop)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            char[] mapArray = mapStr.ToArray();
            _map = new bool[_X, _Y];
            _rest = 0;
            for (int j = 0; j < _Y; j++)
            {
                _map[0, j] = false;
                _map[x + 1, j] = false;
            }
            for (int i = x; i > 0; i--)
            {
                _map[i, 0] = false;
                _map[i, y + 1] = false;
            }
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; )
                {
                    if (mapArray[i * y + j] == '0')
                    {
                        _map[i + 1, ++j] = true;
                        _rest++;
                    }
                    else
                    {
                        _map[i + 1, ++j] = false;
                    }
                }
            }
            #endregion
            #region 解题
            List<Task> taskList = new List<Task>();
            _done = false;
            _path.Clear();
            _restInit = _rest - 1;
            if (loop)
            {
                for (int i = x; i > 0; i--)
                {
                    for (int j = y; j > 0; j--)
                    {
                        if (_map[i, j])
                        {
                            taskList.Add(Task.Factory.StartNew(DoSyncLoop, new int[] { i, j }));
                        }
                    }
                }
            }
            else
            {
                //for (int i = x; i > 0; i--)
                //{
                //    for (int j = y; j > 0; j--)
                //    {
                //        if (_map[i, j])
                //        {
                //            taskList.Add(Task.Factory.StartNew(DoSyncRecursion, new int[] { i, j }));
                //        }
                //    }
                //}
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

        private static string DoMultithreading(int x, int y, string mapStr)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            char[] mapArray = mapStr.ToArray();
            _map = new bool[_X, _Y];
            _rest = 0;
            for (int j = 0; j < _Y; j++)
            {
                _map[0, j] = false;
                _map[x + 1, j] = false;
            }
            for (int i = x; i > 0; i--)
            {
                _map[i, 0] = false;
                _map[i, y + 1] = false;
            }
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; )
                {
                    if (mapArray[i * y + j] == '0')
                    {
                        _map[i + 1, ++j] = true;
                        _rest++;
                    }
                    else
                    {
                        _map[i + 1, ++j] = false;
                    }
                }
            }
            #endregion
            #region 解题
            List<Task> taskList = new List<Task>();
            _done = false;
            _path.Clear();
            _restInit = _rest - 1;
            Point point;
            List<Point> pointList = new List<Point>();
            for (int i = x; i > 0; i--)
            {
                for (int j = y; j > 0; j--)
                {
                    if (_map[i, j])
                    {
                        point = new Point(i, j);
                        if (_map[i - 1, j])
                        {
                            point.directionStack.Push('u');
                        }
                        if (_map[i + 1, j])
                        {
                            point.directionStack.Push('d');
                        }
                        if (_map[i, j - 1])
                        {
                            point.directionStack.Push('l');
                        }
                        if (_map[i, j + 1])
                        {
                            point.directionStack.Push('r');
                        }
                        pointList.Add(point);
                    }
                }
            }
            foreach (Point p in pointList.OrderByDescending(o=>o.directionStack.Count))
            {
                _pointStack.Push(p);
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

        private static string Do(int x, int y, string mapStr)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            char[] mapArray = mapStr.ToArray();
            _map = new bool[_X, _Y];
            _rest = 0;
            for (int j = 0; j < _Y; j++)
            {
                _map[0, j] = false;
                _map[x + 1, j] = false;
            }
            for (int i = x; i > 0; i--)
            {
                _map[i, 0] = false;
                _map[i, y + 1] = false;
            }
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; )
                {
                    if (mapArray[i * y + j] == '0')
                    {
                        _map[i + 1, ++j] = true;
                        _rest++;
                    }
                    else
                    {
                        _map[i + 1, ++j] = false;
                    }
                }
            }
            #endregion
            #region 解题
            _done = false;
            _path.Clear();
            _restInit = _rest - 1;
            List<Point> pointList = new List<Point>();
            Point point;
            for (int i = x; i > 0; i--)
            {
                for (int j = y; j > 0; j--)
                {
                    if (_map[i, j])
                    {
                        point = new Point(i, j);
                        if (_map[i - 1, j])
                        {
                            point.directionStack.Push('u');
                        }
                        if (_map[i + 1, j])
                        {
                            point.directionStack.Push('d');
                        }
                        if (_map[i, j - 1])
                        {
                            point.directionStack.Push('l');
                        }
                        if (_map[i, j + 1])
                        {
                            point.directionStack.Push('r');
                        }
                        pointList.Add(point);
                    }
                }
            }
            Stack<char> path = new Stack<char>();
            int nums = pointList.Count;
            foreach (Point p in pointList.OrderBy(o => o.directionStack.Count))
            {
                _map[p.x, p.y] = false;
                if (Test(_map, _restInit, path, p))
                {
                    _done = true;
                    _a = p.x;
                    _b = p.y;
                    while (path.Any())
                    {
                        _path.Push(path.Pop());
                    }
                    break;
                }
                else
                {
                    _map[p.x, p.y] = true;
                }
                Console.WriteLine("{0} points rest.", --nums);
            }
            #endregion
            string result = string.Format("x={0}&y={1}&path=", _a, _b);
            foreach (char item in _path)
            {
                result += item;
            }
            return result;
        }

        private static string Do(int x, int y, string mapStr, bool loop)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            char[] mapArray = mapStr.ToArray();
            _map = new bool[_X, _Y];
            _rest = 0;
            for (int j = 0; j < _Y; j++)
            {
                _map[0, j] = false;
                _map[x + 1, j] = false;
            }
            for (int i = x; i > 0; i--)
            {
                _map[i, 0] = false;
                _map[i, y + 1] = false;
            }
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; )
                {
                    if (mapArray[i * y + j] == '0')
                    {
                        _map[i + 1, ++j] = true;
                        _rest++;
                    }
                    else
                    {
                        _map[i + 1, ++j] = false;
                    }
                }
            }
            #endregion
            #region 解题
            _done = false;
            _path.Clear();
            _restInit = _rest - 1;
            if (loop)
            {
                Stack<int[]> pointStack = new Stack<int[]>();
                Stack<Stack<char>> pointDirectionsStack = new Stack<Stack<char>>();
                Stack<Stack<char>> pointPathStack = new Stack<Stack<char>>();
                Stack<Stack<int[]>> pointRoadStack = new Stack<Stack<int[]>>();
                for (int i = x; i > 0; i--)
                {
                    for (int j = y; j > 0; j--)
                    {
                        if (_map[i, j])
                        {
                            int[] point = new int[] { i, j };
                            _map[point[0], point[1]] = false;


                            pointStack.Push(point);
                            pointPathStack.Push(new Stack<char>());
                            pointRoadStack.Push(new Stack<int[]>());
                            Stack<char> directionStack = new Stack<char>();
                            if (_map[point[0] - 1, point[1]])
                            {
                                directionStack.Push('u');
                            }
                            if (_map[point[0] + 1, point[1]])
                            {
                                directionStack.Push('d');
                            }
                            if (_map[point[0], point[1] - 1])
                            {
                                directionStack.Push('l');
                            }
                            if (_map[point[0], point[1] + 1])
                            {
                                directionStack.Push('r');
                            }
                            pointDirectionsStack.Push(directionStack);

                            if (LoopTest(_map, _restInit, pointStack, pointDirectionsStack, pointPathStack, pointRoadStack))
                            {
                                _done = true;
                                _a = point[0];
                                _b = point[1];
                            }
                            else
                            {
                                _map[i, j] = true;
                            }
                        }
                    }
                }
            }
            else
            {
                //for (int i = x; i > 0; i--)
                //{
                //    for (int j = y; j > 0; j--)
                //    {
                //        if (_map[i, j])
                //        {
                //            taskList.Add(Task.Factory.StartNew(DoSyncRecursion, new int[] { i, j }));
                //        }
                //    }
                //}
            }
            #endregion
            string result = string.Format("x={0}&y={1}&path=", _a, _b);
            foreach (char item in _path)
            {
                result += item;
            }
            return result;
        }

        private static void DoSyncLoop(object state)
        {
            if (!_done)
            {
                bool[,] copyMap = CopyMap();
                int[] point = state as int[];
                copyMap[point[0], point[1]] = false;

                Stack<int[]> pointStack = new Stack<int[]>();
                Stack<Stack<char>> pointDirectionsStack = new Stack<Stack<char>>();
                Stack<Stack<char>> pointPathStack = new Stack<Stack<char>>();
                Stack<Stack<int[]>> pointRoadStack = new Stack<Stack<int[]>>();
                pointStack.Push(point);
                pointPathStack.Push(new Stack<char>());
                pointRoadStack.Push(new Stack<int[]>());
                Stack<char> directionStack = new Stack<char>();
                if (copyMap[point[0] - 1, point[1]])
                {
                    directionStack.Push('u');
                }
                if (copyMap[point[0] + 1, point[1]])
                {
                    directionStack.Push('d');
                }
                if (copyMap[point[0], point[1] - 1])
                {
                    directionStack.Push('l');
                }
                if (copyMap[point[0], point[1] + 1])
                {
                    directionStack.Push('r');
                }
                pointDirectionsStack.Push(directionStack);

                if (LoopTest(copyMap, _restInit, pointStack, pointDirectionsStack, pointPathStack, pointRoadStack))
                {
                    _done = true;
                    _a = point[0];
                    _b = point[1];
                }
            }
        }

        private static void DoSync()
        {
            Point point;
            while (!_done)
            {
                lock (_pointStack)
                {
                    Console.WriteLine("{0} points rest.", _pointStack.Count);
                    if (_pointStack.Any())
                    {
                        point = _pointStack.Pop();
                    }
                    else
                    {
                        break;
                    }
                }
                bool[,] copyMap = CopyMap();
                copyMap[point.x, point.y] = false;
                Stack<char> path = new Stack<char>();
                if (Test(copyMap, _restInit, path, point))
                {
                    _done = true;
                    _a = point.x;
                    _b = point.y;
                    lock (_path)
                    {
                        while (path.Any())
                        {
                            _path.Push(path.Pop());
                        }
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
            for (int i = 0; i < _X; i++)
            {
                for (int j = 0; j < _Y; j++)
                {
                    target[i, j] = source[i, j];
                }
            }
        }

        private static bool LoopTest(bool[,] map, int rest, Stack<int[]> pointStack, Stack<Stack<char>> pointDirectionsStack, Stack<Stack<char>> pointPathStack, Stack<Stack<int[]>> pointRoadStack)
        {
            bool test = true;
            int[] currentPoint, copyPoint = new int[2], tempPoint;
            Stack<char> currentDirections, currentPath, copyPath;
            char direction;
            Stack<int[]> currentRoad, road, list = new Stack<int[]>();
            int count;
            while (test && pointStack.Any())
            {
                currentPoint = pointStack.Peek();
                currentDirections = pointDirectionsStack.Peek();
                currentPath = pointPathStack.Peek();
                currentRoad = pointRoadStack.Peek();
                if (currentDirections.Any())
                {
                    direction = currentDirections.Pop();
                    copyPoint[0] = currentPoint[0];
                    copyPoint[1] = currentPoint[1];
                    copyPath = new Stack<char>(currentPath.Reverse());
                    road = new Stack<int[]>();
                    LoopGo(map, ref rest, copyPoint, ref direction, copyPath, road);
                    switch (direction)
                    {
                        case 'u':
                        case 'd':
                            {
                                if (map[copyPoint[0], copyPoint[1] - 1])
                                {
                                    Connect(map, copyPoint[0], copyPoint[1] - 1, list);
                                    count = list.Count;
                                    while (list.Any())
                                    {
                                        tempPoint = list.Pop();
                                        map[tempPoint[0], tempPoint[1]] = true;
                                    }
                                    if (count == rest)
                                    {
                                        pointStack.Push(new int[] { copyPoint[0], copyPoint[1] });
                                        pointDirectionsStack.Push(new Stack<char>(_lr));
                                        pointPathStack.Push(copyPath);
                                        pointRoadStack.Push(road);
                                    }
                                    else
                                    {
                                        while (road.Any())
                                        {
                                            tempPoint = road.Pop();
                                            map[tempPoint[0], tempPoint[1]] = true;
                                            rest++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (rest == 0)
                                    {
                                        test = false;
                                        lock (_path)
                                        {
                                            _path.Clear();
                                            while (copyPath.Any())
                                            {
                                                _path.Push(copyPath.Pop());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        while (road.Any())
                                        {
                                            tempPoint = road.Pop();
                                            map[tempPoint[0], tempPoint[1]] = true;
                                            rest++;
                                        }
                                    }
                                }
                                break;
                            }
                        case 'l':
                        case 'r':
                            {
                                if (map[copyPoint[0] - 1, copyPoint[1]])
                                {
                                    Connect(map, copyPoint[0] - 1, copyPoint[1], list);
                                    count = list.Count;
                                    while (list.Any())
                                    {
                                        tempPoint = list.Pop();
                                        map[tempPoint[0], tempPoint[1]] = true;
                                    }
                                    if (count == rest)
                                    {
                                        pointStack.Push(new int[] { copyPoint[0], copyPoint[1] });
                                        pointDirectionsStack.Push(new Stack<char>(_ud));
                                        pointPathStack.Push(copyPath);
                                        pointRoadStack.Push(road);
                                    }
                                    else
                                    {
                                        while (road.Any())
                                        {
                                            tempPoint = road.Pop();
                                            map[tempPoint[0], tempPoint[1]] = true;
                                            rest++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (rest == 0)
                                    {
                                        test = false;
                                        lock (_path)
                                        {
                                            _path.Clear();
                                            while (copyPath.Any())
                                            {
                                                _path.Push(copyPath.Pop());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        while (road.Any())
                                        {
                                            tempPoint = road.Pop();
                                            map[tempPoint[0], tempPoint[1]] = true;
                                            rest++;
                                        }
                                    }
                                }
                                break;
                            }
                    }
                }
                else
                {
                    while (currentRoad.Any())
                    {
                        tempPoint = currentRoad.Pop();
                        map[tempPoint[0], tempPoint[1]] = true;
                        rest++;
                    }
                    pointStack.Pop();
                    pointDirectionsStack.Pop();
                    pointPathStack.Pop();
                    pointRoadStack.Pop();
                }
            }
            return !test;
        }

        private static bool Test(bool[,] map, int rest, Stack<char> path, Point point)
        {
            bool result = false;
            Stack<int[]> road = new Stack<int[]>(), list = new Stack<int[]>();
            Stack<char> tempPath = new Stack<char>();
            char direction;
            int[] currentPoint, tempPoint;
            int count;
            while (point.directionStack.Any())
            {
                direction = point.directionStack.Pop();
                currentPoint = new int[] { point.x, point.y };
                tempPath.Clear();
                LoopGo(map, ref rest, currentPoint, ref direction, tempPath, road);
                switch (direction)
                {
                    case 'u':
                    case 'd':
                        {
                            if (map[currentPoint[0], currentPoint[1] - 1])
                            {
                                Connect(map, currentPoint[0], currentPoint[1] - 1, list);
                                count = list.Count;
                                while (list.Any())
                                {
                                    tempPoint = list.Pop();
                                    map[tempPoint[0], tempPoint[1]] = true;
                                }
                                if (count == rest && Test(map, rest, tempPath, new Point(currentPoint[0], currentPoint[1], _lr)))
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
                                Connect(map, currentPoint[0] - 1, currentPoint[1], list);
                                count = list.Count;
                                while (list.Any())
                                {
                                    tempPoint = list.Pop();
                                    map[tempPoint[0], tempPoint[1]] = true;
                                }
                                if (count == rest && Test(map, rest, tempPath, new Point(currentPoint[0], currentPoint[1], _ud)))
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
                    foreach (char item in tempPath.Reverse())
                    {
                        path.Push(item);
                    }
                    break;
                }
                else
                {
                    while (road.Any())
                    {
                        tempPoint = road.Pop();
                        map[tempPoint[0], tempPoint[1]] = true;
                        rest++;
                    }
                }
            }
            return result;
        }

        private static void LoopGo(bool[,] map, ref int rest, int[] point, ref char direction, Stack<char> path, Stack<int[]> road)
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

        private static void Connect(bool[,] map, int a, int b, Stack<int[]> list)
        {
            list.Push(new int[] { a, b });
            map[a, b] = false;
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
    }

    class Point
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
