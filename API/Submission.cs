namespace algochan.API
{
    public class Submission
    {
        public int id { get; set; }
        public int contestId { get; set; }
        public int creationTimeSeconds { get; set; }
        public int relatimeTimeSeconds { get; set; }
        public Problem problem { get; set; }
    }
}