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
        public List<Player> GetAllPlayerWithoutStatistic()
        {
            var players = GetPlayersSQL("SELECT * FROM player");
           
            return players;
        }

        public Player GetPlayerById(int id)
        {
            return GetPlayersSQL("SELECT * FROM player WHERE id = " + id).FirstOrDefault();
        }

        public List<Player> GetPlayersByNumber(int number)
        {
            return GetPlayersSQL("SELECT * FROM player WHERE number = " + number + " ORDER BY id DESC");
        }

        public List<Player> GetPlayersBySurname(string surname)
        {
             return GetPlayersSQL($"SELECT * FROM player WHERE surname_lower = '{surname.ToLower()}'");
        } 

        /// <summary>
        /// Метод принимает запрос и возвращает класс игрока (что бы объединить разные запросы по игроку)
        /// </summary>
        private List<Player> GetPlayersSQL(string sql)
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

            var players = new List<Player>();
            while (reader.Read())
            {
                var player = ReaderToPlayer(reader);
                players.Add(player);
            }
            return players;
        }

        private Player ReaderToPlayer(SqliteDataReader reader)
        {
            var player = new Player(Convert.ToInt32(reader["number"].ToString()),
                reader["name"].ToString(),
                reader["surname"].ToString(),
                reader["patronymic"].ToString());
            player.Id = Convert.ToInt32(reader["id"].ToString());

            var value = reader["positionid"].ToString();
            if (value != "")
                player.Position = (PlayerPosition) Convert.ToInt32(value);

            //GetPlayerInfo(player);

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

        public Player GetPlayerStatistic(Chat chat, Player player)
        {
            if (player == null) return null;

            var stOptions = DB.ChatToGameOptions(chat);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText =
                "SELECT game_action.* FROM game_action " +
                "LEFT JOIN game ON game.id = game_action.game_id " +
                $"WHERE player_id = {player.Id} {stOptions};";

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
                var action = (Action) Convert.ToInt32(reader["action"].ToString());
                var gameaction = new GameAction(player, game_id, action);
                var par = reader["param"].ToString();
                if (par != "")
                {
                    gameaction.Param = Convert.ToInt32(par);
                }
                player.Actions.Add(gameaction);

            }
            reader.Close();
            cmd.CommandText = "SELECT goal_player.asist, goal.game_id  FROM goal_player  " +
                              "LEFT JOIN goal ON goal_player.goal_id = goal.id " +
                              "LEFT JOIN game ON game.id = goal.game_id " +
                              $"WHERE player_id = {player.Id} {stOptions};";

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
                if (ass) action = Action.Пас;
                ;
                var gameaction = new GameAction(player, game_id, action);

                player.Actions.Add(gameaction);
            }
            return player;
        }

        #region Топы всякие

        internal List<Player> GetTopPlayers(Chat chat, Top type, int count)
        {
            if (type == Top.APG) return GetTopPlayersAPG(chat, count);
            if (type == Top.Penalty) return GetTopPlayersPenalty(chat, count);

            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();

            var typestring = "";
            if (type == Top.Assist) typestring += " WHERE asist = 'True'";
            if (type == Top.Goals) typestring += " WHERE asist = 'False'";

            var stOptions = DB.ChatToGameOptions(chat);
            if (typestring == "")
                stOptions = "WHERE 1 " + stOptions;

            cmd.CommandText =
                "SELECT  player_id , count(*) AS num " +
                "FROM goal_player " +
                "LEFT JOIN goal ON goal_player.goal_id = goal.id " +
                "LEFT JOIN game ON game.id = goal.game_id " +
                $"{typestring} {stOptions} " +
                $"GROUP BY player_id ORDER BY num DESC LIMIT {count};";

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

                GetPlayerStatistic(chat, player);

                players.Add(player);
            }
            return players;
        }

        private List<Player> GetTopPlayersAPG(Chat chat, int input)
        {
            List<Player> players = GetAllPlayerWithoutStatistic();
            foreach (var player in players)
            {
                GetPlayerStatistic(chat, player);
            }

            return players.OrderByDescending(p => p.StatAverragePerGame).ToList().GetRange(0, input);
        }

        private List<Player> GetTopPlayersPenalty(Chat chat, int count)
        {
            List<Player> players = GetAllPlayerWithoutStatistic();
            foreach (var player in players)
            {
                GetPlayerStatistic(chat, player);
            }

            //переметр, показывающий, что игрок должен сыграть более 15 матчей для попадания в статистику пенальти
            var players15 = players.Where(p => p.Games > 15).ToList();

            return players15.OrderBy(p => p.Shtraf).ToList().GetRange(0, count);
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
WHERE game.op_team_id = " + team.Id +
                @" AND goal_player.asist = 'True' AND player_id = (SELECT  player_id FROM goal_player
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
WHERE game.op_team_id = " + team.Id +
                @" AND goal_player.asist = 'False' AND player_id = (SELECT  player_id FROM goal_player
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

        #endregion

        #region Добавление, обновление игрока

        public Player GetPlayerOrInsert(Player player)
        {
            //TODO сделать, что бы первую большую букву делал
            //player.Name = player.Name.ToLowerInvariant()[0].

            var findPlayers = GetPlayersBySurname(player.Surname);

            // не нашли с фамилией, записываем игрока
            if (findPlayers.Count == 0) InsertPlayer(player);

            //если нашли один, проверяем есть ли отчество во входящем и сравниваем
            if (findPlayers.Count == 1)
                OnePlayerAnalayze(player, findPlayers[0]);
            if(findPlayers.Count > 1)
            {
                findPlayers = findPlayers.FindAll(f =>f.Name.ToLower() == player.Name.ToLower());

                if (findPlayers.Count == 0) InsertPlayer(player);
                if (findPlayers.Count == 1)
                    OnePlayerAnalayze(player, findPlayers[0]);

                if (findPlayers.Count > 1)
                {
                    if (player.Patronymic == null)
                        Console.WriteLine("Нашли более одного игрока в базе, необходимо добавить отчество: " + player);
                    else
                    {
                        findPlayers = findPlayers.FindAll(f => f.Patronymic.ToLower() == player.Patronymic.ToLower());

                        if (findPlayers.Count == 0) InsertPlayer(player);
                        if (findPlayers.Count == 1) player.Id = findPlayers[0].Id;
                        if(findPlayers.Count>1)
                            Console.WriteLine("Нашли более одного игрока в базе, ошибка: " + player);
                    }
                }
            }
            return player;
        }

        private void OnePlayerAnalayze(Player player, Player findPlayer)
        {
            if (player.Patronymic == null)
            {
                if (SameName(player, findPlayer))
                {
                    if (findPlayer.Patronymic != null)
                        Console.WriteLine("В базе отчество есть, а во входящем нету: " + player);

                    player.Id = findPlayer.Id;
                }
                else
                    InsertPlayer(player);

            }
            else
            {
                if (findPlayer.Patronymic != null)
                {
                    if (SamePatronymic(player, findPlayer))
                        player.Id = findPlayer.Id;
                    else
                        InsertPlayer(player);
                }
                else
                {
                    UpdatePatronymicPlayer(player);
                    player.Id = findPlayer.Id;
                }

            }
        }

        private void UpdatePatronymicPlayer(Player player)
        {
            

        }

        private bool SameName(Player p1, Player p2)
        {
            return p1.Name.ToLower() == p2.Name.ToLower();
        }
        private bool SameSurname(Player p1, Player p2)
        {
            return p1.Surname.ToLower() == p2.Surname.ToLower();
        }
        private bool SamePatronymic(Player p1, Player p2)
        {
            return p1.Patronymic.ToLower() == p2.Patronymic.ToLower();
        }


        private void InsertPlayer(Player player)
        {
            SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
            cmd.CommandText = String.Format(
                       "INSERT INTO player(name, surname, surname_lower, patronymic, number) VALUES ('{0}', '{1}','{3}', '{4}', {2})",
                       player.Name, player.Surname, player.Number, player.Surname.ToLower(), player.Patronymic);
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"select last_insert_rowid()";
            player.Id = Convert.ToInt32((long)cmd.ExecuteScalar());
        }

        internal void UpdatePlayersInfo(List<Player> players)
        {
            foreach (var player in players)
            {
                var p = GetPlayerOrInsert(player);

                if (p == null) continue;

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

        //public void AddPlayer(Player player)
        //{
        //    SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
        //    cmd.CommandText = string.Format("INSERT INTO player (number, name, surname) VALUES({0}, '{1}', '{2}')",
        //        player.Number, player.Name, player.Surname);

        //    try
        //    {
        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (SqliteException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

        //public void RemovePlayerByNumber(int number)
        //{
        //    SqliteCommand cmd = DB.DBConnection.Connection.CreateCommand();
        //    var player = GetPlayerByNumber(number);
        //    if (player == null) return;

        //    cmd.CommandText = string.Format("DELETE from player where number={0}", number);

        //    try
        //    {
        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (SqliteException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

        #endregion
    }
}
