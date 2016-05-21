using DotNet4.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomba
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter sw = File.CreateText("log.txt");
            sw.Dispose();
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
                        sw = new StreamWriter("log.txt");
                        sw.WriteLine("level:{0} start,{1}", level, DateTime.Now.ToShortTimeString());
                        sw.Dispose();
                        hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", Clean.DoMultithreading(x, y, mapStr));
                        sw = new StreamWriter("log.txt");
                        sw.WriteLine("level:{0} end,{1}", level, DateTime.Now.ToShortTimeString());
                        sw.Dispose();
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
                        sw = new StreamWriter("log.txt");
                        sw.WriteLine("level:{0} start,{1}", level, DateTime.Now.ToShortTimeString());
                        sw.Dispose();
                        hi.URL = string.Format("http://www.qlcoder.com/train/crcheck?{0}", Clean.Do(x, y, mapStr));
                        sw = new StreamWriter("log.txt");
                        sw.WriteLine("level:{0} end,{1}", level, DateTime.Now.ToShortTimeString());
                        sw.Dispose();
                        hr = hh.GetHtml(hi);
                    }
                }
            }
            catch (Exception e)
            {
                sw = new StreamWriter("log.txt");
                sw.WriteLine(e.Message);
            }
            sw.Close();
            sw.Dispose();
        }
    }
}
