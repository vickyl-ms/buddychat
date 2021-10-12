using System;
using CommandLine;
using System.Threading.Tasks;

namespace BuddyChatCLI
{
    class Program
    {
        public static readonly string SignUpFileName = "signup.csv";
        public static readonly string ParticipantsFileName = "participants.json";
        public static readonly string PairingHistoryFileName = "pairing_history.json";

        static int Test(BuddyChatCommand command)
        {
            Console.WriteLine("Command: " + command);
            return 0;
        }

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            return await Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(async (CommandLineOptions opts) => 
                {
                    try 
                    {
                        return Test(opts.Command);
                    }
                    catch
                    {
                        Console.WriteLine("Exception thrown!");
                        return -1;
                    }
                },
                errs => Task.FromResult(-1));
        }
    }
}
