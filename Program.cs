using algochan.OJ;
using algochan.Bot;
namespace algochan
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ojManager = new OjManager();
            ojManager.AddJudge(new Codeforces());
            ojManager.InitializeJudges();


            var algochan = new Algochan("bot_token", ojManager);
            algochan.Run().GetAwaiter().GetResult();
        }
    }
}