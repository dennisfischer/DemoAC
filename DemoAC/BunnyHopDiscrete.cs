using DemoInfo;

namespace DemoAC
{

    enum CHANGE_VELOCITY
    {
        NONE,
        LOW,
        MEDIUM,
        HIGH
    }

    enum CHANGE_POSITION
    {
        NONE,
        LOW,
        HIGH
    }

    enum VELOCITY
    {
        NONE,
        LOW,
        MEDIUM,
        HIGH
    }
    class BunnyHopDiscrete : BunnyHop
    {
        public int Cheating { get; set; }

        public long SteamId { get; set; }

        public int Tick { get; set; }

        public int Moving { get; set; }

        public CHANGE_VELOCITY ChangeVelocity { get; set; }

        public CHANGE_POSITION ChangePosition { get; set; }

        public VELOCITY Velocity { get; set; }

        public BunnyHopDiscrete(bool Cheating, int tick, Player player, Player oldPlayer)
        {
            this.Cheating = Cheating ? 1 : 0;
            this.SteamId = player.SteamID;
            this.Tick = tick;

            Moving = player.Velocity.Absolute > 0 ? 1 : 0;

            Vector velDiff = player.Velocity - oldPlayer.Velocity;
            double velAbsolute = velDiff.Absolute;

            if (velAbsolute == 0)
            {
                ChangeVelocity = CHANGE_VELOCITY.NONE;
            }
            else if (velAbsolute <= 15)
            {
                ChangeVelocity = CHANGE_VELOCITY.LOW;
            }
            else if (velAbsolute <= 30)
            {
                ChangeVelocity = CHANGE_VELOCITY.MEDIUM;
            }
            else
            {
                ChangeVelocity = CHANGE_VELOCITY.HIGH;
            }

            if (oldPlayer.Position == null)
            {
                oldPlayer.Position = new Vector();
            }
            Vector posDiff = player.Position - oldPlayer.Position;
            double posAbsolute = posDiff.Absolute;

            if (posAbsolute == 0)
            {
                ChangePosition = CHANGE_POSITION.NONE;
            }
            else if (posAbsolute <= 4)
            {
                ChangePosition = CHANGE_POSITION.LOW;
            }
            else
            {
                ChangePosition = CHANGE_POSITION.HIGH;
            }


            double velocityAbsolute = player.Velocity.Absolute;
            if(velocityAbsolute > 260)
            {
                Velocity = VELOCITY.HIGH;
            } else if (velocityAbsolute > 230)
            {
                Velocity = VELOCITY.MEDIUM;
            } else if(velocityAbsolute > 200)
            {
                Velocity = VELOCITY.LOW;
            } else
            {
                Velocity = VELOCITY.NONE;
            }
            
        }
    }
}
