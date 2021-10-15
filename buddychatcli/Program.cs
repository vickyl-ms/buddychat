using CommandLine;

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
        public static readonly string ParticipantsFileName = "Participants.json";
        public static readonly string PairingHistoryFileName = "PairingHistory.json";
        public static readonly string NewPairingFileName = "RandomPairings.json";
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            // Create a case insensitive parser
            var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true);

            return parser.ParseArguments<EmailGenerator, PairingGenerator, ParticipantDataGenerator>(args)
                    .MapResult(
                      (EmailGenerator emailGenerator) => emailGenerator.Execute(),
                      (PairingGenerator pairingGenerator) => pairingGenerator.Execute(),
                      (ParticipantDataGenerator participantDataGenerator) => participantDataGenerator.Execute(),
                      errs => (int)ReturnCode.ErrorParsingCommandLine);
        }
    }
}
