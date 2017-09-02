using SufeiUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    public class Roomba
    {
        private int level;
        private int x;
        private int y;
        private string mapStr;
        private int X;
        private int Y;
        private Stack<char> mapArray;
        private bool[][] map;
        private Stack<int> initRestPoints;
        private int restCount;
        private bool done;
        private int a;
        private int b;
        private Stack<char> path;
        private int threadCount;
        private object locker;

        public Roomba()
        {
            initRestPoints = new Stack<int>();
            level = 89;
            path = new Stack<char>();
            threadCount = 4;
            locker = new object();
        }

        public void Auto()
        {
            try
            {
                HttpHelper hh = new HttpHelper();
                HttpItem hi = new HttpItem()
                {
                    Cookie = "laravel_session=eyJpdiI6IlwvRG5neDVtN1lOSGh2WU82dldoNXZ3PT0iLCJ2YWx1ZSI6ImFLV2tSKzJ4Vlc2RWlCWHltOWU1bHpOQUdZalwvWDNTcSt3UGhlT0lQVWdMMGNLcFROekE0T256UHBZekZRS3QwVFd1MEVic2swaTJ0ZEgwMFpGZUxKQT09IiwibWFjIjoiOGQ3MDkwNmVhYmQ0ZTU0NDQ3ODAxNDQ0YzVkYzg4ZTBiMWE5NGIxNzliYjkyMGUzMmE3NGE3YjQ0NWRiNjJjYyJ9"
                };
                HttpResult hr;
                while (true)
                {
                    //hi.URL = "http://www.qlcoder.com/train/autocr";
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
                    Console.WriteLine("level:{0},x:{1},y{2} start, {3}", level, x, y, DateTime.Now.ToString());
                    hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", Clean());
                    //hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", CleanMultithreaded());
                    Console.WriteLine("level:{0} end, {1}", level, DateTime.Now.ToString());
                    hr = hh.GetHtml(hi);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #region single
        private string Clean()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            X = x + 2;
            Y = y + 2;
            mapArray = new Stack<char>(mapStr);
            map = new bool[X][];
            initRestPoints.Clear();
            for (int i = x; i > 0; i--)
            {
                map[i] = new bool[Y];
                for (int j = y; j > 0; j--)
                {
                    if (mapArray.Pop() == '0')
                    {
                        map[i][j] = true;
                        initRestPoints.Push(j);
                        initRestPoints.Push(i);
                    }
                }
            }
            map[0] = new bool[Y];
            map[X - 1] = new bool[Y];
            restCount = initRestPoints.Count - 2;
            Stack<char> path = new Stack<char>();
            Stack<int> road = new Stack<int>();
            int a = 0, b = 0;
            while (initRestPoints.Count > 0)
            {
                a = initRestPoints.Pop();
                b = initRestPoints.Pop();
                Console.WriteLine("point x:{0},y:{1} start, {2} points rest, {3}", a, b, initRestPoints.Count / 2, DateTime.Now.ToString());
                map[a][b] = false;
                if ((map[a][b + 1] && RClean(a, b, path, road)) || (map[a + 1][b] && DClean(a, b, path, road)) || (map[a][b - 1] && LClean(a, b, path, road)) || (map[a - 1][b] && UClean(a, b, path, road)))
                {
                    break;
                }
                else
                {
                    map[a][b] = true;
                }
            }
            StringBuilder sb = new StringBuilder();
            while (path.Count > 0)
            {
                sb.Append(path.Pop());
            }
            string result = string.Format("x={0}&y={1}&path={2}", a, b, sb);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            return result;
        }

        private bool RClean(int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            b++;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                b++;
            } while (map[a][b]);
            b--;
            if (map[a + 1][b])
            {
                if (map[a - 1][b])
                {
                    if (HorizontalPrune(a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = DClean(a, b, path, road) || UClean(a, b, path, road);
                    }
                }
                else
                {
                    result = DClean(a, b, path, road);
                }
            }
            else
            {
                if (map[a - 1][b])
                {
                    result = UClean(a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('r');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool LClean(int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            b--;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                b--;
            } while (map[a][b]);
            b++;
            if (map[a + 1][b])
            {
                if (map[a - 1][b])
                {
                    if (HorizontalPrune(a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = DClean(a, b, path, road) || UClean(a, b, path, road);
                    }
                }
                else
                {
                    result = DClean(a, b, path, road);
                }
            }
            else
            {
                if (map[a - 1][b])
                {
                    result = UClean(a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('l');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool DClean(int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            a++;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                a++;
            } while (map[a][b]);
            a--;
            if (map[a][b + 1])
            {
                if (map[a][b - 1])
                {
                    if (VerticalPrune(a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = RClean(a, b, path, road) || LClean(a, b, path, road);
                    }
                }
                else
                {
                    result = RClean(a, b, path, road);
                }
            }
            else
            {
                if (map[a][b - 1])
                {
                    result = LClean(a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('d');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool UClean(int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            a--;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                a--;
            } while (map[a][b]);
            a++;
            if (map[a][b + 1])
            {
                if (map[a][b - 1])
                {
                    if (VerticalPrune(a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = RClean(a, b, path, road) || LClean(a, b, path, road);
                    }
                }
                else
                {
                    result = RClean(a, b, path, road);
                }
            }
            else
            {
                if (map[a][b - 1])
                {
                    result = LClean(a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('u');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool HorizontalPrune(int a, int b, int roadCount)
        {
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>(), connect = new Stack<int>();
            a++;
            int c;
            do
            {
                map[a][b] = false;
                horizontalConnect.Push(b);
                horizontalConnect.Push(a);
                connect.Push(b);
                connect.Push(a);
                a++;
            } while (map[a][b]);
            do
            {
                while (horizontalConnect.Count > 0)
                {
                    a = horizontalConnect.Pop();
                    c = horizontalConnect.Pop();
                    b = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b++;
                    }
                    b = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b--;
                    }
                }
                while (verticalConnect.Count > 0)
                {
                    c = verticalConnect.Pop();
                    b = verticalConnect.Pop();
                    a = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a++;
                    }
                    a = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a--;
                    }
                }
            } while (horizontalConnect.Count > 0);
            int count = connect.Count;
            while (connect.Count > 0)
            {
                map[connect.Pop()][connect.Pop()] = true;
            }
            return count + roadCount < restCount;
        }

        private bool VerticalPrune(int a, int b, int roadCount)
        {
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>(), connect = new Stack<int>();
            b++;
            int c;
            do
            {
                map[a][b] = false;
                verticalConnect.Push(b);
                verticalConnect.Push(a);
                connect.Push(b);
                connect.Push(a);
                b++;
            } while (map[a][b]);
            do
            {
                while (verticalConnect.Count > 0)
                {
                    c = verticalConnect.Pop();
                    b = verticalConnect.Pop();
                    a = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a++;
                    }
                    a = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a--;
                    }
                }
                while (horizontalConnect.Count > 0)
                {
                    a = horizontalConnect.Pop();
                    c = horizontalConnect.Pop();
                    b = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b++;
                    }
                    b = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b--;
                    }
                }
            } while (verticalConnect.Count > 0);
            int count = connect.Count;
            while (connect.Count > 0)
            {
                map[connect.Pop()][connect.Pop()] = true;
            }
            return count + roadCount < restCount;
        }
        #endregion
        #region multiple
        private string CleanMultithreaded()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            X = x + 2;
            Y = y + 2;
            mapArray = new Stack<char>(mapStr);
            map = new bool[X][];
            initRestPoints.Clear();
            for (int i = x; i > 0; i--)
            {
                map[i] = new bool[Y];
                for (int j = y; j > 0; j--)
                {
                    if (mapArray.Pop() == '0')
                    {
                        map[i][j] = true;
                        initRestPoints.Push(j);
                        initRestPoints.Push(i);
                    }
                }
            }
            map[0] = new bool[Y];
            map[X - 1] = new bool[Y];
            restCount = initRestPoints.Count - 2;
            List<Task> taskList = new List<Task>();
            done = false;
            path.Clear();
            for (int i = 0; i < threadCount; i++)
            {
                Task task = new Task(CleanUnit);
                task.Start();
                taskList.Add(task);
            }
            Task.WaitAll(taskList.ToArray());
            StringBuilder sb = new StringBuilder();
            while (path.Count > 0)
            {
                sb.Append(path.Pop());
            }
            string result = string.Format("x={0}&y={1}&path={2}", a, b, sb);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            return result;
        }

        private void CleanUnit()
        {
            int a = 0, b = 0;
            bool[][] map = new bool[X][];
            for (int i = 0; i < X; i++)
            {
                map[i] = new bool[Y];
                for (int j = 0; j < Y; j++)
                {
                    map[i][j] = this.map[i][j];
                }
            }
            Stack<char> path = new Stack<char>();
            Stack<int> road = new Stack<int>();
            while (!done)
            {
                lock (locker)
                {
                    if (initRestPoints.Count > 0)
                    {
                        a = initRestPoints.Pop();
                        b = initRestPoints.Pop();
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine("point x:{0},y:{1} start, {2} points rest, {3}", a, b, initRestPoints.Count / 2, DateTime.Now.ToString());
                map[a][b] = false;
                if ((map[a][b + 1] && RClean(map, a, b, path, road)) || (map[a + 1][b] && DClean(map, a, b, path, road)) || (map[a][b - 1] && LClean(map, a, b, path, road)) || (map[a - 1][b] && UClean(map, a, b, path, road)))
                {
                    lock (locker)
                    {
                        this.a = a;
                        this.b = b;
                        this.path = path;
                        done = true;
                    }
                    break;
                }
                else
                {
                    map[a][b] = true;
                }
            }
        }

        private bool RClean(bool[][] map, int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            b++;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                b++;
            } while (map[a][b]);
            b--;
            if (map[a + 1][b])
            {
                if (map[a - 1][b])
                {
                    if (HorizontalPrune(map, a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = DClean(map, a, b, path, road) || UClean(map, a, b, path, road);
                    }
                }
                else
                {
                    result = DClean(map, a, b, path, road);
                }
            }
            else
            {
                if (map[a - 1][b])
                {
                    result = UClean(map, a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('r');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool LClean(bool[][] map, int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            b--;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                b--;
            } while (map[a][b]);
            b++;
            if (map[a + 1][b])
            {
                if (map[a - 1][b])
                {
                    if (HorizontalPrune(map, a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = DClean(map, a, b, path, road) || UClean(map, a, b, path, road);
                    }
                }
                else
                {
                    result = DClean(map, a, b, path, road);
                }
            }
            else
            {
                if (map[a - 1][b])
                {
                    result = UClean(map, a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('l');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool DClean(bool[][] map, int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            a++;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                a++;
            } while (map[a][b]);
            a--;
            if (map[a][b + 1])
            {
                if (map[a][b - 1])
                {
                    if (VerticalPrune(map, a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = RClean(map, a, b, path, road) || LClean(map, a, b, path, road);
                    }
                }
                else
                {
                    result = RClean(map, a, b, path, road);
                }
            }
            else
            {
                if (map[a][b - 1])
                {
                    result = LClean(map, a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('d');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool UClean(bool[][] map, int a, int b, Stack<char> path, Stack<int> road)
        {
            bool result;
            int roadCount = road.Count;
            a--;
            do
            {
                map[a][b] = false;
                road.Push(b);
                road.Push(a);
                a--;
            } while (map[a][b]);
            a++;
            if (map[a][b + 1])
            {
                if (map[a][b - 1])
                {
                    if (VerticalPrune(map, a, b, road.Count))
                    {
                        result = false;
                    }
                    else
                    {
                        result = RClean(map, a, b, path, road) || LClean(map, a, b, path, road);
                    }
                }
                else
                {
                    result = RClean(map, a, b, path, road);
                }
            }
            else
            {
                if (map[a][b - 1])
                {
                    result = LClean(map, a, b, path, road);
                }
                else
                {
                    result = road.Count == restCount;
                }
            }
            if (result)
            {
                path.Push('u');
            }
            else
            {
                while (road.Count > roadCount)
                {
                    map[road.Pop()][road.Pop()] = true;
                }
            }
            return result;
        }

        private bool HorizontalPrune(bool[][] map, int a, int b, int roadCount)
        {
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>(), connect = new Stack<int>();
            a++;
            int c;
            do
            {
                map[a][b] = false;
                horizontalConnect.Push(b);
                horizontalConnect.Push(a);
                connect.Push(b);
                connect.Push(a);
                a++;
            } while (map[a][b]);
            do
            {
                while (horizontalConnect.Count > 0)
                {
                    a = horizontalConnect.Pop();
                    c = horizontalConnect.Pop();
                    b = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b++;
                    }
                    b = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b--;
                    }
                }
                while (verticalConnect.Count > 0)
                {
                    c = verticalConnect.Pop();
                    b = verticalConnect.Pop();
                    a = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a++;
                    }
                    a = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a--;
                    }
                }
            } while (horizontalConnect.Count > 0);
            int count = connect.Count;
            while (connect.Count > 0)
            {
                map[connect.Pop()][connect.Pop()] = true;
            }
            return count + roadCount < restCount;
        }

        private bool VerticalPrune(bool[][] map, int a, int b, int roadCount)
        {
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>(), connect = new Stack<int>();
            b++;
            int c;
            do
            {
                map[a][b] = false;
                verticalConnect.Push(b);
                verticalConnect.Push(a);
                connect.Push(b);
                connect.Push(a);
                b++;
            } while (map[a][b]);
            do
            {
                while (verticalConnect.Count > 0)
                {
                    c = verticalConnect.Pop();
                    b = verticalConnect.Pop();
                    a = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a++;
                    }
                    a = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        horizontalConnect.Push(b);
                        horizontalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        a--;
                    }
                }
                while (horizontalConnect.Count > 0)
                {
                    a = horizontalConnect.Pop();
                    c = horizontalConnect.Pop();
                    b = c + 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b++;
                    }
                    b = c - 1;
                    while (map[a][b])
                    {
                        map[a][b] = false;
                        verticalConnect.Push(b);
                        verticalConnect.Push(a);
                        connect.Push(b);
                        connect.Push(a);
                        b--;
                    }
                }
            } while (verticalConnect.Count > 0);
            int count = connect.Count;
            while (connect.Count > 0)
            {
                map[connect.Pop()][connect.Pop()] = true;
            }
            return count + roadCount < restCount;
        }
        #endregion
    }
}
