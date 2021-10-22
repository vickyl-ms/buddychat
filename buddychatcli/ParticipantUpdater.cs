using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{
    [Verb("Update", HelpText = "Updates participant data by combining historical data with new signup data")]
    public class ParticipantUpdater
    {
        [Option(shortName: 'p',
                longName: "PathToHistoricalData",
                Required = false,
                HelpText = "Path to json file with historical participant data (participants.json) and " +
                    "historical pairing data (pairinghistory.json). Defaults to current directory.")]
        public string PathToHistoricalData { get; set; } = Directory.GetCurrentDirectory();

        [Option(shortName: 'n',
                longName: "NewSignupsFile",
                Required = false,
                HelpText = "Filename of csv with new signups data csv file. Defaults signup.csv in current directory")]
        public string NewSignupFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.SignUpFileName);

        [Option(shortName: 's',
                longName: "SessionId",
                Required = true,
                HelpText = "The id for the new session.")]
        public string SessionId { get; set; }

        [Option(shortName: 'o',
                longName: "OutputPath",
                Required = false,
                HelpText = "Path to put the updated participant data and pairing history data. Defaults to <current directory>\\<session id>")]
        public string OutputPath { get; set; }

        public int Execute()
        {
            List<Participant> historicalParticipants = ParticipantHelper.ReadInHistoricalParticipantData(Path.Combine(this.PathToHistoricalData, Defaults.ParticipantsFileName));
            List<Participant> updatedParticipants = ParticipantHelper.MergeNewSignupWithHistoricalData(NewSignupFile, historicalParticipants);
            
            string OutputPathJson = Path.Combine(OutputPath, Defaults.ParticipantsFileName);
            WriteOutUpdatedParticipantsFile(OutputPathJson, updatedParticipants);

            return (int)ReturnCode.Success;
        }

        private void WriteOutUpdatedParticipantsFile(string outputPathJson, List<Participant> updatedParticipants)
        {
            if (File.Exists(outputPathJson))
            {
                throw new Exception($"Output file '{outputPathJson}' already exists. Exiting");
            }

            string json = JsonConvert.SerializeObject(updatedParticipants, Formatting.Indented);
            File.WriteAllText(outputPathJson, json);
            Console.WriteLine("Participant data successfully written to " + outputPathJson);
        }
    }
}
