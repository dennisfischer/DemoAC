using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoInfo;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace DemoAC
{
    class Program
    {
        static DemoParser parser;
        static Boolean roundStart = false;
        static StreamWriter target;

        static void Main(string[] args)
        {                
            if (args.Length == 0)
            {
                Debug.Print("Please specify demo file for input!");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Debug.Print("Specified File does not exists: {0}!", args[0]);
                return;
            }

            target = new StreamWriter("C:/Users/denni/Desktop/demos/parsed1.json");

            Debug.Print("Reading from file: {0}", args[0]);
            using (FileStream demoFile = File.OpenRead(args[0]))
            {
                parser = new DemoParser(demoFile);


                parser.TickDone += Parser_TickDone;
                parser.RoundEnd += Parser_RoundEnd;
                parser.RoundStart += Parser_RoundStart;
                parser.ParseHeader();


                target.Write("[");

                while (parser.ParseNextTick())
                {
                    if(!roundStart)
                    {
                        continue;
                    }
                    target.Write(",");
                }
                target.Write("]");

            }
        }

        private static void Parser_RoundStart(object sender, RoundStartedEventArgs e)
        {
            roundStart = true;
        }

        private static void Parser_RoundEnd(object sender, RoundEndedEventArgs e)
        {
            roundStart = false;
        }

        private static void Parser_TickDone(object sender, TickDoneEventArgs e)
        {
            bool first = true;
            foreach (var player in parser.PlayingParticipants)
            {
                if (!player.IsAlive) continue;
                if (!roundStart) continue;
                if (first)
                {
            
                    first = false;
                    target.Write("[{");
                }
                else
                {
                    target.Write(",{");
                }
                target.Write("\"steamid\":{0},\"tick\":{1},\"velocity\":{2},\"position\":{3},\"view_x\":{4},\"view_y\":{5}", player.SteamID, parser.CurrentTick, FormatVector(player.Velocity), FormatVector(player.Position), player.ViewDirectionX.ToString("G", CultureInfo.InvariantCulture), player.ViewDirectionY.ToString("G", CultureInfo.InvariantCulture));
                target.Write("}");
            }
            if (!first)
            {
                target.Write("]");
            }
        }

        private static string FormatVector(Vector velocity)
        {
            return "{" + String.Format("\"X\":{0},\"Y\":{1},\"Z\":{2}", velocity.X.ToString("G", CultureInfo.InvariantCulture), velocity.Y.ToString("G", CultureInfo.InvariantCulture), velocity.Z.ToString("G", CultureInfo.InvariantCulture)) + "}";
        }
    }
}
