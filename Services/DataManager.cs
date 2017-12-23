using System.Data.SQLite;
using algochan.Helpers;

namespace algochan.Services
{
    public class DataManager
    {
        private readonly SQLiteConnection _dbConnection = new SQLiteConnection(
            @"Data Source=C:\Users\kmepv\Documents\Visual Studio 2015\Projects\algochan\algochan\bin\Debug\algochan.db;Version=3;");

        private SQLiteDataReader Query(string qString)
        {
            var cmd = new SQLiteCommand(qString, _dbConnection);
            return cmd.ExecuteReader();
        }

        public void Initialize()
        {
            _dbConnection.Open();
        }

        public bool UserExists(string discordInfo)
        {
            return Query($@"SELECT * FROM users WHERE discordInfo = ""{discordInfo}""").HasRows;
        }

        public void AddUser(string discordInfo, string serliazedData)
        {
            Query($@"INSERT INTO users (discordInfo, serializedData) values (""{discordInfo}"", ""{
                    EncryptionHelper.Encrypt(serliazedData)
                }"")");
        }

        public void UpdateUser(string discordInfo, string serliazedData)
        {
            Query(
                $@"UPDATE users SET serializedData = ""{
                        EncryptionHelper.Encrypt(serliazedData)
                    }"" WHERE discordInfo = ""{discordInfo}""");
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