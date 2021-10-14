using System;
using CommandLine;
using System.Collections.Generic;

namespace BuddyChatCLI
{
    public enum ReturnCode 
    {
        Success = 0,
        ErrorParsingCommandLine = -1,
        ErrorInvalidCommand = -2,
        ErrorCommandFailed = -3
    }

    // Set of commands recognized by BuddyChatCLI
    public enum BuddyChatCommand 
    {
        InvalidCommand,
        Update,
        CreatePairings,
        CreateEmails
    }

    public class Program
    {
        public static readonly string SignUpFileName = "signup.csv";
        public static readonly string ParticipantsFileName = "participants.json";
        public static readonly string PairingHistoryFileName = "pairing_history.json";

        private static ReturnCode returnCode = 0;

        public static int Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            // Create a case insensitive parser
            var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true);
            
            parser.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(ExecuteCommand)
                .WithNotParsed<CommandLineOptions>(ReportErrors);

            return Convert.ToInt32(returnCode);
        }

        // Executes Command if parsing commandline was successful
        public static void ExecuteCommand(CommandLineOptions options)
        {
            switch(options.Command)
            {
                case BuddyChatCommand.CreatePairings:
                    returnCode = PairingGenerator.ExecutePairingGenerator(options);
                    break;
                case BuddyChatCommand.CreateEmails:
                    returnCode = EmailGenerator.ExecuteEmailGenerator(options);
                    break;
                default:
                    returnCode = ReturnCode.ErrorInvalidCommand;
                    throw new Exception($"Command '{options.Command}' is not implemented yet.");
            }
        }

        // Report errors if parsing commandline was successful
        public static void ReportErrors(IEnumerable<Error> errors)
        {
            returnCode = ReturnCode.ErrorParsingCommandLine;
            Console.Error.WriteLine("Error parsing commandline:");
            foreach (var error in errors)
            {
                Console.Error.WriteLine(error.ToString());
            } 
        }
    }
}
