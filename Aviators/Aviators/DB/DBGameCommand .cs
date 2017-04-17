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
        public Game GetLastGame(int backId = 0)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM game WHERE id =(SELECT MAX(id) FROM game) -" + backId;

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

            var value = reader["best_player_id"].ToString();

            if (value != "")
            {
                var bestplayer = DB.DBCommands.DBPlayer.GetPlayerById(Convert.ToInt32(value));
                DB.DBCommands.DBPlayer.GetPlayerStatistic(bestplayer);
                game.BestPlayer = bestplayer;
            }

            game.Stat1 = GetGameStat(game.Id, 1);
            game.Stat2 = GetGameStat(game.Id, opteam_id);

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

        public void AddGoal(Goal goal, int game_id)
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

        public void AddAction(int newgameId, int playerId, Action action)
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
    }
}
