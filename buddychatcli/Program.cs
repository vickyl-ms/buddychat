using System;
using CommandLine;
using System.Threading.Tasks;
using buddychatcli;

namespace BuddyChatCLI
{
    class Program
    {
        public static readonly string SignUpFileName = "signup.csv";
        public static readonly string ParticipantsFileName = "participants.json";
        public static readonly string PairingHistoryFileName = "pairing_history.json";

        static int Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            CommandLineOptions options = new CommandLineOptions();
            if (!options.ParseCommandline(args))
            {
                return -1;
            }

            Console.WriteLine("Command: " + options.Command);

            EmailGenerator emailGenerator = new EmailGenerator();
            emailGenerator.CreateItemFromTemplate();

            return 0;
        }
    }
}
