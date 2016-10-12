﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aviators
{
    class DBCore
    {
        string DBFile => Program.DBFile;
        readonly string SQLForCreateon = @"data_base\SQLDBCreate.sql";

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



        public Player GetPlayerByNumber(int number)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM player WHERE number = " + number;

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
                var player = new Player(Convert.ToInt32(reader["number"].ToString()), 
                    reader["name"].ToString(),
                    reader["lastname"].ToString());
                player.Id = Convert.ToInt32(reader["id"].ToString());
                return player;
            }
            return null;
        }

        public void Disconnect()
        {
            conn.Close();
            conn.Dispose();
        }

        public void LoadPlayersFromFile()
        {
            var players = File.ReadAllLines("data_base\\PlayersInfo.txt");

            foreach (var player in players)
            {
                var playerinfo = player.Split(';');

                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("INSERT INTO player (number, name, lastname) VALUES({0}, '{1}', '{2}')",
                    playerinfo[0].Trim(), playerinfo[2].Trim(), playerinfo[1].Trim());

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

        public void LoadTeamsFromFile()
        {
            var teams = File.ReadAllText("data_base\\TeamsInfo.txt");

            Match m = Regex.Match(teams, "(?<name>.*)\\((?<town>.*)\\)");
            while (m.Success)
            {
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("INSERT INTO team (name, town) VALUES('{0}', '{1}')",
                    m.Groups["name"].ToString().Trim(), m.Groups["town"].ToString().Trim());

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                m = m.NextMatch();
            }
        }

        public void LoadGamesFromFile()
        {
            var teams = File.ReadAllText("data_base\\GamesInfo.txt");
            teams = teams.Replace("\r", "").Replace("\n", "");
            var games = teams.Split(new[] {"---"}, StringSplitOptions.RemoveEmptyEntries);

            var players = GetAllPlayerWitoutStatistic();

            var season = games[0];
            for (int i = 1; i < games.Length; i++)
            {
                var game = games[i];

                var gameinfo = game.Split(';');

                Game newgame = new Game();
                newgame.Date = DateTime.Parse(gameinfo[0]);
                newgame.Tournament = gameinfo[1];
                newgame.Team2 = gameinfo[2];
                var score = gameinfo[3].Split(':');
                newgame.Score = new Tuple<int, int>(Convert.ToInt32(score[0]), Convert.ToInt32(score[1]));

                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("INSERT INTO game (date, opteam_id, opteamscore,tournament_id) " +
                                                "VALUES('{0}',(select ID from team where lower(name) = '{1}' )," +
                                                " {2}, (select ID from tournament where lower(name) = '{3}'))",
                    newgame.Date, newgame.Team2, newgame.Score.Item2, newgame.Tournament);

                try
                {
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"select last_insert_rowid()";
                    newgame.Id = Convert.ToInt32((long) cmd.ExecuteScalar());
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }


                var goals = gameinfo[4].Split(':')[1];
                var playergoal = goals.Split(',');
                foreach (var pg in playergoal)
                {
                    string name;
                    var num = 1;

                    Regex re = new Regex(@"(?<name>.*)\((?<num>\d+)\)");
                    if (re.IsMatch(pg))
                    {
                        var m = re.Match(pg);
                        name = m.Groups["name"].ToString().Trim();
                        num = Convert.ToInt32(m.Groups["num"].ToString());
                    }
                    else
                    {
                        name = pg.Trim();
                    }

                    var player = players.Find(p => p.Surname == name);
                    if (player == null) continue;

                    for (int j = 0; j < num; j++)
                    {
                        AddAction(newgame.Id, player.Id, Action.Гол);
                    }
                }
            }
        }

        private void AddAction(int newgameId, int playerId, Action action)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO game_action (game_id, player_id, action) VALUES({0}, {1}, {2})",
                newgameId, playerId, (int)action);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public List<Player> GetAllPlayerWitoutStatistic()
        {
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM player";

            SQLiteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SQLiteException ex)
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
                player.Position = reader["position_id"].ToString();
                players.Add(player);
            }
            return players;
        }

        public Player GetPlayerStatistic(string input)
        {
            Player player;
            Regex rxNums = new Regex(@"^\d+$"); // делаем проверку на число
            if (rxNums.IsMatch(input))
            {
                player = GetPlayerByNumber(Convert.ToInt32(input));
            }
            else
            {
                player = GetPlayerByName(input);
            }
            if (player == null) return null;
            
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM game_action WHERE player_id = " + player.Id;

            SQLiteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            //var playeractions = new List<GameAction>();
            while (reader.Read())
            {
                var game = reader["game_id"].ToString();
                var action = (Action) Convert.ToInt32(reader["action"].ToString());
                var gameaction  = new GameAction(player, game, action);

                player.Actions.Add(gameaction);
            }
            return player;
        }

        private Player GetPlayerByName(string input)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText =$"SELECT * FROM player WHERE lastname = '{input}' OR name = '{input}'";

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
                var player = new Player(Convert.ToInt32(reader["number"].ToString()), reader["name"].ToString(),
                    reader["lastname"].ToString());
                player.Id = Convert.ToInt32(reader["id"].ToString());
                return player;
            }
            return null;
        }
    }
}
