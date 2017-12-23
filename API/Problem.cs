using System.Collections.Generic;

namespace algochan.API
{
    public class Problem
    {
        public int contest { get; set; }
        public string index { get; set; }
        public string name { get; set; }
        public ProblemType type { get; set; }
        public float points { get; set; }
        public List<string> tags { get; set; }
    }

    public enum ProblemType
    {
        PROGRAMMING,
        QUESTION
    }
}