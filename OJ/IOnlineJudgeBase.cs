namespace algochan.OJ
{
    public interface IOnlineJudgeBase
    {
        string Name { get; }
        void ParseContests(string api = "");
        bool IsOnline(string api = "");
    }
}