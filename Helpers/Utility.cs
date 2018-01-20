using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using algochan.API;
using Discord;

namespace algochan.Helpers
{
    internal static class Utility
    {
        private static readonly WebClient client = new WebClient();

        public static string DownloadString(string url)
        {
            try
            {
                return client.DownloadString(url);
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
            var duration = TimeSpan.FromSeconds(contest.durationSeconds);
            var durationString = duration.ToString(@"hh\:mm\:ss");
            var embed = new EmbedBuilder();
            embed.ThumbnailUrl = "https://26.org.uk/wp-content/uploads/2016/06/Trophyicon_1198.png";
            embed.Color = Color.Blue;
            embed.Author = new EmbedAuthorBuilder
            {
                Name = contest.name,
                IconUrl = "",
                Url = $"http://codeforces.com/contests/{contest.id}"
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
                    Value = $"{String.Format("{0:F}", FromUnixTime(contest.startTimeSeconds))}"
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
            else if (rating >= 1900 && rating < 2200)
                role = "Candidate Master";
            else if (rating >= 2200 && rating < 2300)
                role = "Master";
            else if (rating >= 2300 && rating < 2400)
                role = "International Master";
            else if (rating >= 2400 && rating < 2600)
                role = "Grandmaster";
            else if (rating >= 2600 && rating < 2900)
                role = "International Grandmaster";
            else
                role = "Legendary Grandmaster";

            return role;
        }
    }
}