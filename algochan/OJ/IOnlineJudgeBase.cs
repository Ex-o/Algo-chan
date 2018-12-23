namespace algochan.OJ
{
    public interface IOnlineJudgeBase
    {
        string Name { get; }
        bool IsInitialized { get; }
        void ParseContests();

        void ReloadContests();

        bool IsOnline(string api = "");
    }
}