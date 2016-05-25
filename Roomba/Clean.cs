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
        private static bool _done;
        private static Stack<char> _path;
        private static int _a;
        private static int _b;
        private static Stack<int[]> _pointStack;
        private static int _threadCount;
        private static bool _desc;

        static Clean()
        {
            _path = new Stack<char>();
            _pointStack = new Stack<int[]>();
            _threadCount = int.Parse(System.Configuration.ConfigurationManager.AppSettings["threadCount"]);
            _desc = System.Configuration.ConfigurationManager.AppSettings["desc"] == "true";
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

        public static string DoMultithreading(int x, int y, string mapStr)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            _mapArray = new Stack<char>(mapStr);
            _map = new bool[_X, _Y];
            _pointStack.Clear();
            for (int i = x; i > 0; i--)
            {
                for (int j = y; j > 0; j--)
                {
                    if (_mapArray.Pop() == '0')
                    {
                        _map[i, j] = true;
                        _pointStack.Push(new int[] { i, j });
                    }
                }
            }
            _rest = _pointStack.Count;
            if (_desc)
            {
                _pointStack = new Stack<int[]>(_pointStack);
            }
            #endregion
            #region 解题
            List<Task> taskList = new List<Task>();
            _done = false;
            _path.Clear();
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

        public static string Do(int x, int y, string mapStr)
        {
            _x = x;
            _y = y;
            _X = x + 2;
            _Y = y + 2;
            #region 初始化地图
            _mapArray = new Stack<char>(mapStr);
            _map = new bool[_X, _Y];
            _pointStack.Clear();
            for (int i = x; i > 0; i--)
            {
                for (int j = y; j > 0; j--)
                {
                    if (_mapArray.Pop() == '0')
                    {
                        _map[i, j] = true;
                        _pointStack.Push(new int[] { i, j });
                    }
                }
            }
            _rest = _pointStack.Count;
            if (_desc)
            {
                _pointStack = new Stack<int[]>(_pointStack);
            }
            #endregion
            #region 解题
            _done = false;
            _path.Clear();
            int[] point;
            Stack<int[]> road = new Stack<int[]>(), list = new Stack<int[]>();
            while (_pointStack.Any())
            {
                Console.WriteLine("{0} points rest", _pointStack.Count);
                point = _pointStack.Pop();
                _map[point[0], point[1]] = false;
                road.Push(point);
                if ((_map[point[0] - 1, point[1]] && TestU(_map, _path, road, list)) || (_map[point[0] + 1, point[1]] && TestD(_map, _path, road, list)) || (_map[point[0], point[1] - 1] && TestL(_map, _path, road, list)) || (_map[point[0], point[1] + 1] && TestR(_map, _path, road, list)))
                {
                    _done = true;
                    _a = point[0];
                    _b = point[1];
                    _path = new Stack<char>(_path);
                    break;
                }
                else
                {
                    _map[point[0], point[1]] = true;
                    road.Pop();
                }
            }
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
            int[] point;
            bool[,] copyMap = CopyMap(); ;
            Stack<char> copyPath = new Stack<char>();
            Stack<int[]> road = new Stack<int[]>(), list = new Stack<int[]>();
            while (!_done)
            {
                lock (_pointStack)
                {
                    Console.WriteLine("{0} points rest", _pointStack.Count);
                    if (_pointStack.Any())
                    {
                        point = _pointStack.Pop();
                    }
                    else
                    {
                        break;
                    }
                }
                copyMap[point[0], point[1]] = false;
                road.Push(point);
                if ((copyMap[point[0] - 1, point[1]] && TestU(copyMap, copyPath, road, list)) || (copyMap[point[0] + 1, point[1]] && TestD(copyMap, copyPath, road, list)) || (copyMap[point[0], point[1] - 1] && TestL(copyMap, copyPath, road, list)) || (copyMap[point[0], point[1] + 1] && TestR(copyMap, copyPath, road, list)))
                {
                    _done = true;
                    _a = point[0];
                    _b = point[1];
                    lock (_path)
                    {
                        _path = new Stack<char>(copyPath);
                    }
                }
                else
                {
                    copyMap[point[0], point[1]] = true;
                    road.Pop();
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

        private static bool TestU(bool[,] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
        {
            int pathCount = path.Count, roadCount = road.Count;
            int[] point;
            if (GoU(map, path, road))
            {
                char direction = path.Peek();
                point = road.Peek();
                int count;
                switch (direction)
                {
                    case 'u':
                    case 'd':
                        {
                            ConnectL(map, point[0], point[1] - 1, list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestL(map, path, road, list) || TestR(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                    case 'l':
                    case 'r':
                        {
                            ConnectU(map, point[0] - 1, point[1], list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestU(map, path, road, list) || TestD(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                }
            }
            else
            {
                if (_rest - road.Count == 0)
                {
                    return true;
                }
                else
                {
                    while (path.Count > pathCount)
                    {
                        path.Pop();
                    }
                    while (road.Count > roadCount)
                    {
                        point = road.Pop();
                        map[point[0], point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool TestD(bool[,] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
        {
            int pathCount = path.Count, roadCount = road.Count;
            int[] point;
            if (GoD(map, path, road))
            {
                char direction = path.Peek();
                point = road.Peek();
                int count;
                switch (direction)
                {
                    case 'u':
                    case 'd':
                        {
                            ConnectL(map, point[0], point[1] - 1, list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestL(map, path, road, list) || TestR(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                    case 'l':
                    case 'r':
                        {
                            ConnectU(map, point[0] - 1, point[1], list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestU(map, path, road, list) || TestD(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                }
            }
            else
            {
                if (_rest - road.Count == 0)
                {
                    return true;
                }
                else
                {
                    while (path.Count > pathCount)
                    {
                        path.Pop();
                    }
                    while (road.Count > roadCount)
                    {
                        point = road.Pop();
                        map[point[0], point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool TestL(bool[,] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
        {
            int pathCount = path.Count, roadCount = road.Count;
            int[] point;
            if (GoL(map, path, road))
            {
                char direction = path.Peek();
                point = road.Peek();
                int count;
                switch (direction)
                {
                    case 'u':
                    case 'd':
                        {
                            ConnectL(map, point[0], point[1] - 1, list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestL(map, path, road, list) || TestR(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                    case 'l':
                    case 'r':
                        {
                            ConnectU(map, point[0] - 1, point[1], list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestU(map, path, road, list) || TestD(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                }
            }
            else
            {
                if (_rest - road.Count == 0)
                {
                    return true;
                }
                else
                {
                    while (path.Count > pathCount)
                    {
                        path.Pop();
                    }
                    while (road.Count > roadCount)
                    {
                        point = road.Pop();
                        map[point[0], point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool TestR(bool[,] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
        {
            int pathCount = path.Count, roadCount = road.Count;
            int[] point;
            if (GoR(map, path, road))
            {
                char direction = path.Peek();
                point = road.Peek();
                int count;
                switch (direction)
                {
                    case 'u':
                    case 'd':
                        {
                            ConnectL(map, point[0], point[1] - 1, list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestL(map, path, road, list) || TestR(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                    case 'l':
                    case 'r':
                        {
                            ConnectU(map, point[0] - 1, point[1], list);
                            count = list.Count;
                            while (list.Any())
                            {
                                point = list.Pop();
                                map[point[0], point[1]] = true;
                            }
                            if (count == (_rest - road.Count) && (TestU(map, path, road, list) || TestD(map, path, road, list)))
                            {
                                return true;
                            }
                            else
                            {
                                while (path.Count > pathCount)
                                {
                                    path.Pop();
                                }
                                while (road.Count > roadCount)
                                {
                                    point = road.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                return false;
                            }
                        }
                }
            }
            else
            {
                if (_rest - road.Count == 0)
                {
                    return true;
                }
                else
                {
                    while (path.Count > pathCount)
                    {
                        path.Pop();
                    }
                    while (road.Count > roadCount)
                    {
                        point = road.Pop();
                        map[point[0], point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool GoU(bool[,] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('u');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a - 1, b])
            {
                map[--a, b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a, b - 1])
            {
                if (map[a, b + 1])
                {
                    return true;
                }
                else
                {
                    return GoL(map, path, road);
                }
            }
            else
            {
                if (map[a, b + 1])
                {
                    return GoR(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool GoD(bool[,] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('d');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a + 1, b])
            {
                map[++a, b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a, b - 1])
            {
                if (map[a, b + 1])
                {
                    return true;
                }
                else
                {
                    return GoL(map, path, road);
                }
            }
            else
            {
                if (map[a, b + 1])
                {
                    return GoR(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool GoL(bool[,] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('l');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a, b - 1])
            {
                map[a, --b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a - 1, b])
            {
                if (map[a + 1, b])
                {
                    return true;
                }
                else
                {
                    return GoU(map, path, road);
                }
            }
            else
            {
                if (map[a + 1, b])
                {
                    return GoD(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool GoR(bool[,] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('r');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a, b + 1])
            {
                map[a, ++b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a - 1, b])
            {
                if (map[a + 1, b])
                {
                    return true;
                }
                else
                {
                    return GoU(map, path, road);
                }
            }
            else
            {
                if (map[a + 1, b])
                {
                    return GoD(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static void ConnectU(bool[,] map, int a, int b, Stack<int[]> list)
        {
            map[a, b] = false;
            list.Push(new int[] { a, b });
            if (map[a - 1, b])
            {
                ConnectU(map, a - 1, b, list);
            }
            if (map[a, b - 1])
            {
                ConnectL(map, a, b - 1, list);
            }
            if (map[a, b + 1])
            {
                ConnectR(map, a, b + 1, list);
            }
        }
        private static void ConnectD(bool[,] map, int a, int b, Stack<int[]> list)
        {
            map[a, b] = false;
            list.Push(new int[] { a, b });
            if (map[a + 1, b])
            {
                ConnectD(map, a + 1, b, list);
            }
            if (map[a, b - 1])
            {
                ConnectL(map, a, b - 1, list);
            }
            if (map[a, b + 1])
            {
                ConnectR(map, a, b + 1, list);
            }
        }
        private static void ConnectL(bool[,] map, int a, int b, Stack<int[]> list)
        {
            map[a, b] = false;
            list.Push(new int[] { a, b });
            if (map[a - 1, b])
            {
                ConnectU(map, a - 1, b, list);
            }
            if (map[a + 1, b])
            {
                ConnectD(map, a + 1, b, list);
            }
            if (map[a, b - 1])
            {
                ConnectL(map, a, b - 1, list);
            }
        }
        private static void ConnectR(bool[,] map, int a, int b, Stack<int[]> list)
        {
            map[a, b] = false;
            list.Push(new int[] { a, b });
            if (map[a - 1, b])
            {
                ConnectU(map, a - 1, b, list);
            }
            if (map[a + 1, b])
            {
                ConnectD(map, a + 1, b, list);
            }
            if (map[a, b + 1])
            {
                ConnectR(map, a, b + 1, list);
            }
        }
    }
}
