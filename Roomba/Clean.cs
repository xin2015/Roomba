using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    public class Clean
    {
        private static int x;
        private static int y;
        private static int X;
        private static int Y;
        private static bool[,] map;
        private static int rest;
        private static bool done;
        private static Stack<char> path;
        private static int a;
        private static int b;

        static Clean()
        {
            path = new Stack<char>();
        }

        public static string DoMultithreading(int _x, int _y, string mapStr)
        {
            x = _x;
            y = _y;
            X = x + 2;
            Y = y + 2;
            #region 初始化地图
            char[] mapArray = mapStr.ToArray();
            map = new bool[X, Y];
            rest = 0;
            for (int j = 0; j < Y; j++)
            {
                map[0, j] = false;
                map[x + 1, j] = false;
            }
            for (int i = x; i > 0; i--)
            {
                map[i, 0] = false;
                map[i, y + 1] = false;
            }
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; )
                {
                    if (mapArray[i * y + j] == '0')
                    {
                        map[i + 1, ++j] = true;
                        rest++;
                    }
                    else
                    {
                        map[i + 1, ++j] = false;
                    }
                }
            }
            #endregion
            #region 解题
            List<Task> taskList = new List<Task>();
            done = false;
            path.Clear();
            rest--;
            for (int i = x; i > 0; i--)
            {
                for (int j = y; j > 0; j--)
                {
                    if (map[i, j])
                    {
                        taskList.Add(Task.Factory.StartNew(DoSync, new int[] { i, j }));
                    }
                }
            }
            Task.WaitAll(taskList.ToArray());
            #endregion
            string result = string.Format("x={0}&y={1}&path=", a, b);
            foreach (char item in path.ToArray().Reverse())
            {
                result += item;
            }
            return result;
        }

        public static string Do(int _x, int _y, string mapStr)
        {
            x = _x;
            y = _y;
            X = x + 2;
            Y = y + 2;
            #region 初始化地图
            char[] mapArray = mapStr.ToArray();
            map = new bool[X, Y];
            rest = 0;
            for (int j = 0; j < Y; j++)
            {
                map[0, j] = false;
                map[x + 1, j] = false;
            }
            for (int i = x; i > 0; i--)
            {
                map[i, 0] = false;
                map[i, y + 1] = false;
            }
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; )
                {
                    if (mapArray[i * y + j] == '0')
                    {
                        map[i + 1, ++j] = true;
                        rest++;
                    }
                    else
                    {
                        map[i + 1, ++j] = false;
                    }
                }
            }
            #endregion
            #region 解题
            done = false;
            path.Clear();
            rest--;
            for (int i = 15; i > 0; i--)
            {
                if (done)
                {
                    break;
                }
                for (int j = 12; j > 0; j--)
                {
                    if (map[i, j])
                    {
                        map[i, j] = false;
                        if ((map[i - 1, j] && Test(map, rest, i, j, 'u', path)) || (map[i + 1, j] && Test(map, rest, i, j, 'd', path)) || (map[i, j - 1] && Test(map, rest, i, j, 'l', path)) || (map[i, j + 1] && Test(map, rest, i, j, 'r', path)))
                        {
                            done = true;
                            a = i;
                            b = j;
                            break;
                        }
                        else
                        {
                            map[i, j] = true;
                        }
                    }
                }
            }
            #endregion
            string result = string.Format("x={0}&y={1}&path=", a, b);
            foreach (char item in path)
            {
                result += item;
            }
            return result;
        }

        private static void DoSync(object state)
        {
            if (!done)
            {
                bool[,] copyMap = CopyMap();
                int[] point = state as int[];
                copyMap[point[0], point[1]] = false;
                Stack<char> tempPath = new Stack<char>();
                if ((copyMap[point[0] - 1, point[1]] && Test(copyMap, rest, point[0], point[1], 'u', tempPath)) || (copyMap[point[0] + 1, point[1]] && Test(copyMap, rest, point[0], point[1], 'd', tempPath)) || (copyMap[point[0], point[1] - 1] && Test(copyMap, rest, point[0], point[1], 'l', tempPath)) || (copyMap[point[0], point[1] + 1] && Test(copyMap, rest, point[0], point[1], 'r', tempPath)))
                {
                    done = true;
                    a = point[0];
                    b = point[1];
                    lock (path)
                    {
                        path = tempPath;
                    }
                }
            }
        }

        private static bool[,] CopyMap()
        {
            bool[,] copyMap = new bool[X, Y];
            CopyMap(map, copyMap);
            return copyMap;
        }

        private static void CopyMap(bool[,] source, bool[,] target)
        {
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    target[i, j] = source[i, j];
                }
            }
        }

        public static bool Test(bool[,] map, int rest, int a, int b, char direction, Stack<char> path)
        {
            bool result = false;
            Stack<char> tempPath = new Stack<char>();
            Stack<int[]> road = new Stack<int[]>();
            int[] point;
            Go(map, ref rest, ref a, ref b, ref direction, tempPath, road);
            if (rest == 0)
            {
                result = true;
            }
            else
            {
                switch (direction)
                {
                    case 'u':
                    case 'd':
                        {
                            if (map[a, b - 1] && map[a, b + 1])
                            {
                                Stack<int[]> list = new Stack<int[]>();
                                Connect(map, a, b - 1, list);
                                int count = list.Count;
                                while (list.Any())
                                {
                                    point = list.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                if (count == rest)
                                {
                                    if (Test(map, rest, a, b, 'l', path) || Test(map, rest, a, b, 'r', path))
                                    {
                                        result = true;
                                    }
                                }
                            }
                            break;
                        }
                    case 'l':
                    case 'r':
                        {
                            if (map[a - 1, b] && map[a + 1, b])
                            {
                                Stack<int[]> list = new Stack<int[]>();
                                Connect(map, a - 1, b, list);
                                int count = list.Count;
                                while (list.Any())
                                {
                                    point = list.Pop();
                                    map[point[0], point[1]] = true;
                                }
                                if (count == rest)
                                {
                                    if (Test(map, rest, a, b, 'u', path) || Test(map, rest, a, b, 'd', path))
                                    {
                                        result = true;
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            if (result)
            {
                while (tempPath.Any())
                {
                    path.Push(tempPath.Pop());
                }
            }
            else
            {
                while (road.Any())
                {
                    point = road.Pop();
                    map[point[0], point[1]] = true;
                }
            }
            return result;
        }

        public static void Go(bool[,] map, ref int rest, ref int a, ref int b, ref char direction, Stack<char> path, Stack<int[]> road)
        {
            path.Push(direction);
            switch (direction)
            {
                case 'u':
                    {
                        while (map[a - 1, b])
                        {
                            map[--a, b] = false;
                            road.Push(new int[] { a, b });
                            rest--;
                        }
                        if (map[a, b + 1])
                        {
                            if (!map[a, b - 1])
                            {
                                direction = 'r';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        else
                        {
                            if (map[a, b - 1])
                            {
                                direction = 'l';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        break;
                    }
                case 'r':
                    {
                        while (map[a, b + 1])
                        {
                            map[a, ++b] = false;
                            road.Push(new int[] { a, b });
                            rest--;
                        }
                        if (map[a - 1, b])
                        {
                            if (!map[a + 1, b])
                            {
                                direction = 'u';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        else
                        {
                            if (map[a + 1, b])
                            {
                                direction = 'd';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        break;
                    }
                case 'd':
                    {
                        while (map[a + 1, b])
                        {
                            map[++a, b] = false;
                            road.Push(new int[] { a, b });
                            rest--;
                        }
                        if (map[a, b + 1])
                        {
                            if (!map[a, b - 1])
                            {
                                direction = 'r';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        else
                        {
                            if (map[a, b - 1])
                            {
                                direction = 'l';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        break;
                    }
                case 'l':
                    {
                        while (map[a, b - 1])
                        {
                            map[a, --b] = false;
                            road.Push(new int[] { a, b });
                            rest--;
                        }
                        if (map[a - 1, b])
                        {
                            if (!map[a + 1, b])
                            {
                                direction = 'u';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        else
                        {
                            if (map[a + 1, b])
                            {
                                direction = 'd';
                                Go(map, ref rest, ref a, ref b, ref direction, path, road);
                            }
                        }
                        break;
                    }
            }
        }

        public static void Connect(bool[,] map, int a, int b, Stack<int[]> list)
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
}
