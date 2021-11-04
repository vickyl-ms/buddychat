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
            ValidateOptions();

            IList<Participant> historicalParticipants = ParticipantHelper.ReadInHistoricalParticipantData(
                Path.Combine(this.PathToHistoricalData, Defaults.ParticipantsFileName));
            IList<Participant> updatedParticipants = ParticipantHelper.MergeNewSignupWithHistoricalData(
                this.NewSignupFile, historicalParticipants);

            IList<PairingHistory> historicalPairingData = ReadInHistoricalPairingData(
                Path.Combine(this.PathToHistoricalData, Defaults.PairingHistoryFileName));
            IList<PairingHistory> updatedPairingData = UpdatePairingData(this.NewSignupFile, historicalPairingData);
            
            string outputFilePath = Path.Combine(this.OutputPath, Defaults.ParticipantsFileName);
            WriteOutUpdatedParticipantsFile(outputFilePath, updatedParticipants);

            outputFilePath = Path.Combine(this.OutputPath, Defaults.PairingHistoryFileName);
            WriteOutUpdatedPairingDataFile(outputFilePath, updatedPairingData);

            return (int)ReturnCode.Success;
        }

        private void WriteOutUpdatedPairingDataFile(string outputFilePath, IList<PairingHistory> updatedPairingData)
        {
            throw new NotImplementedException();
        }

        private IList<PairingHistory> UpdatePairingData(string newSignupFile, IList<PairingHistory> historicalPairingData)
        {
            throw new NotImplementedException();
        }

        private IList<PairingHistory> ReadInHistoricalPairingData(string v)
        {
            throw new NotImplementedException();
        }

        private void ValidateOptions()
        {
            if (String.IsNullOrWhiteSpace(this.SessionId))
            {
                throw new ArgumentException("Session Id is required!");
            }

            if (!File.Exists(this.NewSignupFile))
            {
                string errMsg = $"No '{this.NewSignupFile}' found. New signup file must exist.";
                throw new ArgumentException(errMsg);
            }

            if (!File.Exists(Path.Combine(this.PathToHistoricalData, Defaults.ParticipantsFileName)))
            {
                Console.WriteLine($"No '{Defaults.ParticipantsFileName}' file found in {this.PathToHistoricalData}. Creating new file.");
            }

            if (!File.Exists(Path.Combine(this.PathToHistoricalData, Defaults.PairingHistoryFileName)))
            {
                Console.WriteLine($"No '{Defaults.PairingHistoryFileName}' file found in {this.PathToHistoricalData}. Creating new file.");
            }

            if (String.IsNullOrWhiteSpace(this.OutputPath))
            {
                this.OutputPath = Path.Combine(Directory.GetCurrentDirectory(), SessionId);
                Console.WriteLine($"No output path specified. Defaulting to '{this.OutputPath}'.");
            }

            if (!Directory.Exists(this.OutputPath))
            {
                Console.WriteLine($"Output directory '{this.OutputPath}' does not exist. Creating directory.");
                Directory.CreateDirectory(this.OutputPath);
            }
        }

        private void WriteOutUpdatedParticipantsFile(string outputPathJson, IList<Participant> updatedParticipants)
        {
            if (File.Exists(outputPathJson))
            {
                throw new Exception($"Output file '{outputPathJson}' already exists. Exiting.");
            }

            string json = JsonConvert.SerializeObject(updatedParticipants, Formatting.Indented);
            File.WriteAllText(outputPathJson, json);
            Console.WriteLine("Participant data successfully written to " + outputPathJson);
        }
    }
}
