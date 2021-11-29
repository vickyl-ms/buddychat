using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace BuddyChatCLI
{
    public enum ReturnCode 
    {
        Success = 0,
        ErrorParsingCommandLine = -1,
        ErrorInvalidCommand = -2,
        ErrorCommandFailed = -3
    }

    public static class Defaults
    {
        public static readonly string SignupsFilename = "Signups.csv";
        public static readonly string SignupsConfigFilename = "SignupsConfig.json";
        public static readonly string ParticipantsFileName = "Participants.json";
        public static readonly string PairingHistoryFileName = "PairingHistory.json";
        public static readonly string NewPairingFileName = "NewPairings.json";
        public static readonly string EmailTemplateFilename = "EmailTemplate.oft";
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            // Set console to support unicode
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Create a case insensitive parser
            var parser = new Parser(cfg => 
                {
                    cfg.CaseSensitive = false;
                    cfg.IgnoreUnknownArguments = false;
                    cfg.HelpWriter = Console.Error;
                });

            return parser.ParseArguments<EmailGenerator, PairingGenerator, ParticipantUpdater, PairingHistoryUpdater>(args)
                    .MapResult(
                        (EmailGenerator emailGenerator) => emailGenerator.Execute(),
                        (PairingGenerator pairingGenerator) => pairingGenerator.Execute(),
                        (ParticipantUpdater participantUpdater) => participantUpdater.Execute(),
                        (PairingHistoryUpdater pairingHistoryUpdater) => pairingHistoryUpdater.Execute(),
                        errors => (int)ReturnCode.ErrorParsingCommandLine);
        }

        public static bool AskUserShouldOverwrite()
        {
            ConsoleKeyInfo keyInfo;
            
            do
            {
                keyInfo = Console.ReadKey(true);
                switch(keyInfo.KeyChar)
                {
                    case 'Y':
                    case 'y': break;
                    case 'q':
                    case 'Q':
                    case 'N':
                    case 'n': return false;
                    default: Console.Out.WriteLine("Invalid char: " + keyInfo.KeyChar); break;
                }
            } while(keyInfo.KeyChar != 'y' && keyInfo.KeyChar != 'Y');

            return true;
        }
    }
}
