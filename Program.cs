using System;
using algochan.API;
using algochan.OJ;
using algochan.Bot;
using algochan.Helpers;
using System.Net;
using System.IO;

namespace algochan
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ojManager = new OjManager();
            ojManager.AddJudge(new Codeforces());
            ojManager.InitializeJudges();

            var algochan = new Algochan("MzM1MTk2OTc4MDYzMzQzNjM3.DEmXoQ.OSjbxZksjk2CGSX7m3QUIsJkjIo", ojManager);
            algochan.Run().GetAwaiter().GetResult();
        }
    }
}