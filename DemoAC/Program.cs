using System;
using DemoInfo;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace DemoAC
{
    class Program
    {
        private DemoParser parser;
        private Database database;
        private BunnyHopParser bHopParser;
        private Boolean roundStart = false;
        private bool cheating;
        private bool discrete;
        private int startTick;
        private int endTick;
        private long playerId;
        private int modulo;

        public Program(bool cheating, bool discrete, int startTick, int endTick, long playerId)
        {
            this.cheating = cheating;
            this.discrete = discrete;
            this.startTick = startTick;
            this.endTick = endTick;
            this.playerId = playerId;
        }

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Please specify demo file for input, cheating flag and discrete/gaussian flag!");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Specified File does not exists: {0}!", args[0]);
                return;
            }

            bool cheating = false;
            bool discrete = false;
            int startTick = 0;
            int endTick = 0;
            long playerId = 0;
            try
            {
                cheating = Convert.ToBoolean(args[1]);
                discrete = Convert.ToBoolean(args[2]);

                if (args.Length == 5)
                {
                    startTick = Convert.ToInt32(args[3]);
                    endTick = Convert.ToInt32(args[4]);
                }
                if (args.Length == 6)
                {
                    playerId = Convert.ToInt64(args[5]);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Incorrect usage of parameters! Use application <demo> <cheating> <startTick> <endTick>");
            }

            Program program = new Program(cheating, discrete, startTick, endTick, playerId);
            program.run_sql(args[0], "C:/Users/denni/Desktop/demos/parsed.sqlite");
        }

        private void run_json(string source, string dest)
        {
            bHopParser = new BunnyHopParser(cheating, null);

            Debug.Print("Reading from file: {0}", source);
            using (FileStream demoFile = File.OpenRead(source))
            {
                parser = new DemoParser(demoFile);


                parser.TickDone += Parser_TickDone;
                parser.RoundEnd += Parser_RoundEnd;
                parser.RoundStart += Parser_RoundStart;
                parser.ParseHeader();

                while (parser.ParseNextTick())
                {
                    if (!roundStart)
                    {
                        continue;
                    }
                }
            }

            bHopParser.generateDataSetJson();
        }

        private void run_sql(string source, string dest)
        {
            database = new Database(dest);

            Console.WriteLine("Reading from file: {0}", source);
            string sha1;
            using (FileStream demoFile = File.OpenRead(source))
            {
                SHA1Managed sha = new SHA1Managed();
                byte[] checksum = sha.ComputeHash(demoFile);
                sha1 = BitConverter.ToString(checksum)
                                           .Replace("-", string.Empty);
            }

            using (FileStream demoFile = File.OpenRead(source))
            {
                parser = new DemoParser(demoFile);

                int? match_id = database.GetMatch(sha1);

                if (match_id.HasValue)
                {
                    Console.WriteLine("Match already in database: {0} - {1}!", match_id.Value, sha1);
                    return;
                }


                bHopParser = new BunnyHopParser(cheating, database);

                parser.TickDone += Parser_TickDone;
                parser.RoundEnd += Parser_RoundEnd;
                parser.RoundStart += Parser_RoundStart;
                parser.ParseHeader();


                match_id = database.InsertMatch(sha1, parser.TickRate, parser.Map);
                modulo = (int)Math.Round(parser.TickRate, 0) / 32;

                while (parser.ParseNextTick())
                {
                    if (!roundStart)
                    {
                        continue;
                    }
                }

                bHopParser.generateDataSetSql(match_id.Value);
            }
        }

        private void Parser_RoundStart(object sender, RoundStartedEventArgs e)
        {
            roundStart = true;
        }

        private void Parser_RoundEnd(object sender, RoundEndedEventArgs e)
        {
            roundStart = false;
        }

        private void Parser_TickDone(object sender, TickDoneEventArgs e)
        {
            if (endTick == 0 || (parser.CurrentTick >= startTick && parser.CurrentTick <= endTick))
            {
                if (modulo == 1 || parser.CurrentTick % modulo == 0)
                {
                    bHopParser.parse(discrete, roundStart, parser.CurrentTick, parser.PlayingParticipants, playerId);
                }
            }
        }
    }
}
