using DotNet4.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    class Temp3
    {
        private static int _x;
        private static int _y;
        private static int _X;
        private static int _Y;
        private static Stack<char> _mapArray;
        private static bool[][] _map;
        private static int _rest;
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
            _rest = _pointStack.Count - 1;
            #endregion
            #region 解题
            _path = new Stack<char>();
            int[] point;
            Stack<int[]> road = new Stack<int[]>();
            while (_pointStack.Any())
            {
                Console.WriteLine("{0}：{1} points rest", DateTime.Now.ToString("HH:mm:ss"), _pointStack.Count);
                point = _pointStack.Pop();
                _a = point[0];
                _b = point[1];
                _map[_a][_b] = false;
                if (VerticalTest(1, _map, _a, _b, _path, road) || HorizontalTest(1, _map, _a, _b, _path, road) || VerticalTest(-1, _map, _a, _b, _path, road) || HorizontalTest(-1, _map, _a, _b, _path, road))
                {
                    break;
                }
                else
                {
                    _map[_a][_b] = true;
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

        private static bool HorizontalTest(int move, bool[][] map, int a, int b, Stack<char> path, Stack<int[]> road)
        {
            bool result;
            if (map[a][b + move])
            {
                int roadCount = road.Count;
                b += move;
                do
                {
                    map[a][b] = false;
                    road.Push(new int[] { a, b });
                    b += move;
                } while (map[a][b]);
                if (road.Count == _rest)
                {
                    result = true;
                }
                else
                {
                    b -= move;
                    bool prune = HorizontalPrune(map, a, b, road);
                    if (prune)
                    {
                        result = false;
                    }
                    else
                    {
                        result = VerticalTest(1, map, a, b, path, road) || VerticalTest(-1, map, a, b, path, road);
                    }
                }
                if (result)
                {
                    path.Push(move == 1 ? 'r' : 'l');
                }
                else
                {
                    while (road.Count > roadCount)
                    {
                        int[] point = road.Pop();
                        map[point[0]][point[1]] = true;
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        private static bool VerticalTest(int move, bool[][] map, int a, int b, Stack<char> path, Stack<int[]> road)
        {
            bool result;
            if (map[a + move][b])
            {
                int roadCount = road.Count;
                a += move;
                do
                {
                    map[a][b] = false;
                    road.Push(new int[] { a, b });
                    a += move;
                } while (map[a][b]);
                if (road.Count == _rest)
                {
                    result = true;
                }
                else
                {
                    a -= move;
                    bool prune = VerticalPrune(map, a, b, road);
                    if (prune)
                    {
                        result = false;
                    }
                    else
                    {
                        result = HorizontalTest(1, map, a, b, path, road) || HorizontalTest(-1, map, a, b, path, road);
                    }
                }
                if (result)
                {
                    path.Push(move == 1 ? 'd' : 'u');
                }
                else
                {
                    while (road.Count > roadCount)
                    {
                        int[] point = road.Pop();
                        map[point[0]][point[1]] = true;
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        private static bool HorizontalPrune(bool[][] map, int a, int b, Stack<int[]> road)
        {
            if (map[a - 1][b] && map[a + 1][b])
            {
                Stack<int[]> horizontalConnect = new Stack<int[]>();
                Stack<int[]> verticalConnect = new Stack<int[]>();
                Stack<int[]> connect = new Stack<int[]>();
                a++;
                while (map[a][b])
                {
                    map[a][b] = false;
                    int[] point = new int[] { a, b };
                    connect.Push(point);
                    horizontalConnect.Push(point);
                    a++;
                }
                while (horizontalConnect.Count > 0)
                {
                    while (horizontalConnect.Count > 0)
                    {
                        int[] point = horizontalConnect.Pop();
                        a = point[0];
                        b = point[1] + 1;
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            verticalConnect.Push(pointi);
                            b++;
                        }
                        b = point[1] - 1;
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            verticalConnect.Push(pointi);
                            b--;
                        }
                    }
                    while (verticalConnect.Count > 0)
                    {
                        int[] point = verticalConnect.Pop();
                        a = point[0] + 1;
                        b = point[1];
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            horizontalConnect.Push(pointi);
                            a++;
                        }
                        a = point[0] - 1;
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            horizontalConnect.Push(pointi);
                            a--;
                        }
                    }
                }
                int count = connect.Count;
                while (connect.Count > 0)
                {
                    int[] point = connect.Pop();
                    map[point[0]][point[1]] = true;
                }
                return count + road.Count < _rest;
            }
            else
            {
                return false;
            }
        }

        private static bool VerticalPrune(bool[][] map, int a, int b, Stack<int[]> road)
        {
            if (map[a][b - 1] && map[a][b + 1])
            {
                Stack<int[]> horizontalConnect = new Stack<int[]>();
                Stack<int[]> verticalConnect = new Stack<int[]>();
                Stack<int[]> connect = new Stack<int[]>();
                b++;
                while (map[a][b])
                {
                    map[a][b] = false;
                    int[] point = new int[] { a, b };
                    connect.Push(point);
                    verticalConnect.Push(point);
                    b++;
                }
                while (verticalConnect.Count > 0)
                {
                    while (verticalConnect.Count > 0)
                    {
                        int[] point = verticalConnect.Pop();
                        a = point[0] + 1;
                        b = point[1];
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            horizontalConnect.Push(pointi);
                            a++;
                        }
                        a = point[0] - 1;
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            horizontalConnect.Push(pointi);
                            a--;
                        }
                    }
                    while (horizontalConnect.Count > 0)
                    {
                        int[] point = horizontalConnect.Pop();
                        a = point[0];
                        b = point[1] + 1;
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            verticalConnect.Push(pointi);
                            b++;
                        }
                        b = point[1] - 1;
                        while (map[a][b])
                        {
                            map[a][b] = false;
                            int[] pointi = new int[] { a, b };
                            connect.Push(pointi);
                            verticalConnect.Push(pointi);
                            b--;
                        }
                    }
                }
                int count = connect.Count;
                while (connect.Count > 0)
                {
                    int[] point = connect.Pop();
                    map[point[0]][point[1]] = true;
                }
                return count + road.Count < _rest;
            }
            else
            {
                return false;
            }
        }

        private static void HorizontalConnect(bool[][] map, Stack<int[]> horizontalConnect, Stack<int[]> verticalConnect, Stack<int[]> connect)
        {
            while (horizontalConnect.Count > 0)
            {
                int[] point = horizontalConnect.Pop();
                int a = point[0], b = point[1] + 1;
                while (map[a][b])
                {
                    map[a][b] = false;
                    int[] pointi = new int[] { a, b };
                    connect.Push(pointi);
                    verticalConnect.Push(pointi);
                    b++;
                }
                b = point[1] - 1;
                while (map[a][b])
                {
                    map[a][b] = false;
                    int[] pointi = new int[] { a, b };
                    connect.Push(pointi);
                    verticalConnect.Push(pointi);
                    b--;
                }
            }
            if (verticalConnect.Count > 0)
            {
                VerticalConnect(map, horizontalConnect, verticalConnect, connect);
            }
        }

        private static void VerticalConnect(bool[][] map, Stack<int[]> horizontalConnect, Stack<int[]> verticalConnect, Stack<int[]> connect)
        {
            while (verticalConnect.Count > 0)
            {
                int[] point = verticalConnect.Pop();
                int a = point[0] + 1, b = point[1];
                while (map[a][b])
                {
                    map[a][b] = false;
                    int[] pointi = new int[] { a, b };
                    connect.Push(pointi);
                    horizontalConnect.Push(pointi);
                    a++;
                }
                a = point[0] - 1;
                while (map[a][b])
                {
                    map[a][b] = false;
                    int[] pointi = new int[] { a, b };
                    connect.Push(pointi);
                    horizontalConnect.Push(pointi);
                    a--;
                }
            }
            if (horizontalConnect.Count > 0)
            {
                HorizontalConnect(map, horizontalConnect, verticalConnect, connect);
            }
        }
    }
}
