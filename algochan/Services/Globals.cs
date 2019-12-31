using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using cfapi;
using cfapi.Objects;

namespace algochan.Services
{
    public static class Globals
    {
        public static ProblemSet ProblemSet { get; set; }
        public static Dictionary<int, List<Problem>> ContestsProblemsList { get; set; }
        public static bool Initialized { get; set; }
    }
}
