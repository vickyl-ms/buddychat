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
            return Parser.Default.ParseArguments<EmailGenerator, PairingGenerator, ParticipantUpdater>(args)
                    .MapResult(
                        (EmailGenerator emailGenerator) => emailGenerator.Execute(),
                        (PairingGenerator pairingGenerator) => pairingGenerator.Execute(),
                        (ParticipantUpdater participantUpdater) => participantUpdater.Execute(),
                        errors => (int)ReturnCode.ErrorParsingCommandLine);
        }
    }
}
