using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using Aviators.Configs;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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


        public static string ChatToGameOptions(Chat chat)
        {
            var tourid = "";
            var sesid = "";

            if (chat.Tournament != null) tourid = $"AND tournament_id = {chat.Tournament.Id}";
            if (chat.Season != null) sesid = $"AND season_id = {chat.Season.Id}";

            return tourid + " " + sesid;

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
        //        cmd.CommandText = string.Format("INSERT INTO player (number, name, surname, surname_lower,position_id,vk_href,insta_href) " +
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
        public DBGameCommand DBGame = new DBGameCommand();
       

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
                tournaments.Add(tournament);
            }
            return tournaments;
        }

        public Tournament GetTournament(int id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select name from tournament where id = {0}", id);

            try
            {
                var tournament = new Tournament(cmd.ExecuteScalar().ToString());
                tournament.Id = id;

                return tournament;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public Season GetSeason(int id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select name from season where id = {0}", id);

            try
            {
                var season = new Season(cmd.ExecuteScalar().ToString());
                season.Id = id;

                return season;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public Place GetPlace(int placeId)
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

        public string GetTeam(int opteam_id)
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

      

        public bool AddNewGameAndPlayers(Game game)
        {
            //Добавляем игроков
            foreach (var player in game.Roster)
            {
                var p = DBPlayer.GetPlayerOrInsert(player);
                if (p == null) return false;
            }
            game.Tournament = GetTournamentByNameOrInsert(game.Tournament.Name);
            game.Season = GetSeasonByDateOrInsert(game.Date);
            game.Place = GetPlaceOrInsert(game.Place.Name);

            var opteam_id = GetTeamIdOrInsert(game.Team2);


            #region Добавляем игру

            DBGame.AddGame(game, opteam_id);

            DBGame.AddGameStat(game.Id, game.Stat1, 1);
            DBGame.AddGameStat(game.Id, game.Stat2, opteam_id);

            #endregion

            if (game.Actions.Count != 0)
                foreach (var gameAction in game.Actions)
                {
                    DB.DBCommands.DBGame.AddAction(game.Id, gameAction.Player.Id, gameAction.Action, gameAction.Param);
                }

            ////Добавляем игроков в игру(пока сделаем через Action)
            //foreach (var player in game.Roster)
            //{
            //    //var a = new GameAction(player, game.Id.ToString(), Action.Игра);
            //    DBGame.AddAction(game.Id, player.Id, Action.Игра);
            //    if(player.isA)
            //        DBGame.AddAction(game.Id, player.Id, Action.Ассистент);
            //    if (player.isK)
            //        DBGame.AddAction(game.Id, player.Id, Action.Капитан);


            //}

            foreach (var goal in game.Goal)
            {
                DBGame.AddGoal(goal, game.Id);
            }

            return true;
        }

       

        internal bool UpdateGameAndPlayer(Game findGame, Game game)
        {
            game.Id = findGame.Id;
            //Добавляем игроков
            foreach (var player in game.Roster)
            {
                var p =  DBPlayer.GetPlayerOrInsert(player);
                if (p == null) return false;
            }

            game.Place = GetPlaceOrInsert(game.Place.Name);

            string bestPlayerId = "";
            if (game.BestPlayer != null) bestPlayerId =" , best_player_id = "+ game.BestPlayer.Id;

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"UPDATE game SET " +
                              $"place_id ={game.Place.Id}, score = {game.Score.Item1}, op_score = {game.Score.Item2}, viewers_count = {game.Viewers} {bestPlayerId} " +
                              $"Where id = {findGame.Id}";
            try
            {
                cmd.ExecuteNonQuery();

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            if (findGame.Actions.Count == 0)
                foreach (var gameAction in game.Actions)
                {
                    DB.DBCommands.DBGame.AddAction(findGame.Id, gameAction.Player.Id, gameAction.Action, gameAction.Param);
                }
            if (findGame.Goal.Count == 0)
                foreach (var goal in game.Goal)
                {
                    DB.DBCommands.DBGame.AddGoal(goal, findGame.Id);
                }


            cmd.CommandText = $"DELETE FROM game_stat where game_id = {findGame.Id}";
            try
            {
                cmd.ExecuteNonQuery();

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            var team = DB.DBCommands.GetTeam(game.Team2);

            DBGame.AddGameStat(findGame.Id, game.Stat1, 1);
            DBGame.AddGameStat(findGame.Id, game.Stat2, team.Id);

            return true;

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

        private Tournament GetTournamentByNameOrInsert(string s)
        {
            Tournament tournament = new Tournament(s);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select ID from tournament where name_lower = '{0}'",
                 tournament.Name.ToLower());

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

        private Season GetSeasonByDateOrInsert(DateTime date)
        {
            var seasonName = "";

            if (date.Month < 8) seasonName = (date.Year - 1) + "/" + date.Year;
            else seasonName = date.Year + "/" + (date.Year + 1);

            return GetSeasonByNameOrInsert(seasonName);
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


        /// <summary>
        /// Добавляет информацию в базу об обработанном текстовом файле
        /// </summary>
        /// <param name="gameId">(-1 будет файл с инфой по игрокам; остальное игры)</param>
        /// <returns></returns>
        public string AddParseFile(string fileInfoName, int gameId)
        {

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select count(*) from file_action where game_id = '{0}'", gameId);

            var fileName = fileInfoName;

            try
            {
                int ucount = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                
                if (ucount != 0)
                {
                    fileName += "_update" + ucount;
                }
              
                cmd.CommandText = "INSERT INTO file_action(date, filename, game_id, action)" +
                                  $"VALUES ('{DateTime.Now}', '{fileName}', {gameId}, {ucount})";
                cmd.ExecuteNonQuery();

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return fileName;
        }

        internal List<Season> GetSeasons()
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM season";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            var seasons = new List<Season>();
            while (reader.Read())
            {
                string name = reader["name"].ToString();

                var season = new Season(name);
                season.Id = Convert.ToInt32(reader["id"].ToString());
                seasons.Add(season);
            }
            return seasons;
        }

        #region Chat

        public Chat FindOrInsertChat(Telegram.Bot.Types.Chat incomeChat)
        {
            Chat chat = new Chat(incomeChat);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"select * from chat where id = '{incomeChat.Id}'";

            try
            {
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    cmd.CommandText =
                        $"INSERT INTO chat(id, username, firstname, lastname) VALUES ('{incomeChat.Id}', '{incomeChat.Username}', '{incomeChat.FirstName}', '{incomeChat.LastName}')";
                    cmd.ExecuteNonQuery();

                }
                else
                {
                    while (reader.Read())
                    {
                        var value = reader["isAdmin"].ToString();
                        if (value != "") chat.IsAdmin = Convert.ToBoolean(value);
                        value = reader["tournament_id"].ToString();
                        if (value != "") chat.Tournament = GetTournament(Convert.ToInt32(value));
                        value = reader["season_id"].ToString();
                        if (value != "") chat.Season = GetSeason(Convert.ToInt32(value));
                    }
                }

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return chat;
        }

        public void UpdateChatParams(Chat chat)
        {
            if (chat == null) return;

            string tourStr = "null", seasStr = "null";

            if (chat.Tournament != null) tourStr = chat.Tournament.Id.ToString();
            if (chat.Season != null) seasStr = chat.Season.Id.ToString();

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"update chat " +
                              $"set tournament_id = {tourStr}, season_id = {seasStr}, isAdmin = '{chat.IsAdmin.ToString()}', isTextOnly = '{chat.isTextOnly}' " +
                              $"where id = '{chat.Id}'";

            try
            {
                cmd.ExecuteNonQuery();

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddChatIncomeMsg(Chat chatFinded, string msg)
        {
            try
            {
                SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
                cmd.CommandText =
                    $"INSERT INTO chat_action(chat_id, action, text, date) VALUES ({chatFinded.Id}, 1, '{msg}', '{DateTime.Now}')";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        #endregion

        
    }
}
