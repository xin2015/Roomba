using DotNet4.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void Auto()
        {
            try
            {
                HttpHelper hh = new HttpHelper();
                HttpItem hi = new HttpItem()
                {
                    Cookie = "laravel_session=eyJpdiI6IlwvRG5neDVtN1lOSGh2WU82dldoNXZ3PT0iLCJ2YWx1ZSI6ImFLV2tSKzJ4Vlc2RWlCWHltOWU1bHpOQUdZalwvWDNTcSt3UGhlT0lQVWdMMGNLcFROekE0T256UHBZekZRS3QwVFd1MEVic2swaTJ0ZEgwMFpGZUxKQT09IiwibWFjIjoiOGQ3MDkwNmVhYmQ0ZTU0NDQ3ODAxNDQ0YzVkYzg4ZTBiMWE5NGIxNzliYjkyMGUzMmE3NGE3YjQ0NWRiNjJjYyJ9"
                };
                HttpResult hr;
                int level = 90, x, y;
                string mapStr;
                int maxLevel = int.Parse(System.Configuration.ConfigurationManager.AppSettings["maxLevel"]);
                while (level < maxLevel)
                {
                    hi.URL = "http://www.qlcoder.com/train/autocr?level=" + level.ToString();
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static string Do(int x, int y, string mapStr)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
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
            _map[0] = new bool[_Y];
            _map[_X - 1] = new bool[_Y];
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
                if ((_map[point[0] - 1][point[1]] && TestU(_map, _path, road, list)) || (_map[point[0] + 1][point[1]] && TestD(_map, _path, road, list)) || (_map[point[0]][point[1] - 1] && TestL(_map, _path, road, list)) || (_map[point[0]][point[1] + 1] && TestR(_map, _path, road, list)))
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
            string result = string.Format("x={0}&y={1}&path=", _a, _b);
            foreach (char item in _path)
            {
                result += item;
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            return result;
        }

        private static bool TestU(bool[][] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                        map[point[0]][point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool TestD(bool[][] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                        map[point[0]][point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool TestL(bool[][] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                        map[point[0]][point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool TestR(bool[][] map, Stack<char> path, Stack<int[]> road, Stack<int[]> list)
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                                map[point[0]][point[1]] = true;
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
                                    map[point[0]][point[1]] = true;
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
                        map[point[0]][point[1]] = true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool GoU(bool[][] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('u');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a - 1][b])
            {
                map[--a][b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a][b - 1])
            {
                if (map[a][b + 1])
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
                if (map[a][b + 1])
                {
                    return GoR(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool GoD(bool[][] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('d');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a + 1][b])
            {
                map[++a][b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a][b - 1])
            {
                if (map[a][b + 1])
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
                if (map[a][b + 1])
                {
                    return GoR(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool GoL(bool[][] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('l');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a][b - 1])
            {
                map[a][--b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a - 1][b])
            {
                if (map[a + 1][b])
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
                if (map[a + 1][b])
                {
                    return GoD(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool GoR(bool[][] map, Stack<char> path, Stack<int[]> road)
        {
            path.Push('r');
            int a = road.Peek()[0], b = road.Peek()[1];
            while (map[a][b + 1])
            {
                map[a][++b] = false;
                road.Push(new int[] { a, b });
            }
            if (map[a - 1][b])
            {
                if (map[a + 1][b])
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
                if (map[a + 1][b])
                {
                    return GoD(map, path, road);
                }
                else
                {
                    return false;
                }
            }
        }

        private static void ConnectU(bool[][] map, int a, int b, Stack<int[]> list)
        {
            map[a][b] = false;
            list.Push(new int[] { a, b });
            if (map[a - 1][b])
            {
                ConnectU(map, a - 1, b, list);
            }
            if (map[a][b - 1])
            {
                ConnectL(map, a, b - 1, list);
            }
            if (map[a][b + 1])
            {
                ConnectR(map, a, b + 1, list);
            }
        }

        private static void ConnectD(bool[][] map, int a, int b, Stack<int[]> list)
        {
            map[a][b] = false;
            list.Push(new int[] { a, b });
            if (map[a + 1][b])
            {
                ConnectD(map, a + 1, b, list);
            }
            if (map[a][b - 1])
            {
                ConnectL(map, a, b - 1, list);
            }
            if (map[a][b + 1])
            {
                ConnectR(map, a, b + 1, list);
            }
        }

        private static void ConnectL(bool[][] map, int a, int b, Stack<int[]> list)
        {
            map[a][b] = false;
            list.Push(new int[] { a, b });
            if (map[a - 1][b])
            {
                ConnectU(map, a - 1, b, list);
            }
            if (map[a + 1][b])
            {
                ConnectD(map, a + 1, b, list);
            }
            if (map[a][b - 1])
            {
                ConnectL(map, a, b - 1, list);
            }
        }

        private static void ConnectR(bool[][] map, int a, int b, Stack<int[]> list)
        {
            map[a][b] = false;
            list.Push(new int[] { a, b });
            if (map[a - 1][b])
            {
                ConnectU(map, a - 1, b, list);
            }
            if (map[a + 1][b])
            {
                ConnectD(map, a + 1, b, list);
            }
            if (map[a][b + 1])
            {
                ConnectR(map, a, b + 1, list);
            }
        }
    }
}
