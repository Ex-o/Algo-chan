using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using algochan.Services;
using cfapi.Objects;
using Discord;

namespace algochan.Helpers
{
    internal static class Utility
    {
        //private static readonly WebClient client = new WebClient();

        public static string DownloadString(string url)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                return HttpUtility.HtmlDecode(webClient.DownloadString(url));
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var convertedTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return convertedTime.AddSeconds(unixTime);
        }

        public static void Repeat(this Task myTask, CancellationToken cancellationToken, TimeSpan intervalTimeSpan)
        {
            var action = myTask
                .GetType()
                .GetField("m_action", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(myTask) as Action;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (cancellationToken.WaitHandle.WaitOne(intervalTimeSpan) ||
                        cancellationToken.IsCancellationRequested)
                        break;
                    Task.Factory.StartNew(action, cancellationToken);
                }
            }, cancellationToken);
        }

        public static EmbedBuilder BuildContest(Contest contest)
        {
            var duration = TimeSpan.FromSeconds(contest.Duration);
            var durationString = duration.ToString(@"hh\:mm\:ss");
            var embed = new EmbedBuilder();
            embed.ThumbnailUrl = "https://26.org.uk/wp-content/uploads/2016/06/Trophyicon_1198.png";
            embed.Color = Color.Blue;
            embed.Author = new EmbedAuthorBuilder
            {
                Name = contest.Name,
                IconUrl = "",
                Url = $"http://codeforces.com/contests/{contest.Id}"
            };
            embed.Footer = new EmbedFooterBuilder
            {
                Text = $"Duration: {durationString}",
                IconUrl = "https://d30y9cdsu7xlg0.cloudfront.net/png/273613-200.png"
            };
            embed.Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = "Contest Date:",
                    IsInline = true,
                    Value = $"{String.Format("{0:F}", FromUnixTime(contest.StartTime))}"
                }
            };
            return embed;
        }

        public static string RolePicker(int rating)
        {
            var role = "Newbie";

            if (rating < 1200)
                role = "Newbie";
            else if (rating >= 1200 && rating < 1400)
                role = "Pupil";
            else if (rating >= 1400 && rating < 1600)
                role = "Specialist";
            else if (rating >= 1600 && rating < 1900)
                role = "Expert";
            else if (rating >= 1900 && rating < 2100)
                role = "Candidate Master";
            else if (rating >= 2100 && rating < 2300)
                role = "Master";
            else if (rating >= 2300 && rating < 2400)
                role = "International Master";
            else if (rating >= 2400 && rating < 2600)
                role = "Grandmaster";
            else if (rating >= 2600 && rating < 3000)
                role = "International Grandmaster";
            else
                role = "Legendary Grandmaster";

            return role;
        }

        public static string IsValidCodeforcesProblemUrl(string url)
        {
            var data = url.Split('/').ToList();
            int contestId = -1;
            string problemIdx = "";
            data.RemoveAll(i => string.IsNullOrEmpty(i));
            if (data.Count < 3) return null;
            if(url.Contains(@"/contest/"))
            {
                int.TryParse(data[data.Count - 3], out contestId);
                problemIdx = data[data.Count - 1];
            }
            else if(url.Contains(@"/problemset/"))
            {
                int.TryParse(data[data.Count - 2], out contestId);
                problemIdx = data[data.Count - 1];
            }

            if(contestId != 0 && !String.IsNullOrEmpty(problemIdx))
            {
                if (Globals.ProblemSet.Problems.Count(p => p.ContestId == contestId && p.Index == problemIdx) != 0)
                {
                    return $"https://codeforces.com/contest/{contestId}/problem/{problemIdx}";
                }
                else return null;
            }
            return null;
        }
    }
}