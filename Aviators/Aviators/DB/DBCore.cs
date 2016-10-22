using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using Aviators.Configs;
using System.Globalization;

namespace Aviators
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

        public void LoadTeamsFromFile()
        {
            var teams = File.ReadAllText(Config.DBTeamsInfoPath);

            Match m = Regex.Match(teams, "(?<name>.*)\\((?<town>.*)\\)");
            while (m.Success)
            {
                SqliteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("INSERT INTO team (name, town, name_lower) VALUES('{0}', '{1}', '{2}')",
                    m.Groups["name"].ToString().Trim(), m.Groups["town"].ToString().Trim(), m.Groups["name"].ToString().Trim().ToLower());

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                m = m.NextMatch();
            }
        }

        public void LoadGamesFromFile()
        {
            var teams = File.ReadAllText(Config.DBGamesInfoPath);
            teams = teams.Replace("\r", "").Replace("\n", "");
            var games = teams.Split(new[] {"---"}, StringSplitOptions.RemoveEmptyEntries);

            var players = GetAllPlayerWitoutStatistic();

            //var season = games[0];
            var season = GetSeasonByNameOrInsert(games[0]);
            for (int i = 1; i < games.Length; i++)
            {
                var game = games[i];

                var gameinfo = game.Split(';');

                Game newgame = new Game();
                DateTime date = DateTime.Now;
                DateTime.TryParse(gameinfo[0], CultureInfo.CreateSpecificCulture("ru"), DateTimeStyles.None, out date);
                newgame.Date = date;
                //newgame.Tournament = new Tournament(gameinfo[1]);

                newgame.Tournament = GetTournamentByNameOrInsert(gameinfo[1], season.Id);
                newgame.Team2 = gameinfo[2];
                var score = gameinfo[3].Split(':');
                newgame.Score = new Tuple<int, int>(Convert.ToInt32(score[0]), Convert.ToInt32(score[1]));

                SqliteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("INSERT INTO game (date, opteam_id, opteamscore,tournament_id) " +
                                                "VALUES('{0}',(select ID from team where name_lower = '{1}' )," +
                                                " {2}, {3})",
                    newgame.Date, newgame.Team2.ToLower(), newgame.Score.Item2, newgame.Tournament.Id);

                try
                {
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"select last_insert_rowid()";
                    newgame.Id = Convert.ToInt32((long) cmd.ExecuteScalar());
                }
                catch (SqliteException ex)
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

        private Tournament GetTournamentByNameOrInsert(string s, int season_id)
        {
            Tournament tournament = new Tournament(s);

            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("select ID from tournament where name_lower = '{0}' AND season_id = {1}",
                 tournament.Name.ToLower(), season_id);

            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    cmd.CommandText = $"INSERT INTO tournament(name, name_lower, season_id) VALUES ('{s}', '{s.ToLower()}', {season_id})";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"select last_insert_rowid()";
                    tournament.Id = Convert.ToInt32((long)cmd.ExecuteScalar());
                }
                else
                {
                    tournament.Id = Convert.ToInt32(obj);
                }
                
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
                
            }
            return tournament;
        }

        private Season GetSeasonByNameOrInsert(string s)
        {
            Season season = new Season(s);

            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("select ID from season where name = '{0}'",
                 season.Name);

            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    cmd.CommandText = $"INSERT INTO season(name) VALUES ('{s}')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"select last_insert_rowid()";
                    season.Id = Convert.ToInt32((long)cmd.ExecuteScalar());
                }
                else
                {
                    season.Id = Convert.ToInt32(obj);
                }

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return season;
        }

        #endregion


        private void AddAction(int newgameId, int playerId, Action action)
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO game_action (game_id, player_id, action) VALUES({0}, {1}, {2})",
                newgameId, playerId, (int)action);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
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
                player.Position = reader["position_id"].ToString();
                players.Add(player);
            }
            return players;
        }

        public List<Tournament> GetTournaments()
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM tournament";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            var tournaments = new List<Tournament>();
            while (reader.Read())
            {
                string name = reader["name"].ToString();

                var tournament = new Tournament(name);
                tournament.Id = Convert.ToInt32(reader["id"].ToString());
                //tournament.Season = reader["season_id"].ToString();
                tournaments.Add(tournament);
            }
            return tournaments;
        }

       
        public Player GetPlayerStatisticByNameOrSurname(string nameOrSurname)
        {
            var player = GetPlayerByNameOrSurname(nameOrSurname);
            return GetPlayerStatistic(player);
        }
        public Player GetPlayerStatisticByNumber(int number)
        {
            var player = GetPlayerByNumber(number);
            return GetPlayerStatistic(player);
        }
        public Player GetPlayerStatistic(Player player)
        {
            if (player == null) return null;

            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM game_action WHERE player_id = " + player.Id;

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            //var playeractions = new List<GameAction>();
            while (reader.Read())
            {
                var game_id = reader["game_id"].ToString();
                var action = (Action)Convert.ToInt32(reader["action"].ToString());
                var gameaction = new GameAction(player, game_id, action);

                player.Actions.Add(gameaction);
            }
            return player;
        }


        public List<Player> GetTopPlayers(int input)
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT  player_id , count(*) AS num FROM game_action GROUP BY player_id ORDER BY num DESC LIMIT " +
                input;

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            List<Player> players = new List<Player>();
            while (reader.Read())
            {
                var player = GetPlayerById(Convert.ToInt32(reader["player_id"].ToString()));

                player = GetPlayerStatistic(player);
                players.Add(player);
            }
            return players;
        }

        private Player GetPlayerByNameOrSurname(string nameOrSurname)
        {
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
                var player = new Player(Convert.ToInt32(reader["number"].ToString()), reader["name"].ToString(),
                    reader["lastname"].ToString());
                player.Id = Convert.ToInt32(reader["id"].ToString());
                return player;
            }
            return null;
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
            Console.WriteLine("FillTeamsFromFile");
            db.LoadTeamsFromFile();

            Console.WriteLine("FillGamesFromFile");
            db.LoadGamesFromFile();

            db.Disconnect();
            Console.WriteLine("Finish Initialization");
        }
}
}
