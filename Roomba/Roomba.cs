using SufeiUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    public class Roomba
    {
        int level;
        int x;
        int y;
        string mapStr;
        int X;
        int Y;
        Stack<char> mapArray;
        bool[][] _map;
        Stack<int> restPoints;
        int restCount;
        int _a;
        int _b;
        bool done;
        Stack<char> path;
        int threadCount;
        object locker;
        int startPoint;

        public Roomba()
        {
            level = 191;
            restPoints = new Stack<int>();
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["threadCount"], out threadCount))
            {
                threadCount = 2;
            }
            locker = new object();
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["startPoint"], out startPoint))
            {
                startPoint = 0;
            }
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
                    Console.WriteLine("level:{0},x:{1},y{2} start, {3}", level, x, y, DateTime.Now.ToString());
                    hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", Clean());
                    Console.WriteLine("level:{0} end, {1}", level, DateTime.Now.ToString());
                    hr = hh.GetHtml(hi);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        string Clean()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            X = x + 2;
            Y = y + 2;
            mapArray = new Stack<char>(mapStr);
            _map = new bool[X][];
            restPoints.Clear();
            for (int i = x; i > 0; i--)
            {
                _map[i] = new bool[Y];
                for (int j = y; j > 0; j--)
                {
                    if (mapArray.Pop() == '0')
                    {
                        _map[i][j] = true;
                        restPoints.Push(j);
                        restPoints.Push(i);
                    }
                }
            }
            _map[0] = new bool[Y];
            _map[X - 1] = new bool[Y];
            restCount = restPoints.Count / 2;
            if (startPoint == 0)
            {
                startPoint = restCount;
            }
            while (restPoints.Count / 2 > startPoint)
            {
                restPoints.Pop();
                restPoints.Pop();
            }
            startPoint = 0;
            List<Task> taskList = new List<Task>();
            done = false;
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
            string result = string.Format("x={0}&y={1}&path={2}", _a, _b, sb);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            return result;
        }

        void CleanUnit()
        {
            int a = 0, b = 0;
            bool[][] map = new bool[X][];
            for (int i = 0; i < X; i++)
            {
                map[i] = new bool[Y];
                for (int j = 0; j < Y; j++)
                {
                    map[i][j] = _map[i][j];
                }
            }
            Stack<int> roadx = new Stack<int>(), roady = new Stack<int>();
            while (!done)
            {
                lock (locker)
                {
                    if (restPoints.Count > 0)
                    {
                        a = restPoints.Pop();
                        b = restPoints.Pop();
                        Console.WriteLine("point x:{0},y:{1} start, {2} points rest, {3}", a, b, restPoints.Count / 2, DateTime.Now.ToShortTimeString());
                    }
                    else
                    {
                        break;
                    }
                }
                if (Clean(map, a, b, roadx, roady))
                {
                    lock (locker)
                    {
                        done = true;
                        _a = a;
                        _b = b;
                        path = new Stack<char>();
                        char direction;
                        X = roadx.Pop();
                        Y = roady.Pop();
                        x = roadx.Peek();
                        y = roady.Peek();
                        if (X == x)
                        {
                            direction = Y > y ? 'r' : 'l';
                        }
                        else
                        {
                            direction = X > x ? 'd' : 'u';
                        }
                        path.Push(direction);
                        while (roadx.Count > 1)
                        {
                            X = roadx.Pop();
                            Y = roady.Pop();
                            x = roadx.Peek();
                            y = roady.Peek();
                            if (X == x)
                            {
                                direction = Y > y ? 'r' : 'l';
                            }
                            else
                            {
                                direction = X > x ? 'd' : 'u';
                            }
                            if (direction != path.Peek())
                            {
                                path.Push(direction);
                            }
                        }
                    }
                }
            }
        }

        bool Clean(bool[][] map, int a, int b, Stack<int> roadx, Stack<int> roady)
        {
            Stack<int> moveStack = new Stack<int>();
            Stack<bool> directionStack = new Stack<bool>();
            Stack<int> restore = new Stack<int>();
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>();
            int roadCount;
            int move;
            bool oneway;
            map[a][b] = false;
            roadx.Push(a);
            roady.Push(b);
            if (map[a - 1][b])
            {
                moveStack.Push(-1);
                directionStack.Push(false);
            }
            if (map[a][b - 1])
            {
                moveStack.Push(-1);
                directionStack.Push(true);
            }
            if (map[a + 1][b])
            {
                moveStack.Push(1);
                directionStack.Push(false);
            }
            if (map[a][b + 1])
            {
                moveStack.Push(1);
                directionStack.Push(true);
            }
            while (moveStack.Count > 0)
            {
                move = moveStack.Pop();
                if (move == 0)
                {
                    roadCount = restore.Pop();
                    while (roadx.Count > roadCount)
                    {
                        map[roadx.Pop()][roady.Pop()] = true;
                    }
                    a = roadx.Peek();
                    b = roady.Peek();
                }
                else
                {
                    moveStack.Push(0);
                    restore.Push(roadx.Count);
                    oneway = true;
                    do
                    {
                        if (directionStack.Pop())
                        {
                            b += move;
                            do
                            {
                                map[a][b] = false;
                                roadx.Push(a);
                                roady.Push(b);
                                b += move;
                            } while (map[a][b]);
                            b -= move;
                            if (map[a + 1][b])
                            {
                                if (map[a - 1][b])
                                {
                                    oneway = false;
                                    roadCount = roadx.Count;
                                    a++;
                                    do
                                    {
                                        map[a][b] = false;
                                        roadx.Push(a);
                                        roady.Push(b);
                                        horizontalConnect.Push(b);
                                        horizontalConnect.Push(a);
                                        a++;
                                    } while (map[a][b]);
                                    do
                                    {
                                        while (horizontalConnect.Count > 0)
                                        {
                                            a = horizontalConnect.Pop();
                                            move = horizontalConnect.Pop();
                                            b = move + 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                verticalConnect.Push(b);
                                                verticalConnect.Push(a);
                                                b++;
                                            }
                                            b = move - 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                verticalConnect.Push(b);
                                                verticalConnect.Push(a);
                                                b--;
                                            }
                                        }
                                        while (verticalConnect.Count > 0)
                                        {
                                            move = verticalConnect.Pop();
                                            b = verticalConnect.Pop();
                                            a = move + 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                horizontalConnect.Push(b);
                                                horizontalConnect.Push(a);
                                                a++;
                                            }
                                            a = move - 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                horizontalConnect.Push(b);
                                                horizontalConnect.Push(a);
                                                a--;
                                            }
                                        }
                                    } while (horizontalConnect.Count > 0);
                                    if (roadx.Count == restCount)
                                    {
                                        while (roadx.Count > roadCount)
                                        {
                                            map[roadx.Peek()][roady.Peek()] = true;
                                            horizontalConnect.Push(roady.Pop());
                                            horizontalConnect.Push(roadx.Pop());
                                        }
                                        roadCount = 0;
                                        map[roadx.Peek()][roady.Peek()] = true;
                                        do
                                        {
                                            a = horizontalConnect.Pop();
                                            b = horizontalConnect.Pop();
                                            move = 0;
                                            if (map[a][b + 1]) move++;
                                            if (map[a + 1][b]) move++;
                                            if (move < 2 && map[a][b - 1]) move++;
                                            if (move < 2 && map[a - 1][b]) move++;
                                            if (move < 2) roadCount++;
                                        } while (roadCount < 2 && horizontalConnect.Count > 0);
                                        if (roadCount < 2)
                                        {
                                            moveStack.Push(-1);
                                            directionStack.Push(false);
                                            moveStack.Push(1);
                                            directionStack.Push(false);
                                            a = roadx.Peek();
                                            b = roady.Peek();
                                            map[a][b] = false;
                                        }
                                        else
                                        {
                                            horizontalConnect.Clear();
                                        }
                                    }
                                    else
                                    {
                                        while (roadx.Count > roadCount)
                                        {
                                            map[roadx.Pop()][roady.Pop()] = true;
                                        }
                                    }
                                }
                                else
                                {
                                    move = 1;
                                    directionStack.Push(false);
                                }
                            }
                            else
                            {
                                if (map[a - 1][b])
                                {
                                    move = -1;
                                    directionStack.Push(false);
                                }
                                else
                                {
                                    if (roadx.Count == restCount)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        oneway = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            a += move;
                            do
                            {
                                map[a][b] = false;
                                roadx.Push(a);
                                roady.Push(b);
                                a += move;
                            } while (map[a][b]);
                            a -= move;
                            if (map[a][b + 1])
                            {
                                if (map[a][b - 1])
                                {
                                    oneway = false;
                                    roadCount = roadx.Count;
                                    b++;
                                    do
                                    {
                                        map[a][b] = false;
                                        roadx.Push(a);
                                        roady.Push(b);
                                        verticalConnect.Push(b);
                                        verticalConnect.Push(a);
                                        b++;
                                    } while (map[a][b]);
                                    do
                                    {
                                        while (verticalConnect.Count > 0)
                                        {
                                            move = verticalConnect.Pop();
                                            b = verticalConnect.Pop();
                                            a = move + 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                horizontalConnect.Push(b);
                                                horizontalConnect.Push(a);
                                                a++;
                                            }
                                            a = move - 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                horizontalConnect.Push(b);
                                                horizontalConnect.Push(a);
                                                a--;
                                            }
                                        }
                                        while (horizontalConnect.Count > 0)
                                        {
                                            a = horizontalConnect.Pop();
                                            move = horizontalConnect.Pop();
                                            b = move + 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                verticalConnect.Push(b);
                                                verticalConnect.Push(a);
                                                b++;
                                            }
                                            b = move - 1;
                                            while (map[a][b])
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                verticalConnect.Push(b);
                                                verticalConnect.Push(a);
                                                b--;
                                            }
                                        }
                                    } while (verticalConnect.Count > 0);
                                    if (roadx.Count == restCount)
                                    {
                                        while (roadx.Count > roadCount)
                                        {
                                            map[roadx.Peek()][roady.Peek()] = true;
                                            verticalConnect.Push(roady.Pop());
                                            verticalConnect.Push(roadx.Pop());
                                        }
                                        roadCount = 0;
                                        map[roadx.Peek()][roady.Peek()] = true;
                                        do
                                        {
                                            a = verticalConnect.Pop();
                                            b = verticalConnect.Pop();
                                            move = 0;
                                            if (map[a][b + 1]) move++;
                                            if (map[a + 1][b]) move++;
                                            if (move < 2 && map[a][b - 1]) move++;
                                            if (move < 2 && map[a - 1][b]) move++;
                                            if (move < 2) roadCount++;
                                        } while (roadCount < 2 && verticalConnect.Count > 0);
                                        if (roadCount < 2)
                                        {
                                            moveStack.Push(-1);
                                            directionStack.Push(true);
                                            moveStack.Push(1);
                                            directionStack.Push(true);
                                            a = roadx.Peek();
                                            b = roady.Peek();
                                            map[a][b] = false;
                                        }
                                        else
                                        {
                                            verticalConnect.Clear();
                                        }
                                    }
                                    else
                                    {
                                        while (roadx.Count > roadCount)
                                        {
                                            map[roadx.Pop()][roady.Pop()] = true;
                                        }
                                    }
                                }
                                else
                                {
                                    move = 1;
                                    directionStack.Push(true);
                                }
                            }
                            else
                            {
                                if (map[a][b - 1])
                                {
                                    move = -1;
                                    directionStack.Push(true);
                                }
                                else
                                {
                                    if (roadx.Count == restCount)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        oneway = false;
                                    }
                                }
                            }
                        }
                    } while (oneway);
                }
            }
            map[a][b] = true;
            roadx.Pop();
            roady.Pop();
            return false;
        }
    }
}
