﻿using CommandLine;

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
        public static readonly string SignUpFileName = "signup.csv";
        public static readonly string ParticipantsFileName = "participants.json";
        public static readonly string PairingHistoryFileName = "pairing_history.json";
        public static readonly string NewPairingFileName = "random_pairings.json";
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            // Create a case insensitive parser
            var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true);

            return Parser.Default.ParseArguments<EmailGenerator, PairingGenerator, ParticipantDataGenerator>(args)
                    .MapResult(
                      (EmailGenerator emailGenerator) => emailGenerator.Execute(),
                      (PairingGenerator pairingGenerator) => pairingGenerator.Execute(),
                      (ParticipantDataGenerator participantDataGenerator) => participantDataGenerator.Execute(),
                      errs => (int)ReturnCode.ErrorParsingCommandLine);
        }
    }
}
