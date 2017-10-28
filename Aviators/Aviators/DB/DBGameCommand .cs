using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace Aviators
{
    public class DBGameCommand
    {
        public Game GetLastGame(Chat chat, int backId = 0)
        {
            var tourid = "";
            var sesid ="";

            if (chat.Tournament != null) tourid = $"AND tournament_id = {chat.Tournament.Id}";
            if (chat.Season != null) sesid = $"AND season_id = {chat.Season.Id}";

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM game WHERE 1 {tourid} {sesid} ORDER BY id DESC LIMIT 1";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Game game = null;

            while (reader.Read())
            {
                game = ReaderToGame(reader);
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
                game = ReaderToGame(reader);
            }

            return game;
        }
        public List<Game> GetAllGames(Chat chat)
        {
            List<Game> games = new List<Game>();

            var tourid = "";
            var sesid = "";

            if (chat.Tournament != null) tourid = $"AND tournament_id = {chat.Tournament.Id}";
            if (chat.Season != null) sesid = $"AND season_id = {chat.Season.Id}";

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM game WHERE 1 {tourid} {sesid}";

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
                Game game = ReaderToGame(reader);
                games.Add(game);
            }

            return games;

        }

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
                Game game = ReaderToGame(reader);

                games.Add(game);
            }

            return games;
        }

        private Game ReaderToGame(SqliteDataReader reader)
        {
            var game = new Game();

            game.Id = Convert.ToInt32(reader["id"].ToString());
            game.Date = Convert.ToDateTime(reader["date"].ToString());

            game.Team1 = "Авиаторы";
            var opteam_id = Convert.ToInt32(reader["op_team_id"].ToString());
            game.Team2 = DB.DBCommands.GetTeam(opteam_id);

            game.Score = new Tuple<int, int>(
                Convert.ToInt32(reader["score"].ToString()),
                Convert.ToInt32(reader["op_score"].ToString()));

            var tournamentId = Convert.ToInt32(reader["tournament_id"].ToString());
            game.Tournament = DB.DBCommands.GetTournament(tournamentId);

            var placeId = Convert.ToInt32(reader["place_id"].ToString());
            game.Place = DB.DBCommands.GetPlace(placeId);

            game.Viewers = Convert.ToInt32(reader["viewers_count"].ToString());
            game.Description = reader["description"].ToString();
            game.PenaltyGame = Convert.ToBoolean(reader["penaltygame"].ToString());

            var value = reader["best_player_id"].ToString();

            if (value != "")
            {
                var bestplayer = DB.DBCommands.DBPlayer.GetPlayerById(Convert.ToInt32(value));
                DB.DBCommands.DBPlayer.GetPlayerStatistic(bestplayer);
                game.BestPlayer = bestplayer;
            }

            game.Stat1 = GetGameStat(game.Id, 1);
            game.Stat2 = GetGameStat(game.Id, opteam_id);

            game.Goal = GetGameGoals(game.Id);

            game.Actions = GetGameActions(game);

            //проставляем в состав капитана и ассистента.
            //вообще говно, надо как то по-другому
            var kap = game.Actions.Find(a => a.Action == Action.Капитан);
            if (kap != null)
            {
                var k = game.Roster.Find(i => i.Id == kap.Player.Id);
                if (k != null) k.isK = true;
            }

            foreach (var ass in game.Actions.FindAll(a => a.Action == Action.Ассистент))
            {
                var a = game.Roster.Find(i => i.Id == ass.Player.Id);
                if (a != null) a.isA = true;
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

        private List<Goal> GetGameGoals(int gameId)
        {

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM goal WHERE game_id = {gameId}";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            List<Goal> goals = new List<Goal>();
            while (reader.Read())
            {
                Goal goal = new Goal();
                goal.Id = Convert.ToInt32(reader["id"].ToString());
                goal.PowerPlay = Convert.ToBoolean(reader["pp"].ToString());
                goal.ShortHand = Convert.ToBoolean(reader["sh"].ToString());
                goal.isPenalty = Convert.ToBoolean(reader["penalty"].ToString());

                GetGoalPlayers(goal);
                goals.Add(goal);
            }

            return goals;
        }

        private List<GameAction> GetGameActions(Game game)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM game_action WHERE game_id = {game.Id}";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            List<GameAction> actions = new List<GameAction>();
            while (reader.Read())
            {
                int playerid = Convert.ToInt32(reader["player_id"].ToString());
                var player = DB.DBCommands.DBPlayer.GetPlayerById(playerid);
                var act = Convert.ToInt32(reader["action"].ToString());

                GameAction action = new GameAction(player, game, (Action) act);
                action.Id = Convert.ToInt32(reader["id"].ToString());

                actions.Add(action);
            }
            return actions;
        }

        private void GetGoalPlayers(Goal goal)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM goal_player WHERE goal_id = {goal.Id}";

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
                bool isAssist = Convert.ToBoolean(reader["asist"].ToString());
                int playerid = Convert.ToInt32(reader["player_id"].ToString());

                if (!isAssist)
                    goal.Author = DB.DBCommands.DBPlayer.GetPlayerById(playerid);
                else
                {
                    if (goal.Assistant1 == null)
                        goal.Assistant1 = DB.DBCommands.DBPlayer.GetPlayerById(playerid);
                    else
                        goal.Assistant2 = DB.DBCommands.DBPlayer.GetPlayerById(playerid);
                }
            }
        }

        public void AddGameStat(int gameId, TeamStat stat, int teamId)
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

        public void AddGame(Game game, int opteam_id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();

            //т.к. неонтяно как в склайт засунуть нулл, если у нас нул, то сделаем такую хрень
            var bp = game.BestPlayer == null ? ")" : ",{8})";
            var bp1 = game.BestPlayer == null ? ")" : ", best_player_id) ";

            cmd.CommandText = string.Format(
                "INSERT INTO game " +
                "(date, op_team_id, score, op_score,tournament_id, viewers_count, place_id, description, penaltygame, season_id" + bp1 +
                "VALUES('{0}',{1}, {2}, {3}, {4},{5},{6},'{7}', '{9}', {10} " + bp,
                game.Date, opteam_id, game.Score.Item1, game.Score.Item2, game.Tournament.Id, game.Viewers,
                game.Place.Id, game.Description, game.BestPlayer?.Id ?? null, game.PenaltyGame, game.Season.Id);

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

        public void AddGoal(Goal goal, int game_id)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format(
                "INSERT INTO goal " +
                "(game_id, pp, sh, penalty) " +
                "VALUES({0},'{1}', '{2}', '{3}')",
                game_id, goal.PowerPlay, goal.ShortHand, goal.isPenalty);

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

            if (goal.Assistant1 != null)
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

        public void AddAction(int newgameId, int playerId, Action action, int param = 0)
         {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            if (param == 0)
                cmd.CommandText = string.Format("INSERT INTO game_action (game_id, player_id, action) VALUES({0}, {1}, {2})",
                newgameId, playerId, (int)action);
            else
            {
                cmd.CommandText = string.Format("INSERT INTO game_action (game_id, player_id, action, param) VALUES({0}, {1}, {2}, {3})",
                    newgameId, playerId, (int)action, param);
            }

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Поиск игр по сопернику и дате (нужен для обновления игр)
        /// </summary>
        public Game FindGame(Game game)
        {
            var team = DB.DBCommands.GetTeam(game.Team2);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = string.Format("select ID from game where op_team_id = '{0}' AND date = '{1}'", team.Id, game.Date);

            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    return null;
                }
                else
                {
                    return GetGame(Convert.ToInt32(obj.ToString()));
                }

            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);

            }
            return null;
        }
    }
}
