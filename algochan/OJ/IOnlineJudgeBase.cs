namespace algochan.OJ
{
    public interface IOnlineJudgeBase
    {
        string Name { get; }
        void ParseContests();
        bool IsOnline(string api = "");
    }
}