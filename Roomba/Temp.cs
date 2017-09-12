using SufeiUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    public class Temp
    {
        int level;
        int x;
        int y;
        string mapStr;
        int X;
        int Y;
        int R;
        int D;
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
        string cookie;

        public Temp()
        {
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
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["level"], out level))
            {
                level = 0;
            }
            cookie = System.Configuration.ConfigurationManager.AppSettings["cookie"];
        }

        public void Auto()
        {
            try
            {
                HttpHelper hh = new HttpHelper();
                HttpItem hi = new HttpItem()
                {
                    Cookie = cookie
                };
                HttpResult hr;
                while (true)
                {
                    hi.URL = level == 0 ? "http://www.qlcoder.com/train/autocr" : string.Format("http://www.qlcoder.com/train/autocr?level={0}", level);
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
                    level++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        string Clean()
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                X = x + 2;
                Y = y + 2;
                R = y + 1;
                D = x + 1;
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        void CleanUnit()
        {
            int a = 0, b = 0;
            bool[][] map = new bool[X][];
            int[][] directionMap = new int[X][];
            int danger = 0;
            for (int i = 0; i < X; i++)
            {
                map[i] = new bool[Y];
                directionMap[i] = new int[Y];
                for (int j = 0; j < Y; j++)
                {
                    if (_map[i][j])
                    {
                        map[i][j] = true;
                        if (_map[i][j + 1]) directionMap[i][j]++;
                        if (_map[i + 1][j]) directionMap[i][j]++;
                        if (_map[i][j - 1]) directionMap[i][j]++;
                        if (_map[i - 1][j]) directionMap[i][j]++;
                        if (directionMap[i][j] == 1)
                        {
                            danger++;
                            Console.WriteLine("x:{0},y:{1} is danger.", i, j);
                        }
                    }
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
                        Console.WriteLine("point x:{0},y:{1} start, {2} points rest, {3}", a, b, restPoints.Count / 2, DateTime.Now.ToString("HH:mm:ss"));
                    }
                    else
                    {
                        break;
                    }
                }
                if (Clean(map, directionMap, danger, a, b, roadx, roady))
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

        void Connect(bool[][] map, int[][] directionMap, Stack<int> wall, Stack<int> connect)
        {
            int c, d, e;
            do
            {
                c = connect.Pop();
                d = connect.Pop();
                if (c == 1 || c == x || d == 1 || d == y)
                {
                    if (directionMap[0][0] == 0)
                    {
                        directionMap[0][0] = -1;
                        wall.Push(0);
                        wall.Push(0);
                        directionMap[0][R] = -1;
                        wall.Push(R);
                        wall.Push(0);
                        directionMap[D][0] = -1;
                        wall.Push(0);
                        wall.Push(D);
                        directionMap[D][R] = -1;
                        wall.Push(R);
                        wall.Push(D);
                        for (int i = 1; i < R; i++)
                        {
                            directionMap[0][i] = -1;
                            wall.Push(i);
                            wall.Push(0);
                            directionMap[D][i] = -1;
                            wall.Push(i);
                            wall.Push(D);
                            if (directionMap[1][i] == 0)
                            {
                                directionMap[1][i] = -1;
                                wall.Push(i);
                                wall.Push(1);
                                connect.Push(i);
                                connect.Push(1);
                            }
                            if (directionMap[x][i] == 0)
                            {
                                directionMap[x][i] = -1;
                                wall.Push(i);
                                wall.Push(x);
                                connect.Push(i);
                                connect.Push(x);
                            }
                        }
                        for (int i = 1; i < D; i++)
                        {
                            directionMap[i][0] = -1;
                            wall.Push(0);
                            wall.Push(i);
                            directionMap[i][R] = -1;
                            wall.Push(R);
                            wall.Push(i);
                            if (directionMap[i][1] == 0)
                            {
                                directionMap[i][1] = -1;
                                wall.Push(1);
                                wall.Push(i);
                                connect.Push(1);
                                connect.Push(i);
                            }
                            if (directionMap[i][y] == 0)
                            {
                                directionMap[i][y] = -1;
                                wall.Push(y);
                                wall.Push(i);
                                connect.Push(y);
                                connect.Push(i);
                            }
                        }
                    }
                    if (c == 1)
                    {
                        if (d == 1)
                        {
                            if (directionMap[2][2] == 0)
                            {
                                directionMap[2][2] = -1;
                                wall.Push(2);
                                wall.Push(2);
                                connect.Push(2);
                                connect.Push(2);
                            }
                        }
                        else if (d == y)
                        {
                            if (directionMap[2][y - 1] == 0)
                            {
                                directionMap[2][y - 1] = -1;
                                wall.Push(y - 1);
                                wall.Push(2);
                                connect.Push(y - 1);
                                connect.Push(2);
                            }
                        }
                        else
                        {
                            if (directionMap[2][d] == 0)
                            {
                                directionMap[2][d] = -1;
                                wall.Push(d);
                                wall.Push(2);
                                connect.Push(d);
                                connect.Push(2);
                            }
                            if (directionMap[2][d - 1] == 0)
                            {
                                directionMap[2][d - 1] = -1;
                                wall.Push(d - 1);
                                wall.Push(2);
                                connect.Push(d - 1);
                                connect.Push(2);
                            }
                            if (directionMap[2][d + 1] == 0)
                            {
                                directionMap[2][d + 1] = -1;
                                wall.Push(d + 1);
                                wall.Push(2);
                                connect.Push(d + 1);
                                connect.Push(2);
                            }
                        }
                    }
                    else if (c == x)
                    {
                        e = x - 1;
                        if (d == 1)
                        {
                            if (directionMap[e][2] == 0)
                            {
                                directionMap[e][2] = -1;
                                wall.Push(2);
                                wall.Push(e);
                                connect.Push(2);
                                connect.Push(e);
                            }
                        }
                        else if (d == y)
                        {
                            if (directionMap[e][y - 1] == 0)
                            {
                                directionMap[e][y - 1] = -1;
                                wall.Push(y - 1);
                                wall.Push(e);
                                connect.Push(y - 1);
                                connect.Push(e);
                            }
                        }
                        else
                        {
                            if (directionMap[e][d] == 0)
                            {
                                directionMap[e][d] = -1;
                                wall.Push(d);
                                wall.Push(e);
                                connect.Push(d);
                                connect.Push(e);
                            }
                            if (directionMap[e][d - 1] == 0)
                            {
                                directionMap[e][d - 1] = -1;
                                wall.Push(d - 1);
                                wall.Push(e);
                                connect.Push(d - 1);
                                connect.Push(e);
                            }
                            if (directionMap[e][d + 1] == 0)
                            {
                                directionMap[e][d + 1] = -1;
                                wall.Push(d + 1);
                                wall.Push(e);
                                connect.Push(d + 1);
                                connect.Push(e);
                            }
                        }
                    }
                    else
                    {
                        if (d == 1)
                        {
                            if (directionMap[c][2] == 0)
                            {
                                directionMap[c][2] = -1;
                                wall.Push(2);
                                wall.Push(c);
                                connect.Push(2);
                                connect.Push(c);
                            }
                            if (directionMap[c - 1][2] == 0)
                            {
                                directionMap[c - 1][2] = -1;
                                wall.Push(2);
                                wall.Push(c - 1);
                                connect.Push(2);
                                connect.Push(c - 1);
                            }
                            if (directionMap[c + 1][2] == 0)
                            {
                                directionMap[c + 1][2] = -1;
                                wall.Push(2);
                                wall.Push(c + 1);
                                connect.Push(2);
                                connect.Push(c + 1);
                            }
                        }
                        else
                        {
                            e = y - 1;
                            if (directionMap[c][e] == 0)
                            {
                                directionMap[c][e] = -1;
                                wall.Push(e);
                                wall.Push(c);
                                connect.Push(e);
                                connect.Push(c);
                            }
                            if (directionMap[c - 1][e] == 0)
                            {
                                directionMap[c - 1][e] = -1;
                                wall.Push(e);
                                wall.Push(c - 1);
                                connect.Push(e);
                                connect.Push(c - 1);
                            }
                            if (directionMap[c + 1][e] == 0)
                            {
                                directionMap[c + 1][e] = -1;
                                wall.Push(e);
                                wall.Push(c + 1);
                                connect.Push(e);
                                connect.Push(c + 1);
                            }
                        }
                    }
                }
                else
                {
                    if (directionMap[c][d - 1] == 0)
                    {
                        directionMap[c][d - 1] = -1;
                        wall.Push(d - 1);
                        wall.Push(c);
                        connect.Push(d - 1);
                        connect.Push(c);
                    }
                    if (directionMap[c][d + 1] == 0)
                    {
                        directionMap[c][d + 1] = -1;
                        wall.Push(d + 1);
                        wall.Push(c);
                        connect.Push(d + 1);
                        connect.Push(c);
                    }
                    e = c - 1;
                    if (directionMap[e][d] == 0)
                    {
                        directionMap[e][d] = -1;
                        wall.Push(d);
                        wall.Push(e);
                        connect.Push(d);
                        connect.Push(e);
                    }
                    if (directionMap[e][d - 1] == 0)
                    {
                        directionMap[e][d - 1] = -1;
                        wall.Push(d - 1);
                        wall.Push(e);
                        connect.Push(d - 1);
                        connect.Push(e);
                    }
                    if (directionMap[e][d + 1] == 0)
                    {
                        directionMap[e][d + 1] = -1;
                        wall.Push(d + 1);
                        wall.Push(e);
                        connect.Push(d + 1);
                        connect.Push(e);
                    }
                    e = c + 1;
                    if (directionMap[e][d] == 0)
                    {
                        directionMap[e][d] = -1;
                        wall.Push(d);
                        wall.Push(e);
                        connect.Push(d);
                        connect.Push(e);
                    }
                    if (directionMap[e][d - 1] == 0)
                    {
                        directionMap[e][d - 1] = -1;
                        wall.Push(d - 1);
                        wall.Push(e);
                        connect.Push(d - 1);
                        connect.Push(e);
                    }
                    if (directionMap[e][d + 1] == 0)
                    {
                        directionMap[e][d + 1] = -1;
                        wall.Push(d + 1);
                        wall.Push(e);
                        connect.Push(d + 1);
                        connect.Push(e);
                    }
                }
            } while (connect.Count > 0);
        }

        bool Clean(bool[][] map, int[][] directionMap, int danger, int a, int b, Stack<int> roadx, Stack<int> roady)
        {
            Stack<int> moveStack = new Stack<int>();
            Stack<bool> directionStack = new Stack<bool>();
            Stack<int> restore = new Stack<int>();
            Stack<int> wall = new Stack<int>();
            Stack<int> connect = new Stack<int>();
            int roadCount, wallCount;
            int move;
            bool prune = true;
            if (danger > 1)
            {
                Console.WriteLine("Warning! danger is {0}", danger);
            }
            map[a][b] = false;
            roadx.Push(a);
            roady.Push(b);
            if (directionMap[a][b] == 1)
            {
                danger--;
                connect.Push(b);
                connect.Push(a);
                Connect(map, directionMap, wall, connect);
                if (map[a - 1][b])
                {
                    moveStack.Push(-1);
                    directionStack.Push(false);
                    directionMap[a - 1][b]--;
                    if (directionMap[a - 1][b] == 1)
                    {
                        danger++;
                    }
                }
                if (map[a][b - 1])
                {
                    moveStack.Push(-1);
                    directionStack.Push(true);
                    directionMap[a][b - 1]--;
                    if (directionMap[a][b - 1] == 1)
                    {
                        danger++;
                    }
                }
                if (map[a + 1][b])
                {
                    moveStack.Push(1);
                    directionStack.Push(false);
                    directionMap[a + 1][b]--;
                    if (directionMap[a + 1][b] == 1)
                    {
                        danger++;
                    }
                }
                if (map[a][b + 1])
                {
                    moveStack.Push(1);
                    directionStack.Push(true);
                    directionMap[a][b + 1]--;
                    if (directionMap[a][b + 1] == 1)
                    {
                        danger++;
                    }
                }
            }
            else
            {
                if (a == 1)
                {
                    if (b == 1)
                    {
                        moveStack.Push(1);
                        directionStack.Push(false);
                        directionMap[2][1]--;
                        if (directionMap[2][1] == 1)
                        {
                            danger++;
                        }
                        if (!map[2][2])
                        {
                            directionMap[2][2] = -1;
                            wall.Push(2);
                            wall.Push(2);
                            connect.Push(2);
                            connect.Push(2);
                            Connect(map, directionMap, wall, connect);
                        }
                        moveStack.Push(1);
                        directionStack.Push(true);
                        directionMap[1][2]--;
                        if (directionMap[1][2] == 1)
                        {
                            danger++;
                        }
                        if (directionMap[0][2] == 0)
                        {
                            connect.Push(1);
                            connect.Push(1);
                            Connect(map, directionMap, wall, connect);
                        }
                        else
                        {
                            prune = false;
                        }
                    }
                    else if (b == y)
                    {
                        moveStack.Push(-1);
                        directionStack.Push(true);
                        directionMap[1][b - 1]--;
                        if (!map[2][b - 1])
                        {
                            directionMap[2][b - 1] = -1;
                            wall.Push(b - 1);
                            wall.Push(2);
                            connect.Push(b - 1);
                            connect.Push(2);
                            Connect(map, directionMap, wall, connect);
                        }
                        moveStack.Push(1);
                        directionStack.Push(false);
                        directionMap[2][b]--;
                        if (directionMap[2][R] == 0)
                        {
                            connect.Push(R);
                            connect.Push(2);
                            Connect(map, directionMap, wall, connect);
                        }
                    }
                    else
                    {
                        if (map[1][b - 1])
                        {
                            moveStack.Push(-1);
                            directionStack.Push(true);
                            directionMap[1][b - 1]--;
                            if (!map[2][b - 1])
                            {
                                directionMap[2][b - 1] = -1;
                                wall.Push(b - 1);
                                wall.Push(2);
                                connect.Push(b - 1);
                                connect.Push(2);
                                Connect(map, directionMap, wall, connect);
                            }
                        }
                        if (map[2][b])
                        {
                            moveStack.Push(1);
                            directionStack.Push(false);
                            directionMap[2][b]--;
                            if (!map[2][b + 1])
                            {
                                if (directionMap[2][b + 1] == 0)
                                {
                                    directionMap[2][b + 1] = -1;
                                    wall.Push(b + 1);
                                    wall.Push(2);
                                    connect.Push(b + 1);
                                    connect.Push(2);
                                    Connect(map, directionMap, wall, connect);
                                }
                                else
                                {
                                    prune = false;
                                }
                            }
                        }
                        if (map[1][b + 1])
                        {
                            moveStack.Push(1);
                            directionStack.Push(true);
                            directionMap[2][b + 1]--;
                            if (directionMap[0][b + 1] == 0)
                            {
                                con
                            }
                        }
                    }
                }
            }
            if (danger == 0)
            {
                if (map[a - 1][b])
                {
                    moveStack.Push(-1);
                    directionStack.Push(false);
                    directionMap[a - 1][b]--;
                    if (directionMap[a - 1][b] == 1)
                    {
                        danger++;
                    }
                    if (directionMap[a - 1][b + 1] == 0)
                    {

                    }
                }
                if (map[a][b - 1])
                {
                    moveStack.Push(-1);
                    directionStack.Push(true);
                    directionMap[a][b - 1]--;
                    if (directionMap[a][b - 1] == 1)
                    {
                        danger++;
                    }
                }
                if (map[a + 1][b])
                {
                    moveStack.Push(1);
                    directionStack.Push(false);
                    directionMap[a + 1][b]--;
                    if (directionMap[a + 1][b] == 1)
                    {
                        danger++;
                    }
                }
                if (map[a][b + 1])
                {
                    moveStack.Push(1);
                    directionStack.Push(true);
                    directionMap[a][b + 1]--;
                    if (directionMap[a][b + 1] == 1)
                    {
                        danger++;
                    }
                }
            }
            else
            {
                if (directionMap[a][b] == 1)
                {
                    danger--;
                    connect.Push(b);
                    connect.Push(a);
                    Connect(map, directionMap, wall, connect);
                    if (map[a - 1][b])
                    {
                        moveStack.Push(-1);
                        directionStack.Push(false);
                        directionMap[a - 1][b]--;
                        if (directionMap[a - 1][b] == 1)
                        {
                            danger++;
                        }
                    }
                    if (map[a][b - 1])
                    {
                        moveStack.Push(-1);
                        directionStack.Push(true);
                        directionMap[a][b - 1]--;
                        if (directionMap[a][b - 1] == 1)
                        {
                            danger++;
                        }
                    }
                    if (map[a + 1][b])
                    {
                        moveStack.Push(1);
                        directionStack.Push(false);
                        directionMap[a + 1][b]--;
                        if (directionMap[a + 1][b] == 1)
                        {
                            danger++;
                        }
                    }
                    if (map[a][b + 1])
                    {
                        moveStack.Push(1);
                        directionStack.Push(true);
                        directionMap[a][b + 1]--;
                        if (directionMap[a][b + 1] == 1)
                        {
                            danger++;
                        }
                    }
                }
                else
                {

                }
            }
            if (prune)
            {

                while (moveStack.Count > 0)
                {
                    move = moveStack.Pop();
                    if (move == 0)
                    {
                        danger = restore.Pop();
                        wallCount = restore.Pop();
                        while (wall.Count != wallCount)
                        {
                            directionMap[wall.Pop()][wall.Pop()] = 0;
                        }
                        roadCount = restore.Pop();
                        if (directionStack.Pop())
                        {
                            while (roadx.Count != roadCount)
                            {
                                a = roadx.Pop();
                                b = roady.Pop();
                                map[a][b] = true;
                                if (map[a + 1][b])
                                {
                                    directionMap[a + 1][b]++;
                                }
                                if (map[a - 1][b])
                                {
                                    directionMap[a - 1][b]++;
                                }
                            }
                        }
                        else
                        {
                            while (roadx.Count != roadCount)
                            {
                                a = roadx.Pop();
                                b = roady.Pop();
                                map[a][b] = true;
                                if (map[a][b + 1])
                                {
                                    directionMap[a][b + 1]++;
                                }
                                if (map[a][b - 1])
                                {
                                    directionMap[a][b - 1]++;
                                }
                            }
                        }
                        a = roadx.Peek();
                        b = roady.Peek();
                    }
                    else
                    {
                        moveStack.Push(0);
                        restore.Push(roadx.Count);
                        restore.Push(wall.Count);
                        restore.Push(danger);
                        prune = true;
                        if (directionStack.Peek())
                        {
                            b += move;
                            if (directionMap[a][b] == 1)
                            {
                                danger--;
                            }
                            if (danger > 1)
                            {
                                prune = false;
                            }
                            while (prune && map[a][b + move])
                            {
                                map[a][b] = false;
                                roadx.Push(a);
                                roady.Push(b);
                                if (map[a + 1][b])
                                {
                                    directionMap[a + 1][b]--;
                                    if (directionMap[a + 1][b] == 1)
                                    {
                                        danger++;
                                        if (danger == 2)
                                        {
                                            prune = false;
                                        }
                                    }
                                    if (!map[a + 1][b + move])
                                    {
                                        if (directionMap[a + 1][b + move] == 0)
                                        {
                                            directionMap[a + 1][b + move] = -1;
                                            wall.Push(b + move);
                                            wall.Push(a + 1);
                                            connect.Push(b + move);
                                            connect.Push(a + 1);
                                            Connect(map, directionMap, wall, connect);
                                        }
                                        else
                                        {
                                            prune = false;
                                        }
                                    }
                                }
                                if (map[a - 1][b])
                                {
                                    directionMap[a - 1][b]--;
                                    if (directionMap[a - 1][b] == 1)
                                    {
                                        danger++;
                                        if (danger == 2)
                                        {
                                            prune = false;
                                        }
                                    }
                                    if (!map[a - 1][b + move])
                                    {
                                        if (directionMap[a - 1][b + move] == 0)
                                        {
                                            directionMap[a - 1][b + move] = -1;
                                            wall.Push(b + move);
                                            wall.Push(a - 1);
                                            connect.Push(b + move);
                                            connect.Push(a - 1);
                                            Connect(map, directionMap, wall, connect);
                                        }
                                        else
                                        {
                                            prune = false;
                                        }
                                    }
                                }
                                b += move;
                            }
                            if (prune)
                            {
                                if (map[a + 1][b])
                                {
                                    if (map[a - 1][b])
                                    {
                                        if (directionMap[a + 1][b] == 1 || directionMap[a - 1][b] == 1)
                                        {
                                            prune = false;
                                        }
                                        if (prune)
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            if (directionMap[a][b + move] == 0)
                                            {
                                                moveStack.Push(-1);
                                                directionStack.Push(false);
                                                moveStack.Push(1);
                                                directionStack.Push(false);
                                                if (b + move == 0 || b + move == R)
                                                {
                                                    connect.Push(b);
                                                    connect.Push(a);
                                                }
                                                else
                                                {
                                                    directionMap[a][b + move] = -1;
                                                    wall.Push(b + move);
                                                    wall.Push(a);
                                                    connect.Push(b + move);
                                                    connect.Push(a);
                                                }
                                                Connect(map, directionMap, wall, connect);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (directionMap[a + 1][b] == 1)
                                        {
                                            if (roadx.Count + 2 == restCount)
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                a++;
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            moveStack.Push(1);
                                            directionStack.Push(false);
                                            directionMap[a + 1][b]--;
                                            if (directionMap[a + 1][b] == 1)
                                            {
                                                danger++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (map[a - 1][b])
                                    {
                                        if (directionMap[a - 1][b] == 1)
                                        {
                                            if (roadx.Count + 2 == restCount)
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                a--;
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            moveStack.Push(-1);
                                            directionStack.Push(false);
                                            directionMap[a - 1][b]--;
                                            if (directionMap[a - 1][b] == 1)
                                            {
                                                danger++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (roadx.Count + 1 == restCount)
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            a += move;
                            if (directionMap[a][b] == 1)
                            {
                                danger--;
                            }
                            if (danger > 1)
                            {
                                prune = false;
                            }
                            while (prune && map[a + move][b])
                            {
                                map[a][b] = false;
                                roadx.Push(a);
                                roady.Push(b);
                                if (map[a][b + 1])
                                {
                                    directionMap[a][b + 1]--;
                                    if (directionMap[a][b + 1] == 1)
                                    {
                                        danger++;
                                        if (danger == 2)
                                        {
                                            prune = false;
                                        }
                                    }
                                    if (!map[a + move][b + 1])
                                    {
                                        if (directionMap[a + move][b + 1] == 0)
                                        {
                                            directionMap[a + move][b + 1] = -1;
                                            wall.Push(b + 1);
                                            wall.Push(a + move);
                                            connect.Push(b + 1);
                                            connect.Push(a + move);
                                            Connect(map, directionMap, wall, connect);
                                        }
                                        else
                                        {
                                            prune = false;
                                        }
                                    }
                                }
                                if (map[a][b - 1])
                                {
                                    directionMap[a][b - 1]--;
                                    if (directionMap[a][b - 1] == 1)
                                    {
                                        danger++;
                                        if (danger == 2)
                                        {
                                            prune = false;
                                        }
                                    }
                                    if (!map[a + move][b - 1])
                                    {
                                        if (directionMap[a + move][b - 1] == 0)
                                        {
                                            directionMap[a + move][b - 1] = -1;
                                            wall.Push(b - 1);
                                            wall.Push(a + move);
                                            connect.Push(b - 1);
                                            connect.Push(a + move);
                                            Connect(map, directionMap, wall, connect);
                                        }
                                        else
                                        {
                                            prune = false;
                                        }
                                    }
                                }
                                a += move;
                            }
                            if (prune)
                            {
                                if (map[a][b + 1])
                                {
                                    if (map[a][b - 1])
                                    {
                                        if (directionMap[a][b + 1] == 1 || directionMap[a][b - 1] == 1)
                                        {
                                            prune = false;
                                        }
                                        if (prune)
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            if (directionMap[a + move][b] == 0)
                                            {
                                                moveStack.Push(-1);
                                                directionStack.Push(true);
                                                moveStack.Push(1);
                                                directionStack.Push(true);
                                                if (a + move == 0 || a + move == D)
                                                {
                                                    connect.Push(b);
                                                    connect.Push(a);
                                                }
                                                else
                                                {
                                                    directionMap[a + move][b] = -1;
                                                    wall.Push(b);
                                                    wall.Push(a + move);
                                                    connect.Push(b);
                                                    connect.Push(a + move);
                                                }
                                                Connect(map, directionMap, wall, connect);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (directionMap[a][b + 1] == 1)
                                        {
                                            if (roadx.Count + 2 == restCount)
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                b++;
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            moveStack.Push(1);
                                            directionStack.Push(true);
                                            directionMap[a][b + 1]--;
                                            if (directionMap[a][b + 1] == 1)
                                            {
                                                danger++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (map[a][b - 1])
                                    {
                                        if (directionMap[a][b - 1] == 1)
                                        {
                                            if (roadx.Count + 2 == restCount)
                                            {
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                b--;
                                                map[a][b] = false;
                                                roadx.Push(a);
                                                roady.Push(b);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            moveStack.Push(-1);
                                            directionStack.Push(true);
                                            directionMap[a][b - 1]--;
                                            if (directionMap[a][b - 1] == 1)
                                            {
                                                danger++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (roadx.Count + 1 == restCount)
                                        {
                                            map[a][b] = false;
                                            roadx.Push(a);
                                            roady.Push(b);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            map[a][b] = true;
            roadx.Pop();
            roady.Pop();
            if (map[a - 1][b])
            {
                directionMap[a - 1][b]++;
            }
            if (map[a][b - 1])
            {
                directionMap[a][b - 1]++;
            }
            if (map[a + 1][b])
            {
                directionMap[a + 1][b]++;
            }
            if (map[a][b + 1])
            {
                directionMap[a][b + 1]++;
            }
            return false;
        }
    }
}
