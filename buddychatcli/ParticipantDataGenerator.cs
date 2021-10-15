using CommandLine;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{
    [Verb("CreateParticipantData", HelpText = "Creates Participant Data combining the existing data so far with the new signup data from the csv files into a new csv file")]
    public class ParticipantDataGenerator
    {
        [Option(shortName: 'e',
                longName: "existingDataFilePath",
                Required = false,
                HelpText = "Path of the existing csv file which already has the participant data")]
        public string ExistingListPath { get; set; } = Directory.GetCurrentDirectory();

        [Option(shortName: 'n',
                longName: "newSessionFilePath",
                Required = false,
                HelpText = "Path of the new signups csv file ")]
        public string newSessionFilePath { get; set; } = Directory.GetCurrentDirectory();

        [Option(shortName: 'f',
                longName: "fullParticipantDataFilePath",
                Required = false,
                HelpText = "Path of the new csv file generated which has entire participant data ")]
        public string newSignupListPath { get; set; } = Directory.GetCurrentDirectory();

        public int Execute()
        {
            return 0;
        }
    }
}
