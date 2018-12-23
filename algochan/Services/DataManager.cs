using System.Data.SQLite;
using System.Runtime.Remoting.Channels;
using algochan.Helpers;

namespace algochan.Services
{
    public class DataManager
    {
        private const string SERVER =
            @"Data Source=C:\Users\algochan\Desktop\algochan\algochan.db;Version=3;";

        private const string LOCAL =
            @"Data Source=C:\Users\Desktop\algo\algochan.db;Version=3;";
        private readonly SQLiteConnection _dbConnection = new SQLiteConnection(SERVER);

        public SQLiteDataReader Query(string qString)
        {
            var cmd = new SQLiteCommand(qString, _dbConnection);
            return cmd.ExecuteReader();
        }

        public void Initialize()
        {
            _dbConnection.Open();
        }

        public bool ProblemExists(ulong discordId, string problemUrl)
        {
            return Query($@"SELECT * FROM favorite WHERE discordId = ""{discordId}"" AND problem = ""{problemUrl}""").HasRows;
        }

        public void AddFavoriteProblem(ulong discordId, string problemUrl)
        {
            Query($@"INSERT into favorite (discordId, problem) values (""{discordId}"", ""{problemUrl}"")");
        }
        public bool UserExists(ulong discordId)
        {
            return Query($@"SELECT * FROM users WHERE discordInfo = ""{discordId}""").HasRows;
        }

        public void AddUser(ulong discordId, string serliazedData)
        {
            Query($@"INSERT INTO users (discordInfo, serializedData) values (""{discordId}"", ""{
                    EncryptionHelper.Encrypt(serliazedData)
                }"")");
        }

        public void UpdateUser(ulong discordId, string serliazedData)
        {
            Query(
                $@"UPDATE users SET serializedData = ""{
                        EncryptionHelper.Encrypt(serliazedData)
                    }"" WHERE discordInfo = ""{discordId}""");
        }

        public SQLiteDataReader GetAllFavorites(ulong discordId)
        {
            return Query($@"SELECT * FROM favorite WHERE discordId = ""{discordId}""");
        }
        public SQLiteDataReader GetAllUsers()
        {
            return Query($@"SELECT * FROM users");
        }

        internal void AddSubscribe(ulong id)
        {
            try
            {
                Query($"INSERT INTO subscribers(discordId) values({id})");
            }
            catch
            {
            }
        }

        public SQLiteDataReader GetSubscribers()
        {
            return Query("SELECT * FROM subscribers");
        }

        internal void RemoveSubscribe(ulong id)
        {
            try
            {
                Query($"DELETE FROM subscribers where discordId = {id}");
            }
            catch
            {
            }
        }
    }
}