﻿using Common.Logging;
using SufeiUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace RoombaService
{
    class RoombaService : ServiceControl
    {
        private ILog logger;
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
        string cookie;

        public RoombaService()
        {
            logger = LogManager.GetLogger<RoombaService>();
            restPoints = new Stack<int>();
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["threadCount"], out threadCount))
            {
                threadCount = 2;
            }
            cookie = System.Configuration.ConfigurationManager.AppSettings["cookie"];
            locker = new object();
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
            logger.Info(sw.Elapsed);
            return result;
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
                        logger.DebugFormat("point x:{0},y:{1} start, {2} points rest, {3}", a, b, restPoints.Count / 2, DateTime.Now.ToString("HH:mm:ss"));
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

        bool Clean(bool[][] map, int[][] directionMap, int danger, int a, int b, Stack<int> roadx, Stack<int> roady)
        {
            Stack<int> moveStack = new Stack<int>();
            Stack<bool> directionStack = new Stack<bool>();
            Stack<int> restore = new Stack<int>();
            Stack<int> horizontalConnect = new Stack<int>(), verticalConnect = new Stack<int>();
            int roadCount;
            int move;
            bool prune;
            if (directionMap[a][b] == 1)
            {
                danger--;
            }
            map[a][b] = false;
            roadx.Push(a);
            roady.Push(b);
            if (map[a - 1][b])
            {
                moveStack.Push(-1);
                directionStack.Push(false);
                directionMap[a - 1][b]--;
                if (directionMap[a - 1][b] < 2)
                {
                    danger++;
                }
            }
            if (map[a][b - 1])
            {
                moveStack.Push(-1);
                directionStack.Push(true);
                directionMap[a][b - 1]--;
                if (directionMap[a][b - 1] < 2)
                {
                    danger++;
                }
            }
            if (map[a + 1][b])
            {
                moveStack.Push(1);
                directionStack.Push(false);
                directionMap[a + 1][b]--;
                if (directionMap[a + 1][b] < 2)
                {
                    danger++;
                }
            }
            if (map[a][b + 1])
            {
                moveStack.Push(1);
                directionStack.Push(true);
                directionMap[a][b + 1]--;
                if (directionMap[a][b + 1] < 2)
                {
                    danger++;
                }
            }
            while (moveStack.Count > 0)
            {
                move = moveStack.Pop();
                if (move == 0)
                {
                    danger = restore.Pop();
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
                                            do
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
                                            while (horizontalConnect.Count > 0);
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
                                            do
                                            {
                                                map[roadx.Pop()][roady.Pop()] = true;
                                            }
                                            while (roadx.Count != roadCount);
                                            moveStack.Push(1);
                                            directionStack.Push(false);
                                            moveStack.Push(-1);
                                            directionStack.Push(false);
                                            a = roadx.Peek();
                                            b = roady.Peek();
                                            directionMap[a + 1][b]--;
                                            directionMap[a - 1][b]--;
                                            if (directionMap[a + 1][b] == 1)
                                            {
                                                danger++;
                                            }
                                            if (directionMap[a - 1][b] == 1)
                                            {
                                                danger++;
                                            }
                                        }
                                        else
                                        {
                                            do
                                            {
                                                map[roadx.Pop()][roady.Pop()] = true;
                                            }
                                            while (roadx.Count != roadCount);
                                            map[roadx.Pop()][roady.Pop()] = true;
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
                                            do
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
                                            while (verticalConnect.Count > 0);
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
                                            do
                                            {
                                                map[roadx.Pop()][roady.Pop()] = true;
                                            }
                                            while (roadx.Count != roadCount);
                                            moveStack.Push(1);
                                            directionStack.Push(true);
                                            moveStack.Push(-1);
                                            directionStack.Push(true);
                                            a = roadx.Peek();
                                            b = roady.Peek();
                                            directionMap[a][b + 1]--;
                                            directionMap[a][b - 1]--;
                                            if (directionMap[a][b + 1] == 1)
                                            {
                                                danger++;
                                            }
                                            if (directionMap[a][b - 1] == 1)
                                            {
                                                danger++;
                                            }
                                        }
                                        else
                                        {
                                            do
                                            {
                                                map[roadx.Pop()][roady.Pop()] = true;
                                            }
                                            while (roadx.Count != roadCount);
                                            map[roadx.Pop()][roady.Pop()] = true;
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

        public virtual void Start()
        {
            try
            {
                Task task = Task.Factory.StartNew(() =>
                {
                    HttpHelper hh = new HttpHelper();
                    HttpItem hi = new HttpItem()
                    {
                        Cookie = cookie
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
                        logger.InfoFormat("level:{0},x:{1},y{2} start, {3}", level, x, y, DateTime.Now.ToString());
                        hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", Clean());
                        logger.InfoFormat("level:{0} end, {1}", level, DateTime.Now.ToString());
                        hr = hh.GetHtml(hi);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Fatal("Roomba start failed.", ex);
            }
        }

        public bool Start(HostControl hostControl)
        {
            Start();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            return true;
        }
    }
}
