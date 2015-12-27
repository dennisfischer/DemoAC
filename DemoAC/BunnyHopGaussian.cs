using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoInfo;

namespace DemoAC
{
    class BunnyHopGaussian : BunnyHop
    {
        public bool Cheating { get; set; }

        public long SteamId { get; set; }

        public int Tick { get; set; }

        public float Velocity { get; set; }

        public float DeltaPosition { get; set; }
        public float DeltaVelocity { get; set; }

        public BunnyHopGaussian(bool cheating, int tick, Player player, Player oldPlayer)
        {
            this.Cheating = cheating;
            this.SteamId = player.SteamID;
            this.Tick = tick;
            Velocity = (float)player.Velocity.Absolute;
            DeltaVelocity = (float)(oldPlayer.Velocity - player.Velocity).Absolute;
            if (oldPlayer.Position == null)
            {
                oldPlayer.Position = new Vector();
                DeltaPosition = 0;
            }
            else {
                DeltaPosition = (float)(player.Position - oldPlayer.Position).Absolute;

                if(DeltaPosition > 50)
                {
                    DeltaPosition = 0;
                }
            }
        }
    }
}
