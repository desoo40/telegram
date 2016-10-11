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
        SQLiteConnection conn = new SQLiteConnection("Data Source=data_base\\database.db; Version=3;");


        public void Connect()
        {
            try
            {
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

        public void CreateTables()
        {
            string sql =
@"CREATE TABLE team (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    logo TEXT NULL    
); 
CREATE TABLE player(
    id INTEGER PRIMARY KEY,
    number INTEGER NOT NULL,
    name TEXT NOT NULL,
    lastname TEXT NOT NULL,
    photo TEXT NULL,
team_id integer null,
FOREIGN KEY(team_id) REFERENCES team(id)
);

            ";

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
                    playerinfo[0], playerinfo[1], playerinfo[2]);

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

    }
}
