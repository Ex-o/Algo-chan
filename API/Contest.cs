using System;

namespace algochan.API
{
    public class Contest
    {
        public int id { get; set; }
        public string name { get; set; }
        public ContestType type { get; set; }
        public ContestPhase phase { get; set; }
        public bool frozen { get; set; }
        public int durationSeconds { get; set; }
        public int startTimeSeconds { get; set; }
        public int relativeTimeSeconds { get; set; }
        public DateTime time { get; set; }
    }

    public enum ContestType
    {
        CF,
        IOI,
        ICPC
    }

    public enum ContestPhase
    {
        BEFORE,
        CODING,
        PENDING_SYSTEM_TEST,
        SYSTEM_TEST,
        FINISHED
    }
}