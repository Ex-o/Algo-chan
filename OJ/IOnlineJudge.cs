namespace algochan.OJ
{
    public interface IOnlineJudge
    {
        void ParseContests(string api = "");
        bool IsOnline(string api = "");
    }
}