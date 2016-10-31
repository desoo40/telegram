using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using HockeyBot.Configs;
using System.Globalization;

namespace HockeyBot
{
    class DBCore
    {
        string DBFile => Config.DBFile;
        readonly string SQLForCreateon = @"DB/SQLDBCreate.sql";

        SqliteConnection conn;

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
                conn = new SqliteConnection($"Data Source={DBFile}; Version=3;");
                conn.Open();

                SqliteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA foreign_keys = 1";
                cmd.ExecuteNonQuery();

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Disconnect()
        {
            conn.Close();
            conn.Dispose();
        }

        public void CreateDefaultDB()
        {
            string sql = File.ReadAllText(SQLForCreateon);

            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Player GetPlayerByNumber(int number)
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM player WHERE number = " + number;

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (reader.Read())
            {
                var player = new Player(Convert.ToInt32(reader["number"].ToString()), 
                    reader["name"].ToString(),
                    reader["lastname"].ToString());
                player.Id = Convert.ToInt32(reader["id"].ToString());
                return player;
            }
            return null;
        }

        public Player GetPlayerById(int id)
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM player WHERE id = " + id;

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (reader.Read())
            {
                var player = new Player(Convert.ToInt32(reader["number"].ToString()),
                    reader["name"].ToString(),
                    reader["lastname"].ToString());
                player.Id = Convert.ToInt32(reader["id"].ToString());
                return player;
            }
            return null;
        }

        public List<Player> GetAllPlayerWitoutStatistic()
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM player";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            var players = new List<Player>();
            while (reader.Read())
            {
                int number = Convert.ToInt32(reader["number"].ToString());
                string name = reader["name"].ToString();
                string lastname = reader["lastname"].ToString();

                var player = new Player(number, name, lastname);
                player.Id = Convert.ToInt32(reader["id"].ToString());
                player.Position = reader["position"].ToString();
                players.Add(player);
            }
            return players;
        }

        public Player GetPlayerStatistic(Player player)
        {                       
            return player;
        }

        public Player GetPlayerStatisticByNumber(int number)
        {
            var player = GetPlayerByNumber(number);
            return GetPlayerStatistic(player);
        }       

        public List<HockeyBot.Event> GetEventsByType(string type)
        {
            var events = new List<HockeyBot.Event>();

            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM event WHERE type = '{type}'";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (reader.Read())
            {
                var even = new HockeyBot.Event();
                even.Id = int.Parse(reader["id"].ToString());
                even.Type = reader["type"].ToString();
                even.Date = reader["date"].ToString();
                even.Time = reader["time"].ToString();
                even.Place = reader["place"].ToString();
                even.Address = reader["address"].ToString();
                even.Details = reader["details"].ToString();
                even.Members = reader["members"].ToString();

                events.Add(even);
            }

            return events;
        }

        public List<Player> GetPlayersByNameOrSurname(string nameOrSurname)
        {
            var players = new List<Player>();

            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText =$"SELECT * FROM player WHERE lastname_lower = '{nameOrSurname.ToLower()}' OR name = '{nameOrSurname}'";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (reader.Read())
            {
                var player = new Player(
                    Convert.ToInt32(reader["number"].ToString()), 
                    reader["name"].ToString(),
                    reader["lastname"].ToString());
                player.Id = Convert.ToInt32(reader["id"].ToString());
                players.Add(player);
            }

            return players;
        }

        public void AddPlayer(Player player)
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO player (number, name, lastname) VALUES({0}, '{1}', '{2}')",
                player.Number, player.Name, player.Surname);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void RemovePlayerByNumber(int number)
        {
            SqliteCommand cmd = conn.CreateCommand();
            var player = GetPlayerByNumber(number);
            if (player == null) return;

            cmd.CommandText = string.Format("DELETE from player where number={0}", number);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Initialization()
        {
            Console.WriteLine("Start Initialization");
            File.Delete(Config.DBFile);
            DBCore db = new DBCore();

            Console.WriteLine("CreateDB");
            db.CreateDefaultDB();

            Console.WriteLine("FillPlayersFromFile");
            db.LoadPlayersFromFile();

            Console.WriteLine("FillEventsFromFile");
            db.LoadEventsFromFile();
                        
            db.Disconnect();
            Console.WriteLine("Finish Initialization");
        }

        #region Import

        public void LoadPlayersFromFile()
        {
            var players = File.ReadAllLines(Config.DBPlayersInfoPath);

            foreach (var player in players)
            {
                var playerinfo = player.Split(';');

                SqliteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("INSERT INTO player (number, name, lastname, lastname_lower) VALUES({0}, '{1}', '{2}', '{3}')",
                    playerinfo[0].Trim(), playerinfo[2].Trim(), playerinfo[1].Trim(), playerinfo[1].Trim().ToLower());

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        struct Event
        {
            public string type;
            public string date;
            public string time;
            public string place;
            public string address;
            public string details;
            public string be;
            public string maybe;
            public string notbe;
        }

        public void LoadEventsFromFile()
        {

            var events = File.ReadAllLines(Config.DBEventsInfoPath);

            foreach (var even in events)
            {
                var fields = even.Split(';');
                //type=Игра;date=30 октября;time=11:00;place=Янтарь;address=г.Москва, ул.Маршала Катукова, д.26;details=Сезон 2016-2017 дивизион КБЧ-Восток%Янтарь-2 Wild Woodpeckers%Будут:1 Возможно:1 Не будут:1;be=Игорь Смирнов;maybe=Латохин Дмитрий;notbe=Скалин Петр
                var ev = new Event();
                ev.type = "";
                ev.date = "";
                ev.time = "";
                ev.place = "";
                ev.address = "";
                ev.details = "";
                ev.be = "*Будут:*\n";
                ev.maybe = "\n*Возможно:*\n";
                ev.notbe = "\n*Не будут:*\n";
                foreach (var field in fields)
                {
                    var keyvalue = field.Split('=');
                    if (keyvalue[0] == "type") ev.type = keyvalue[1];
                    if (keyvalue[0] == "date") ev.date = keyvalue[1];
                    if (keyvalue[0] == "time") ev.time = keyvalue[1];
                    if (keyvalue[0] == "place") ev.place = keyvalue[1];
                    if (keyvalue[0] == "address") ev.address = keyvalue[1];
                    if (keyvalue[0] == "details") ev.details = keyvalue[1];
                    if (keyvalue[0] == "be") ev.be += keyvalue[1] + "\n";
                    if (keyvalue[0] == "maybe") ev.maybe += keyvalue[1] + "\n";
                    if (keyvalue[0] == "notbe") ev.notbe += keyvalue[1] + "\n";
                }

                ev.details = ev.details.Replace('%', '\n');

                if (ev.be == "*Будут:*\n") ev.be = "";
                if (ev.maybe == "\n*Возможно:*\n") ev.maybe = "";
                if (ev.notbe == "\n*Не будут:*\n") ev.notbe = "";

                SqliteCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"INSERT INTO event (type, date, time, place, address, details, members) VALUES('{ev.type}', '{ev.date}', '{ev.time}', '{ev.place}', '{ev.address}', '{ev.details}', '{ev.be + ev.maybe + ev.notbe}')";                

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            
        }
        #endregion

    }
}
