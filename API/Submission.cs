namespace algochan.API
{
    public class Submission
    {
        public int id { get; set; }
        public int contestId { get; set; }
        public int creationTimeSeconds { get; set; }
        public int relatimeTimeSeconds { get; set; }
        public Problem problem { get; set; }

        public Party author { get; set; }
        public string programmingLanguage { get; set; }
        public Verdict verdict { get; set; }
    }

    public enum Verdict
    {
        FAILED, OK, PARTIAL, COMPILATION_ERROR, RUNTIME_ERROR, WRONG_ANSWER, PRESENTATION_ERROR, TIME_LIMIT_EXCEEDED, MEMORY_LIMIT_EXCEEDED, IDLENESS_LIMIT_EXCEEDED, SECURITY_VIOLATED, CRASHED, INPUT_PREPARATION_CRASHED, CHALLENGED, SKIPPED, TESTING, REJECTED
    }
}