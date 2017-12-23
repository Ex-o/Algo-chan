using System.Collections.Generic;

namespace algochan.API
{
    public class Party
    {
        public int contestId { get; set; }
        public List<Member> members { get; set; }
        public ParticipantType participantType { get; set; }
        public int teamId { get; set; }
        public string teamName { get; set; }
        public bool ghost { get; set; }
        public int room { get; set; }
        public int startTimeSeconds { get; set; }
    }

    public enum ParticipantType
    {
        CONTESTANT,
        PRACTICE,
        VIRTUAL,
        MANAGER,
        OUT_OF_COMPETITION
    }
}