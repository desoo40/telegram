using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    class DBCore
    {
        private readonly string SQLForCreateon = "data_base\\SQLDBCreate.sql";
        private readonly string DBFile = "data_base\\database.db";

        SQLiteConnection conn;

        /// <summary>
        /// При создании класса, сразу подключаем.(если базы нет, он ее создаст)
        /// </summary>
        public DBCore()
        {
            Connect();
        }

        public void Connect()
        {
            try
            {
                conn = new SQLiteConnection($"Data Source={DBFile}; Version=3;");
                conn.Open();

                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA foreign_keys = 1";
                cmd.ExecuteNonQuery();
                
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void CreateDefaultDB()
        {
            string sql = File.ReadAllText(SQLForCreateon);

            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void LoadPlayersFromFile()
        {
            var players = File.ReadAllLines("data_base\\PlayersInfo.txt");

            foreach (var player in players)
            {
                var playerinfo = player.Split(';');

                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("INSERT INTO player (number, name, lastname) VALUES({0}, '{1}', '{2}')",
                    playerinfo[0], playerinfo[2], playerinfo[1]);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public Player GetPlayerByNumber(int number)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM player WHERE number = "+ number;

            SQLiteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (reader.Read())
            {
                return new Player(number, reader["name"].ToString(), reader["lastname"].ToString());
            }
            return null;
        }

        public void Disconnect()
        {
           conn.Close();
        }

        public void LoadTeamsFromFile()
        {
            throw new NotImplementedException();
        }
    }
}
