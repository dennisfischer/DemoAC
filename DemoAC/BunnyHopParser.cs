using System.Collections.Generic;
using DemoInfo;
using Newtonsoft.Json;
using System.IO;
using System;

namespace DemoAC
{
    class BunnyHopParser
    {
        private Dictionary<long, List<BunnyHop>> playerHops = new Dictionary<long, List<BunnyHop>>();
        private Dictionary<long, string> playerNames = new Dictionary<long, string>();
        private Dictionary<long, Player> oldPlayer = new Dictionary<long, Player>();
        private bool cheating;
        private Database database;

        public BunnyHopParser(bool cheating, Database database)
        {
            this.database = database;
            this.cheating = cheating;
        }

        public void parse(bool discrete, bool roundStart, int tick, IEnumerable<Player> playingParticipants, long playerId)
        {
            var defaultPlayer = new Player();
            foreach (var player in playingParticipants)
            {
                if (playerId != 0 && player.SteamID != playerId) continue;
                if (!player.IsAlive) {
                    oldPlayer.Remove(player.SteamID);
                    continue;
                }
                //if (!roundStart) continue;

                if(!playerHops.ContainsKey(player.SteamID))
                {
                    playerHops[player.SteamID] = new List<BunnyHop>();
                    playerNames[player.SteamID] = player.Name;
                }

                Player old = defaultPlayer;
                if (oldPlayer.ContainsKey(player.SteamID))
                {
                    old = oldPlayer[player.SteamID];
                }
                if (discrete)
                {
                    playerHops[player.SteamID].Add(new BunnyHopDiscrete(cheating, tick, player, old));
                } else
                {
                    playerHops[player.SteamID].Add(new BunnyHopGaussian(cheating, tick, player, old));

                }
                oldPlayer[player.SteamID] = player.Copy();
            }
        }

        public void generateDataSetJson()
        {
            foreach(var entry in playerHops)
            {
                if (entry.Key == 0) continue;
                StreamWriter target = new StreamWriter("C:/Users/denni/Desktop/demos/"+entry.Key+"-"+(cheating ? "on" : "off")+".json");
                target.Write(JsonConvert.SerializeObject(entry.Value));
                target.Close();
            }
        }

        public void generateDataSetSql(int match_id)
        {
            Dictionary<long, int> playerIds = new Dictionary<long, int>();

            database.Transaction();
            foreach (var entry in playerNames)
            {
                if (entry.Key == 0) continue;
                int? player_id = database.InsertPlayer(entry.Key, entry.Value);

                foreach (BunnyHopGaussian tick in playerHops[entry.Key])
                {
                    database.InsertTick(match_id, player_id.Value, tick.Tick, tick.Velocity, tick.DeltaVelocity, tick.DeltaPosition, tick.Cheating);
                }
            }
            database.Commit();
        }
    }
}
