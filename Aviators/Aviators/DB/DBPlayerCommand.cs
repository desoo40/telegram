using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace Aviators
{
    public class DBPlayerCommand
    {
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
                var player = new Player(Convert.ToInt32(reader["number"].ToString()),
                   reader["name"].ToString(),
                   reader["lastname"].ToString());
                player.Id = Convert.ToInt32(reader["id"].ToString());

                var value = reader["positionid"].ToString();
                if (value != "")
                    player.Position = (PlayerPosition)Convert.ToInt32(value);
                //player.VK = reader["vk_href"].ToString();
                //player.INSTA = reader["insta_href"].ToString();
                players.Add(player);
            }
            return players;
        }

        public Player GetPlayerById(int id)
        {
            return GetPlayerSQL("SELECT * FROM player WHERE id = " + id);


            //SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            //cmd.CommandText = "SELECT * FROM player WHERE id = " + id;

            //SqliteDataReader reader = null;
            //try
            //{
            //    reader = cmd.ExecuteReader();
            //}
            //catch (SqliteException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //while (reader.Read())
            //{
            //    var player = new Player(Convert.ToInt32(reader["number"].ToString()),
            //        reader["name"].ToString(),
            //        reader["lastname"].ToString());
            //    player.Id = Convert.ToInt32(reader["id"].ToString());
            //    return player;
            //}
            //return null;
        }

        /// <summary>
        /// Метод принимает запрос и возвращает класс игрока (что бы объединить разные запросы по игроку)
        /// </summary>
        private Player GetPlayerSQL(string sql)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = sql;
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
                var player = ReaderToPlayer(reader);
                return player;
            }
            return null;
        }

        private Player ReaderToPlayer(SqliteDataReader reader)
        {
            var player = new Player(Convert.ToInt32(reader["number"].ToString()),
                reader["name"].ToString(),
                reader["lastname"].ToString());
            player.Id = Convert.ToInt32(reader["id"].ToString());

            var value = reader["positionid"].ToString();
            if (value != "")
                player.Position = (PlayerPosition) Convert.ToInt32(value);

            GetPlayerInfo(player);

            return player;
        }

        private void GetPlayerInfo(Player player)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM player_info WHERE player_id =" + player.Id;
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
                player.VK = reader["vk"].ToString();
                player.INSTA = reader["insta"].ToString();
            }
        }

        public Player GetPlayerByNumber(int number)
        {
            return GetPlayerSQL("SELECT * FROM player WHERE number = " + number + " ORDER BY id DESC");
            //SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            //cmd.CommandText = "SELECT * FROM player WHERE number = " + number + " ORDER BY id DESC";

            //SqliteDataReader reader = null;
            //try
            //{
            //    reader = cmd.ExecuteReader();
            //}
            //catch (SqliteException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //while (reader.Read())
            //{
            //    var player = new Player(Convert.ToInt32(reader["number"].ToString()),
            //        reader["name"].ToString(),
            //        reader["lastname"].ToString());
            //    player.Id = Convert.ToInt32(reader["id"].ToString());
            //    //player.Position = reader["pos"].ToString();
            //    //player.VK = reader["vk_href"].ToString();
            //    //player.INSTA = reader["insta_href"].ToString();

            //    return player;
            //}
            //return null;
        }

       

        private Player GetPlayerByNameOrSurname(string nameOrSurname)
        {
            return GetPlayerSQL($"SELECT * FROM player WHERE lastname_lower = '{nameOrSurname.ToLower()}' OR name = '{nameOrSurname}'");

            //SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            ////TODO тут надо переделать, что бы у нас все что касается имен делалось с большой буквы, потом маленькими
            //cmd.CommandText = $"SELECT * FROM player WHERE lastname_lower = '{nameOrSurname.ToLower()}' OR name = '{nameOrSurname}'";

            //SqliteDataReader reader = null;
            //try
            //{
            //    reader = cmd.ExecuteReader();
            //}
            //catch (SqliteException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //while (reader != null && reader.Read())
            //{
            //    var player = new Player(Convert.ToInt32(reader["number"].ToString()), reader["name"].ToString(),
            //        reader["lastname"].ToString());
            //    player.Id = Convert.ToInt32(reader["id"].ToString());
            //    return player;
            //}
            //return null;
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
                var par = reader["param"].ToString();
                if (par != "")
                {
                    gameaction.Param = Convert.ToInt32(par);
                }
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
                if (ass) action = Action.Пас; ;
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

            return players.OrderByDescending(p => p.StatAverragePerGame).ToList().GetRange(0, input);
        }

        private List<Player> GetTopPlayersPenalty(int count)
        {
            List<Player> players = GetAllPlayerWitoutStatistic();
            foreach (var player in players)
            {
                GetPlayerStatistic(player);
            }

            var players15 = players.Where(p => p.Games > 15).ToList();

            return players15.OrderBy(p => p.Shtraf).ToList().GetRange(0, count);
        }

        internal List<Player> GetTopPlayers(Top type, int count)
        {
            if (type == Top.APG) return GetTopPlayersAPG(count);
            if (type == Top.Penalty) return GetTopPlayersPenalty(count);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();

            var typestring = "";
            if (type == Top.Assist) typestring += " WHERE asist = 'True'";
            if (type == Top.Goals) typestring += " WHERE asist = 'False'";

            cmd.CommandText =
                "SELECT  player_id , count(*) AS num FROM goal_player " + typestring + " GROUP BY player_id ORDER BY num DESC LIMIT " +
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

                if (type == Top.Assist) player.AllStatAssist = Convert.ToInt32(reader["num"].ToString());
                if (type == Top.Goals) player.AllStatGoal = Convert.ToInt32(reader["num"].ToString());
                if (type == Top.Points) player.AllStatBomb = Convert.ToInt32(reader["num"].ToString());

                players.Add(player);
            }
            return players;
        }

        public Player GetPlayerTopForTeam(Team team)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();

            //cmd.CommandText =
            //    "SELECT  player_id , count(*) AS num FROM goal_player " +
            //    "LEFT JOIN goal ON goal_player.goal_id = goal.id " +
            //    "LEFT JOIN game ON goal.game_id = game.id " +
            //    "WHERE game.op_team_id = " + team.Id + " " +
            //    "GROUP BY player_id ORDER BY num DESC LIMIT 1";

            cmd.CommandText =
                @"SELECT player_id, count(*) AS num FROM goal_player
LEFT JOIN goal ON goal_player.goal_id = goal.id
LEFT JOIN game ON goal.game_id = game.id
WHERE game.op_team_id = " + team.Id + @" AND goal_player.asist = 'True' AND player_id = (SELECT  player_id FROM goal_player
LEFT JOIN goal ON goal_player.goal_id = goal.id
LEFT JOIN game ON goal.game_id = game.id
WHERE game.op_team_id = " + team.Id + @"
GROUP BY player_id ORDER BY count(*) DESC LIMIT 1)
GROUP BY player_id ORDER BY num DESC LIMIT 1";

            SqliteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Player player = null;
            while (reader != null && reader.Read())
            {
                player = GetPlayerById(Convert.ToInt32(reader["player_id"].ToString()));

                if (player == null) return null;

                player.AllStatAssist = Convert.ToInt32(reader["num"].ToString());

            }
            reader.Close();

            cmd.CommandText =
                @"SELECT player_id, count(*) AS num FROM goal_player
LEFT JOIN goal ON goal_player.goal_id = goal.id
LEFT JOIN game ON goal.game_id = game.id
WHERE game.op_team_id = " + team.Id + @" AND goal_player.asist = 'False' AND player_id = (SELECT  player_id FROM goal_player
LEFT JOIN goal ON goal_player.goal_id = goal.id
LEFT JOIN game ON goal.game_id = game.id
WHERE game.op_team_id = " + team.Id + @"
GROUP BY player_id ORDER BY count(*) DESC LIMIT 1)
GROUP BY player_id ORDER BY num DESC LIMIT 1";

            reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }

            while (reader != null && reader.Read())
            {
                if (player != null)
                    player.AllStatGoal = Convert.ToInt32(reader["num"].ToString());
            }
            return player;
        }


        #region Добавление, обновление игрока

        public Player GetPlayerOrInsert(Player player)
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
                    player.Id = Convert.ToInt32((long) cmd.ExecuteScalar());
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

        internal void UpdatePlayersInfo(List<Player> players)
        {
            foreach (var player in players)
            {
                var p = GetPlayerOrInsert(player);

                SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
                cmd.CommandText = string.Format("UPDATE player SET positionid = {1} WHERE id = {0};" +
                                                "DELETE FROM player_info WHere player_id = {0}; " +
                                                "INSERT INTO player_info (player_id, vk, insta) VALUES({0}, '{2}', '{3}')",
                    p.Id, (int) player.Position, player.VK, player.INSTA);

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

        #region не используется

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

        #endregion
    }
}
