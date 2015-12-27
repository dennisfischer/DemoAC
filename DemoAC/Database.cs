using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAC
{
    class Database
    {
        private SQLiteConnection connection;
        private SQLiteTransaction transaction;

        public Database(string path)
        {
            connection = new SQLiteConnection(String.Format("Data Source={0};Version=3;Foreign Keys=True", path));
            connection.Open();

            string schema = @"CREATE TABLE IF NOT EXISTS maps (
                                id INTEGER NOT NULL PRIMARY KEY,
                                name TEXT NOT NULL UNIQUE
                            );

                            CREATE TABLE IF NOT EXISTS matches (
                                id INTEGER NOT NULL PRIMARY KEY,
                                sha1 TEXT NOT NULL UNIQUE,
                                tickrate INTEGER NOT NULL,
                                map_id INTEGER NOT NULL,
                                FOREIGN KEY(map_id) REFERENCES maps(id)
                            );

                            CREATE TABLE IF NOT EXISTS players (
                                id INTEGER NOT NULL PRIMARY KEY,
                                steam_id INTEGER NOT NULL UNIQUE,
                                name TEXT NOT NULL
                            );

                            CREATE TABLE IF NOT EXISTS ticks_movement (
                                match_id INTEGER NOT NULL,
                                player_id INTEGER NOT NULL,
                                tick INTEGER NOT NULL,
                                velocity REAL NOT NULL,
                                delta_velocity REAL NOT NULL,
                                delta_position REAL NOT NULL,
	                            cheating BOOL NOT NULL,
                                PRIMARY KEY(match_id, player_id, tick),
                                FOREIGN KEY(match_id) REFERENCES matches(id),
                                FOREIGN KEY(player_id) REFERENCES players(id)
                            );";
            SQLiteCommand command = new SQLiteCommand(schema, connection);
            command.ExecuteNonQuery();
        }


        public int? GetMap(string map)
        {
            string sql = "SELECT id FROM maps WHERE name = @map";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("map", map));

            object result = command.ExecuteScalar();
            if(result == null)
            {
                return null;
            }
            return Convert.ToInt32(result);
        }

        public int? InsertMap(string map)
        {
            try
            {
                string sql = "INSERT INTO maps (name) VALUES (@name)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.Add(new SQLiteParameter("name", map));
                command.ExecuteNonQuery();
                return LastInsertId();
            }
            catch (SQLiteException)
            {
                return GetMap(map);
            }
        }

        public int? GetMatch(string sha1)
        {
            string sql = "SELECT id FROM matches WHERE sha1 = @sha1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("sha1", sha1));

            object result = command.ExecuteScalar();
            if (result == null)
            {
                return null;
            }
            return Convert.ToInt32(result);
        }

        public int? InsertMatch(string sha1, float tickrate, string map)
        {
            int? map_id = InsertMap(map);
            return InsertMatch(sha1, tickrate, map_id.Value);
        }

        public int? InsertMatch(string sha1, float tickrate, int map_id)
        {
            try
            {
                string sql = "INSERT INTO matches (sha1, tickrate, map_id) VALUES (@sha1,@tickrate,@map_id)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.Add(new SQLiteParameter("sha1", sha1));
                command.Parameters.Add(new SQLiteParameter("tickrate", tickrate));
                command.Parameters.Add(new SQLiteParameter("map_id", map_id));
                command.ExecuteNonQuery();
                return LastInsertId();
            }
            catch (SQLiteException)
            {
                return GetMatch(sha1);
            }
        }

        public int? GetPlayer(long steam_id)
        {
            string sql = "SELECT id FROM players WHERE steam_id = @steam_id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("steam_id", steam_id));

            object result = command.ExecuteScalar();
            if (result == null)
            {
                return null;
            }
            return Convert.ToInt32(result);
        }

        public int? InsertPlayer(long steam_id, string name)
        {
            try
            {
                string sql = "INSERT INTO players (steam_id, name) VALUES (@steam_id,@name)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.Add(new SQLiteParameter("steam_id", steam_id));
                command.Parameters.Add(new SQLiteParameter("name", name));
                command.ExecuteNonQuery();
                return LastInsertId();
            }
            catch (SQLiteException)
            {
                return GetPlayer(steam_id);
            }
        }

        public int? LastInsertId()
        {
            string insert_id = "SELECT last_insert_rowid()";
            SQLiteCommand command = new SQLiteCommand(insert_id, connection);

            object result = command.ExecuteScalar();
            if (result == null)
            {
                return null;
            }
            return Convert.ToInt32(result);
        }

        public void InsertTick(int match_id, int player_id, int tick, float velocity, float delta_velocity, float delta_position, bool cheating)
        {
            string sql = "INSERT INTO ticks_movement (match_id, player_id, tick, velocity, delta_velocity, delta_position, cheating) VALUES (@match_id,@player_id,@tick,@velocity,@delta_velocity,@delta_position,@cheating)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("match_id", match_id));
            command.Parameters.Add(new SQLiteParameter("player_id", player_id));
            command.Parameters.Add(new SQLiteParameter("tick", tick));
            command.Parameters.Add(new SQLiteParameter("velocity", velocity));
            command.Parameters.Add(new SQLiteParameter("delta_velocity", delta_velocity));
            command.Parameters.Add(new SQLiteParameter("delta_position", delta_position));
            command.Parameters.Add(new SQLiteParameter("cheating", cheating));
            command.ExecuteNonQuery();
        }

        public void Transaction()
        {
            transaction = connection.BeginTransaction();
        }

        public void Commit()
        {
            transaction.Commit();
        }
    }
}
