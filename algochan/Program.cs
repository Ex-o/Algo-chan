
using cfapi.Objects;
using algochan.OJ;
using algochan.Bot;
using algochan.Helpers;
using System.Net;
using System.IO;
using System;
using System.Collections;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using cfapi.Methods;
using cfapi;
using algochan.Services;

namespace algochan
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ojManager = new OjManager();
            ojManager.AddJudge(new Codeforces());
            ojManager.InitializeJudges();

            var algochan = new Algochan("token", ojManager);
            algochan.Run().GetAwaiter().GetResult();
        }
    }
}