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
            Stack<int> moveStack = new Stack<int>();
            Stack<bool> directionStack = new Stack<bool>();
            Stack<int> restore = new Stack<int>();
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>(), connect = new Stack<int>();
            int roadCount, move;
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
                            directionStack.Push(false);
                            if (map[a + 1][b] == map[a - 1][b])
                            {
                                oneway = false;
                            }
                            else
                            {
                                oneway = true;
                                move = map[a + 1][b] ? 1 : -1;
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
                            directionStack.Push(true);
                            if (map[a][b + 1] == map[a][b - 1])
                            {
                                oneway = false;
                            }
                            else
                            {
                                oneway = true;
                                move = map[a][b + 1] ? 1 : -1;
                            }
                        }
                    } while (oneway);
                    if (directionStack.Pop())
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
                                    move = verticalConnect.Pop();
                                    b = verticalConnect.Pop();
                                    a = move + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        horizontalConnect.Push(b);
                                        horizontalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        a++;
                                    }
                                    a = move - 1;
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
                                    move = horizontalConnect.Pop();
                                    b = move + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        verticalConnect.Push(b);
                                        verticalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        b++;
                                    }
                                    b = move - 1;
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
                            if (roadCount + roadx.Count == restCount)
                            {
                                horizontalConnect = new Stack<int>(connect);
                                while (connect.Count > 0)
                                {
                                    map[connect.Pop()][connect.Pop()] = true;
                                }
                                roadCount = 0;
                                map[roadx.Peek()][roady.Peek()] = true;
                                while (roadCount < 2 && horizontalConnect.Count > 0)
                                {
                                    b = horizontalConnect.Pop();
                                    a = horizontalConnect.Pop();
                                    move = 0;
                                    if (move < 2 && map[a][b + 1]) move++;
                                    if (move < 2 && map[a + 1][b]) move++;
                                    if (move < 2 && map[a][b - 1]) move++;
                                    if (move < 2 && map[a - 1][b]) move++;
                                    if (move < 2) roadCount++;
                                }
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
                                    horizontalConnect.Clear();
                                }
                            }
                            else
                            {
                                while (connect.Count > 0)
                                {
                                    map[connect.Pop()][connect.Pop()] = true;
                                }
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
                                    move = horizontalConnect.Pop();
                                    b = move + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        verticalConnect.Push(b);
                                        verticalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        b++;
                                    }
                                    b = move - 1;
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
                                    move = verticalConnect.Pop();
                                    b = verticalConnect.Pop();
                                    a = move + 1;
                                    while (map[a][b])
                                    {
                                        map[a][b] = false;
                                        horizontalConnect.Push(b);
                                        horizontalConnect.Push(a);
                                        connect.Push(b);
                                        connect.Push(a);
                                        a++;
                                    }
                                    a = move - 1;
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
                            if (roadCount + roadx.Count == restCount)
                            {
                                verticalConnect = new Stack<int>(connect);
                                while (connect.Count > 0)
                                {
                                    map[connect.Pop()][connect.Pop()] = true;
                                }
                                roadCount = 0;
                                map[roadx.Peek()][roady.Peek()] = true;
                                while (roadCount < 2 && verticalConnect.Count > 0)
                                {
                                    b = verticalConnect.Pop();
                                    a = verticalConnect.Pop();
                                    move = 0;
                                    if (move < 2 && map[a][b + 1]) move++;
                                    if (move < 2 && map[a + 1][b]) move++;
                                    if (move < 2 && map[a][b - 1]) move++;
                                    if (move < 2 && map[a - 1][b]) move++;
                                    if (move < 2) roadCount++;
                                }
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
                                    verticalConnect.Clear();
                                }
                            }
                            else
                            {
                                while (connect.Count > 0)
                                {
                                    map[connect.Pop()][connect.Pop()] = true;
                                }
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
