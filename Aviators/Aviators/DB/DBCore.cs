using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using Aviators.Configs;
using System.Globalization;
using System.Linq;

namespace Aviators
{
    public static class DB
    {
        public static DBCore DBConnection { get; set; }
        public static DBCommands DBCommands { get; set; }
        static DB()
        {
            DBConnection = new DBCore();
            DBCommands = new DBCommands();
        }
    }


    public class DBCore
    {
        string DBFile => Config.DBFile;
        readonly string SQLForCreateon = @"DB/SQLDBCreate.sql";

        public SqliteConnection Connection;

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
                Connection = new SqliteConnection($"Data Source={DBFile}; Version=3;");
                Connection.Open();

                SqliteCommand cmd = Connection.CreateCommand();
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
            Connection.Close();
            Connection.Dispose();
        }

        public void CreateDefaultDB()
        {
            string sql = File.ReadAllText(SQLForCreateon);

            SqliteCommand cmd = Connection.CreateCommand();
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

        #region Import

        //public void LoadPlayersFromFile()
        //{
        //    var players = File.ReadAllLines(Config.DBPlayersInfoPath);

        //    foreach (var player in players)
        //    {
        //        var playerinfo = player.Split(';');
        //        if (playerinfo.Length < 5)
        //        {
        //            Console.WriteLine(playerinfo + " - неверный формат сторки");
        //            continue;
        //        }

        //        SqliteCommand cmd = Connection.CreateCommand();
        //        cmd.CommandText = string.Format("INSERT INTO player (number, name, lastname, lastname_lower,position_id,vk_href,insta_href) " +
        //                                        "VALUES({0}, '{1}', '{2}', '{3}', (select ID from position_dic where name = '{4}'), '{5}','{6}')",
        //            playerinfo[0].Trim(), playerinfo[2].Trim(), playerinfo[1].Trim(), playerinfo[1].Trim().ToLower(), playerinfo[3],playerinfo[4], playerinfo[5]);

        //        try
        //        {
        //            cmd.ExecuteNonQuery();
        //        }
        //        catch (SqliteException ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //}

        //public void LoadTeamsFromFile()
        //{
        //    var teams = File.ReadAllText(Config.DBTeamsInfoPath);

        //    Match m = Regex.Match(teams, "(?<name>.*)\\((?<town>.*)\\)");
        //    while (m.Success)
        //    {
        //        SqliteCommand cmd = Connection.CreateCommand();
        //        cmd.CommandText = string.Format("INSERT INTO team (name, town, name_lower) VALUES('{0}', '{1}', '{2}')",
        //            m.Groups["name"].ToString().Trim(), m.Groups["town"].ToString().Trim(), m.Groups["name"].ToString().Trim().ToLower());

        //        try
        //        {
        //            cmd.ExecuteNonQuery();
        //        }
        //        catch (SqliteException ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }

        //        m = m.NextMatch();
        //    }
        //}

        //public void LoadGamesFromFile()
        //{
        //    var teams = File.ReadAllText(Config.DBGamesInfoPath);
        //    teams = teams.Replace("\r", "").Replace("\n", "");
        //    var games = teams.Split(new[] {"---"}, StringSplitOptions.RemoveEmptyEntries);

        //    var players = GetAllPlayerWitoutStatistic();

        //    //var season = games[0];
        //    var season = GetSeasonByNameOrInsert(games[0]);
        //    for (int i = 1; i < games.Length; i++)
        //    {
        //        var game = games[i];

        //        var gameinfo = game.Split(';');

        //        Game newgame = new Game();
        //        DateTime date = DateTime.Now;
        //        DateTime.TryParse(gameinfo[0], CultureInfo.CreateSpecificCulture("ru"), DateTimeStyles.None, out date);
        //        newgame.Date = date;
        //        //newgame.Tournament = new Tournament(gameinfo[1]);

        //        newgame.Tournament = GetTournamentByNameOrInsert(gameinfo[1], season.Id);
        //        newgame.Team2 = gameinfo[2];
        //        var score = gameinfo[3].Split(':');
        //        newgame.Score = new Tuple<int, int>(Convert.ToInt32(score[0]), Convert.ToInt32(score[1]));

        //        SqliteCommand cmd = Connection.CreateCommand();
        //        cmd.CommandText = string.Format("INSERT INTO game (date, opteam_id, opteamscore,tournament_id) " +
        //                                        "VALUES('{0}',(select ID from team where name_lower = '{1}' )," +
        //                                        " {2}, {3})",
        //            newgame.Date, newgame.Team2.ToLower(), newgame.Score.Item2, newgame.Tournament.Id);

        //        try
        //        {
        //            cmd.ExecuteNonQuery();

        //            cmd.CommandText = @"select last_insert_rowid()";
        //            newgame.Id = Convert.ToInt32((long) cmd.ExecuteScalar());
        //        }
        //        catch (SqliteException ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            continue;
        //        }


        //        var goals = gameinfo[4].Split(':')[1];
        //        var playergoal = goals.Split(',');
        //        foreach (var pg in playergoal)
        //        {
        //            string name;
        //            var num = 1;

        //            Regex re = new Regex(@"(?<name>.*)\((?<num>\d+)\)");
        //            if (re.IsMatch(pg))

        //            {
        //                var m = re.Match(pg);
        //                name = m.Groups["name"].ToString().Trim();
        //                num = Convert.ToInt32(m.Groups["num"].ToString());
        //            }
        //            else
        //            {
        //                name = pg.Trim();
        //            }

        //            var player = players.Find(p => p.Surname == name);
        //            if (player == null) continue;

        //            for (int j = 0; j < num; j++)
        //            {
        //                AddAction(newgame.Id, player.Id, Action.Гол);
        //            }
        //        }
        //    }
        //}

        

        #endregion

        public static void Initialization()
        {
            Console.WriteLine("Start Initialization");
            File.Delete(Config.DBFile);
            DBCore db = new DBCore();

            Console.WriteLine("CreateDB");
            db.CreateDefaultDB();

            //Console.WriteLine("FillPlayersFromFile");
            //db.LoadPlayersFromFile();
            //Console.WriteLine("FillTeamsFromFile");
            //db.LoadTeamsFromFile();

            //Console.WriteLine("FillGamesFromFile");
            //db.LoadGamesFromFile();

            db.Disconnect();
            Console.WriteLine("Finish Initialization");
        }
}

    public class DBCommands
    {
        public Player GetPlayerByNumber(int number)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM player WHERE number = " + number + " ORDER BY id DESC";

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
                //player.Position = reader["pos"].ToString();
                //player.VK = reader["vk_href"].ToString();
                //player.INSTA = reader["insta_href"].ToString();

                return player;
            }
            return null;
        }

        public Player GetPlayerById(int id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
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

        private void AddAction(int newgameId, int playerId, Action action)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
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
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
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
                player.Position = reader["positionid"].ToString();
                players.Add(player);
            }
            return players;
        }

        public List<Tournament> GetTournaments()
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
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

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM game_action  WHERE player_id = " + player.Id;

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
            reader.Close();
            cmd.CommandText = "SELECT goal_player.asist, goal.game_id  FROM goal_player  LEFT JOIN goal ON goal_player.goal_id = goal.id WHERE player_id = " + player.Id;

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
                //var id  = reader["id"].ToString();
                var game_id = reader["game_id"].ToString();
                 bool ass = reader["asist"].ToString() == "True";

                //var s = reader["asist"].ToString();

                //var kek = reader.GetBoolean(1);
                var action = Action.Гол;
               if (ass) action = Action.Пас;;
                var gameaction = new GameAction(player, game_id, action);

                player.Actions.Add(gameaction);
            }
            return player;
        }


        private List<Player> GetTopPlayersAPG(int input)
        {
            List<Player> players = GetAllPlayerWitoutStatistic();
            foreach (var player in players)
            {
                GetPlayerStatistic(player);
            }
           
            return players.OrderByDescending(p=>p.StatAverragePerGame).ToList().GetRange(0,input);
        }

        internal List<Player> GetTopPlayers(Top type, int count)
        {
            if (type == Top.APG) return GetTopPlayersAPG(count);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();

            var typestring = "";
            if (type == Top.Assist) typestring += " WHERE asist = 'True'";
            if (type == Top.Goals) typestring += " WHERE asist = 'False'";

            cmd.CommandText =
                "SELECT  player_id , count(*) AS num FROM goal_player "+ typestring + " GROUP BY player_id ORDER BY num DESC LIMIT " +
                count;

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

                if (type == Top.Assist) player.StatAssist = Convert.ToInt32(reader["num"].ToString());
                if (type == Top.Goals) player.StatGoal = Convert.ToInt32(reader["num"].ToString());
                if (type == Top.Points) player.StatBomb = Convert.ToInt32(reader["num"].ToString());

                players.Add(player);
            }
            return players;
        }

        private Player GetPlayerByNameOrSurname(string nameOrSurname)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM player WHERE lastname_lower = '{nameOrSurname.ToLower()}' OR name = '{nameOrSurname}'";

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

        public Game GetGame(int backId = 0)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM game WHERE id =(SELECT MAX(id) FROM game) -"+ backId;

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Game game = new Game();

            while (reader.Read())
            {

                game.Id = Convert.ToInt32(reader["id"].ToString());
                game.Date = Convert.ToDateTime(reader["date"].ToString());

                game.Team1 = "Авиаторы";
                var opteam_id = Convert.ToInt32(reader["op_team_id"].ToString());
                game.Team2 = GetTeam(opteam_id);

                game.Score = new Tuple<int, int>(
                    Convert.ToInt32(reader["score"].ToString()),
                    Convert.ToInt32(reader["op_score"].ToString()));

                var tournamentId = Convert.ToInt32(reader["tournament_id"].ToString());
                game.Tournament = GetTournament(tournamentId);

                game.Stat1 = GetGameStat(game.Id, 1);
                game.Stat2 = GetGameStat(game.Id, opteam_id);
            }

            return game;
        }

        private TeamStat GetGameStat(int gameId, int teamId)
        {

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM game_stat WHERE game_id = {gameId} AND team_id = {teamId}";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            TeamStat stat = new TeamStat();

            while (reader.Read())
            {
                stat.Shots = Convert.ToInt32(reader["shots"].ToString());
                stat.ShotsIn = Convert.ToInt32(reader["shots_in"].ToString());
                stat.Faceoff = Convert.ToInt32(reader["faceoff"].ToString());
                stat.Hits = Convert.ToInt32(reader["hits"].ToString());
                stat.BlockShots = Convert.ToInt32(reader["block_shots"].ToString());
                stat.Penalty = Convert.ToInt32(reader["penalty"].ToString());
            }

            return stat;
        }

        private Tournament GetTournament(int tournamentId)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select name from tournament where id = {0}", tournamentId);

            try
            {
                return new Tournament(cmd.ExecuteScalar().ToString());


            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return null;
        }

        private string GetTeam(int opteam_id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select name from team where id = {0}", opteam_id);

            try
            {
                return cmd.ExecuteScalar().ToString();


            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return "";
        }

        public void AddPlayer(Player player)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
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
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
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

        public void AddNewGameAndPlayers(Game game, List<Player> roster)
        {
            //Добавляем игроков
            foreach (var player in roster)
            {
                GetPlayerOrInsert(player);
            }
            //получаем ид турнира //TODO как то тупо, видимо надо просто по ссылке
            game.Tournament = GetTournamentByNameOrInsert(game.Tournament.Name, 0);

            var opteam_id = GetTeamId(game.Team2);

            #region Добавляем игру

            AddGame(game, opteam_id);

            AddGameStat(game.Id, game.Stat1, 1);
            AddGameStat(game.Id, game.Stat2, opteam_id);

            #endregion

            //Добавляем игроков в игру(пока сделаем через Action)
            foreach (var player in roster)
            {
                //var a = new GameAction(player, game.Id.ToString(), Action.Игра);
                AddAction(game.Id, player.Id, Action.Игра);
            }

            foreach (var goal in game.Goal)
            {
                AddGoal(goal, game.Id);
            }
        }

        private void AddGameStat(int gameId, TeamStat stat, int teamId)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format(
                "INSERT INTO game_stat " +
                "(game_id, team_id, shots, shots_in,faceoff, hits, block_shots, penalty) " +
                "VALUES({0},{1}, {2}, {3}, {4},{5}, {6},{7})",
                gameId, teamId, stat.Shots, stat.ShotsIn, stat.Faceoff, stat.Hits, stat.BlockShots, stat.Penalty);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void AddGame(Game game, int opteam_id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format(
                "INSERT INTO game " +
                "(date, op_team_id, score, op_score,tournament_id, viewers_count) " +
                "VALUES('{0}',{1}, {2}, {3}, {4},{5})",
                game.Date, opteam_id, game.Score.Item1, game.Score.Item2, game.Tournament.Id, game.Viewers);

            try
            {
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"select last_insert_rowid()";
                game.Id = Convert.ToInt32((long)cmd.ExecuteScalar());
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddGoal(Goal goal, int game_id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format(
                "INSERT INTO goal " +
                "(game_id, pp, sh) " +
                "VALUES({0},'{1}', '{2}')",
                game_id, goal.PowerPlay, goal.ShortHand);

            try
            {
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"select last_insert_rowid()";
                goal.Id = Convert.ToInt32((long)cmd.ExecuteScalar());
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            var sql = string.Format("INSERT INTO goal_player " +
                "(goal_id, player_id, asist) " +
                "VALUES ({0},{1}, '{2}');",
                 goal.Id, goal.Author.Id, false);

            if (goal.Assistant1!= null)
                sql += string.Format("INSERT INTO goal_player " +
                "(goal_id, player_id,asist) " +
                "VALUES ({0},{1}, '{2}');",
                 goal.Id, goal.Assistant1.Id, true);
            if (goal.Assistant2 != null)
                sql += string.Format("INSERT INTO goal_player " +
                "(goal_id, player_id, asist) " +
                "VALUES ({0},{1}, '{2}');",
                 goal.Id, goal.Assistant2.Id, true);


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


        #region SelectOrInsert

        private Player GetPlayerOrInsert(Player player)
        {
            //TODO сделать, что бы первую большую букву делал
            //player.Name = player.Name.ToLowerInvariant()[0].

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select ID from player where name = '{0}' AND " +
                                            "lastname = '{1}' AND number = {2}",
                 player.Name, player.Surname, player.Number);

            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    cmd.CommandText = String.Format(
                        "INSERT INTO player(name, lastname, number) VALUES ('{0}', '{1}', {2})",
                        player.Name, player.Surname, player.Number);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"select last_insert_rowid()";
                    player.Id = Convert.ToInt32((long)cmd.ExecuteScalar());
                }
                else
                {
                    player.Id = Convert.ToInt32(obj);
                }

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return player;
        }

        private int GetTeamId(string teamname)
        {

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"select ID from team where name_lower = '{teamname.ToLower()}' ";

            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    cmd.CommandText =
                        $"INSERT INTO team(name, name_lower) VALUES ('{teamname}', '{teamname.ToLower()}')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"select last_insert_rowid()";
                    return Convert.ToInt32((long) cmd.ExecuteScalar());
                }
                else
                {
                    return Convert.ToInt32(obj);
                }

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return 0;
        }

        private Tournament GetTournamentByNameOrInsert(string s, int season_id)
        {
            Tournament tournament = new Tournament(s);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select ID from tournament where name_lower = '{0}'",
                 tournament.Name.ToLower(), season_id);

            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    cmd.CommandText = $"INSERT INTO tournament(name, name_lower) VALUES ('{s}', '{s.ToLower()}')";
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

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
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

    }
}
