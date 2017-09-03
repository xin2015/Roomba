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
        int level;
        int x;
        int y;
        string mapStr;
        int X;
        int Y;
        Stack<char> mapArray;
        bool[][] map;
        Stack<int> initRestPoints;
        int restCount;
        Stack<int> roadx;
        Stack<int> roady;
        int startIndex;

        public Roomba()
        {
            level = 101;
            initRestPoints = new Stack<int>();
            roadx = new Stack<int>();
            roady = new Stack<int>();
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["startIndex"], out startIndex))
            {
                startIndex = 0;
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
            restCount = initRestPoints.Count / 2;
            roadx.Clear();
            roady.Clear();
            int a = 0, b = 0;
            if (startIndex == 0)
            {
                startIndex = restCount;
            }
            while (initRestPoints.Count > 0)
            {
                a = initRestPoints.Pop();
                b = initRestPoints.Pop();
                if (initRestPoints.Count / 2 > startIndex)
                {
                    continue;
                }
                if (Clean(a, b))
                {
                    break;
                }
            }
            StringBuilder sb = new StringBuilder();
            Stack<char> path = new Stack<char>();
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
            while (path.Count > 0)
            {
                sb.Append(path.Pop());
            }
            string result = string.Format("x={0}&y={1}&path={2}", a, b, sb);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            startIndex = 0;
            return result;
        }

        bool Clean(int a, int b)
        {
            Console.WriteLine("point x:{0},y:{1} start, {2} points rest, {3}", a, b, initRestPoints.Count / 2, DateTime.Now.ToString());
            Stack<int> work = new Stack<int>();
            Stack<int> restore = new Stack<int>();
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>(), connect = new Stack<int>();
            int roadCount, movex, movey;
            bool oneway;
            map[a][b] = false;
            roadx.Push(a);
            roady.Push(b);
            if (map[a - 1][b])
            {
                work.Push(0);
                work.Push(-1);
            }
            if (map[a][b - 1])
            {
                work.Push(-1);
                work.Push(0);
            }
            if (map[a + 1][b])
            {
                work.Push(0);
                work.Push(1);
            }
            if (map[a][b + 1])
            {
                work.Push(1);
                work.Push(0);
            }
            while (work.Count > 0)
            {
                movex = work.Pop();
                if (movex == 2)
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
                    movey = work.Pop();
                    work.Push(2);
                    restore.Push(roadx.Count);
                    do
                    {
                        if (movex == 0)
                        {
                            b += movey;
                            do
                            {
                                map[a][b] = false;
                                roadx.Push(a);
                                roady.Push(b);
                                b += movey;
                            } while (map[a][b]);
                            b -= movey;
                            if (map[a + 1][b] == map[a - 1][b])
                            {
                                oneway = false;
                            }
                            else
                            {
                                oneway = true;
                                movex = map[a + 1][b] ? 1 : -1;
                                movey = 0;
                            }
                        }
                        else
                        {
                            a += movex;
                            do
                            {
                                map[a][b] = false;
                                roadx.Push(a);
                                roady.Push(b);
                                a += movex;
                            } while (map[a][b]);
                            a -= movex;
                            if (map[a][b + 1] == map[a][b - 1])
                            {
                                oneway = false;
                            }
                            else
                            {
                                oneway = true;
                                movey = map[a][b + 1] ? 1 : -1;
                                movex = 0;
                            }
                        }
                    } while (oneway);
                    if (movex == 0)
                    {
                        if (map[a + 1][b])
                        {
                            a++;
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
                                    movex = horizontalConnect.Pop();
                                    b = movex + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        verticalConnect.Push(b);
                                        verticalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        b++;
                                    }
                                    b = movex - 1;
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
                                    movex = verticalConnect.Pop();
                                    b = verticalConnect.Pop();
                                    a = movex + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        horizontalConnect.Push(b);
                                        horizontalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        a++;
                                    }
                                    a = movex - 1;
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
                            roadCount = connect.Count / 2;
                            while (connect.Count > 0)
                            {
                                map[connect.Pop()][connect.Pop()] = true;
                            }
                            if (roadCount + roadx.Count == restCount)
                            {
                                work.Push(0);
                                work.Push(-1);
                                work.Push(0);
                                work.Push(1);
                                a = roadx.Peek();
                                b = roady.Peek();
                            }
                        }
                        else
                        {
                            if (roadx.Count == restCount)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (map[a][b + 1])
                        {
                            b++;
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
                                    movex = verticalConnect.Pop();
                                    b = verticalConnect.Pop();
                                    a = movex + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        horizontalConnect.Push(b);
                                        horizontalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        a++;
                                    }
                                    a = movex - 1;
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
                                    movex = horizontalConnect.Pop();
                                    b = movex + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        verticalConnect.Push(b);
                                        verticalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        b++;
                                    }
                                    b = movex - 1;
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
                            roadCount = connect.Count / 2;
                            while (connect.Count > 0)
                            {
                                map[connect.Pop()][connect.Pop()] = true;
                            }
                            if (roadCount + roadx.Count == restCount)
                            {
                                work.Push(-1);
                                work.Push(0);
                                work.Push(1);
                                work.Push(0);
                                a = roadx.Peek();
                                b = roady.Peek();
                            }
                        }
                        else
                        {
                            if (roadx.Count == restCount)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            map[a][b] = true;
            roadx.Pop();
            roady.Pop();
            return false;
        }
    }
}
