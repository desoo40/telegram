﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using Aviators.Configs;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

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
        public DBPlayerCommand DBPlayer = new DBPlayerCommand();
       

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
       
        public Game GetLastGame(int backId = 0)
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
        public Game GetGame(int Id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM game WHERE id =" + Id;

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

                var placeId = Convert.ToInt32(reader["place_id"].ToString());
                game.Place = GetPlace(placeId);

                game.Viewers = Convert.ToInt32(reader["viewers_count"].ToString());

                var value = reader["best_player_id"].ToString();

                if (value != "")
                {
                    var bestplayer = DBPlayer.GetPlayerById(Convert.ToInt32(value));
                    DBPlayer.GetPlayerStatistic(bestplayer);
                    game.BestPlayer = bestplayer;
                }
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

        private Place GetPlace(int placeId)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select * from place where id = {0}", placeId);

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Place place = new Place("");

            while (reader.Read())
            {
                place.Id = Convert.ToInt32(reader["id"].ToString());
                place.Name = reader["name"].ToString();
                place.FullAdress = reader["adress"].ToString();
                place.GeoPos = reader["geoposition"].ToString();
            }

            return place;
        }

        public List<Game> GetAllGames()
        {
            List<Game> games = new List<Game>();

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM game";

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
                Game game = new Game();
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

                games.Add(game);
            }

            return games;

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

      

        public void AddNewGameAndPlayers(Game game, List<Player> roster)
        {
            //Добавляем игроков
            foreach (var player in roster)
            {
                DBPlayer.GetPlayerOrInsert(player);
            }
            //получаем ид турнира //TODO как то тупо, видимо надо просто по ссылке
            game.Tournament = GetTournamentByNameOrInsert(game.Tournament.Name, 0);
            game.Place = GetPlaceOrInsert(game.Place.Name);

            var opteam_id = GetTeamIdOrInsert(game.Team2);


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

            //т.к. неонтяно как в склайт засунуть нулл, если у нас нул, то сделаем такую хрень
            var bp = game.BestPlayer == null ? ")" : ",{7})";
            var bp1 = game.BestPlayer == null ? ")" : ", best_player_id) ";

            cmd.CommandText = string.Format(
                "INSERT INTO game " +
                "(date, op_team_id, score, op_score,tournament_id, viewers_count, place_id" + bp1 +
                "VALUES('{0}',{1}, {2}, {3}, {4},{5},{6} " + bp,
                game.Date, opteam_id, game.Score.Item1, game.Score.Item2, game.Tournament.Id, game.Viewers,
                game.Place.Id, game.BestPlayer?.Id ?? null);

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

        public Team GetTeam(string teamname)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"select * from team where name_lower = '{teamname.ToLower()}' ";
            SqliteDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }


            Team team  = new Team();
            while (reader != null && reader.Read())
            {
                team.Id = Convert.ToInt32(reader["id"].ToString());
                team.Name = reader["name"].ToString();
            }

            return team;
        }


        #region SelectOrInsert

        private int GetTeamIdOrInsert(string teamname)
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
        private Place GetPlaceOrInsert(string s)
        {
            Place place  = new Place(s);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select ID from place where name_lower = '{0}'",
                 place.Name.ToLower());

            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    cmd.CommandText = $"INSERT INTO place(name, name_lower) VALUES ('{s}', '{s.ToLower()}')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"select last_insert_rowid()";
                    place.Id = Convert.ToInt32((long)cmd.ExecuteScalar());
                }
                else
                {
                    place.Id = Convert.ToInt32(obj);
                }

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return place;
        }


        #endregion

        public List<Game> GetGamesTeam(Team team)
        {
            List<Game> games = new List<Game>();

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM game WHERE op_team_id = " + team.Id;

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
                Game game = new Game();
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

                games.Add(game);
            }

            return games;
        }

        public List<Team> GetAllTeams()
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"select * from team";
            SqliteDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            List<Team> teams = new List<Team>();
            
            while (reader != null && reader.Read())
            {
                Team team = new Team();
                team.Id = Convert.ToInt32(reader["id"].ToString());
                team.Name = reader["name"].ToString();
                teams.Add(team);
            }
            return teams;
        }
    }
}
