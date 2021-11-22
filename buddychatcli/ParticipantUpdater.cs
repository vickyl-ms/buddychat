using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{
    [Verb("UpdateParticipants", HelpText = "Updates participant data by combining historical data with new signup data")]
    public class ParticipantUpdater
    {
        [Option(shortName: 'h',
                longName: "HistoricalParticipantsFile",
                Required = false,
                HelpText = "Path to json file with historical participant data. Defaults to participants.json in current directory.")]
        public string HistoricalParticipantsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.ParticipantsFileName);

        [Option(shortName: 'n',
                longName: "NewSignupsFile",
                Required = false,
                HelpText = "Filename of csv with new signups data csv file. Defaults signup.csv in current directory")]
        public string NewSignupsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.SignupsFilename);

        [Option(shortName: 'c',
                longName: "SignupsConfigFile",
                Required = false,
                HelpText = "Filename of json doc with signup config file that specifies the meaning of the columns in the signups csv. Defaults to signupsconfig.json in current directory")]

        public string SignupsConfigFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.SignupsConfigFilename);

        [Option(shortName: 's',
                longName: "SessionId",
                Required = true,
                HelpText = "The id for the new session.")]
        public string SessionId { get; set; }

        [Option(shortName: 'u',
                longName: "UpdateParticipantsFile",
                Required = false,
                HelpText = "Path to put the updated participant data. Defaults to file named participants.json in current directory")]
        public string UpdateParticipantsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.ParticipantsFileName);

        public int Execute()
        {
            ValidateOptions();

            // Read in historical participants file if there is one
            IList<Participant> historicalParticipants = ReadInHistoricalParticipantsData(this.HistoricalParticipantsFile);

            // Read in signups file
            IList<Participant> newParticipants = SignupsReader.CreateParticipantsFromNewSignUps(this.NewSignupsFile, this.SignupsConfigFile);

            // Read in historical pairings file
            // IList<PairingHistory> historicalPairingData = new List<PairingHistory>();
            // string historicalPairingsFilePath = Path.Combine(this.PathToHistoricalData, Defaults.PairingHistoryFileName);
            // if (File.Exists(historicalPairingsFilePath))
            // {
            //     string historicalPairingsFileContent = File.ReadAllText(historicalPairingsFilePath);
            //     historicalPairingData = JsonConvert.DeserializeObject<IList<PairingHistory>>(historicalPairingsFileContent);
            // }

            // Merge signup data with historical participant data
            // NOTE: this method mutates the historical participants list!!
            IList<Participant> updatedParticipants = MergeNewSignupWithHistoricalData(
                historicalParticipants, newParticipants, this.SessionId);
            
            // sort alphabetically
            updatedParticipants = updatedParticipants.OrderBy(p => p.name).ToList();
            
            // Write out updated participant file
            WriteOutUpdatedParticipantsFile(this.UpdateParticipantsFile, updatedParticipants);

            return (int)ReturnCode.Success;
        }

        private IList<Participant> ReadInHistoricalParticipantsData(string historicalParticipantsFile)
        {
            IList<Participant> historicalParticipants = new List<Participant>();
            if (File.Exists(historicalParticipantsFile))
            {
                string historicalParticipantsFileContent = File.ReadAllText(historicalParticipantsFile);
                historicalParticipants = JsonConvert.DeserializeObject<IList<Participant>>(historicalParticipantsFileContent);
            }

            foreach(Participant p in historicalParticipants)
            {
                p.Validate();
            }

            return historicalParticipants;
        }

        /// <summary>
        /// NOTE: This method mutates the objects in historical participants!!
        /// </summary>
        /// <param name="historicalParticipants"></param>
        /// <param name="newParticipants"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static IList<Participant> MergeNewSignupWithHistoricalData(
            IList<Participant> historicalParticipants, 
            IList<Participant> newParticipants,
            string sessionId)
        {
            IList<Participant> updatedParticipants = new List<Participant>(historicalParticipants);
            int numNewParticipantsAdded = 0; 
            int numParticipantsUpdated = 0;
            int numParticipantsReusingInfo = 0;

            // Create dictionary of historical particpants
            IDictionary<string, Participant> historicalParticipantsAsDictionary = new Dictionary<string, Participant>();

            foreach(Participant p in historicalParticipants)
            {
                if (historicalParticipantsAsDictionary.ContainsKey(p.email))
                {
                    throw new Exception($"Historical participants contains multiple entries with email: {p.email}.");
                }

                historicalParticipantsAsDictionary.Add(p.email, p);
            }

            // Go through new participants. Update session ids and inserting new participants into updated list
            foreach (Participant p in newParticipants)
            {
                Participant participantToUpdate;

                if (historicalParticipantsAsDictionary.TryGetValue(p.email, out participantToUpdate))
                {
                    // check if new participant has data fields, completely replace historical data field if so
                    if (p.data.Count > 0)
                    {
                        Console.Out.WriteLine($"Participant '{p.ToString()}' had data updated:");
                        Console.Out.WriteLine("Old data:");
                        Console.Out.WriteLine(participantToUpdate.ToDetailedString());
                        Console.Out.WriteLine("New data:");
                        Console.Out.WriteLine(p.ToDetailedString());
                        
                        participantToUpdate.data = p.data;
                        numParticipantsUpdated++;
                    }
                    else
                    {
                        Console.Out.WriteLine($"Participant '{p.ToString()}' reusing signup info.");
                        numParticipantsReusingInfo++;
                    }
                }
                else
                {
                    if (p.data.Count == 0)
                    {
                        throw new Exception($"Participant '{p.ToString()}' is not in historical data but did not provide signup data.");
                    }

                    participantToUpdate = p;
                    updatedParticipants.Add(p);

                    Console.Out.WriteLine($"Participant '{p.ToString()}' added to participant list.");
                    numNewParticipantsAdded++;
                }

                participantToUpdate.AddSession(sessionId);
            }

            ConsoleColor saved = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine();
            Console.Out.WriteLine("Summary:");
            Console.Out.WriteLine($"{numNewParticipantsAdded} new participants added.");
            Console.Out.WriteLine($"{numParticipantsReusingInfo} returning participants reusing signup info.");
            Console.Out.WriteLine($"{numParticipantsUpdated} returning participants updating signup info.");
            Console.Out.WriteLine();
            Console.ForegroundColor = saved;
            
            return updatedParticipants;
        }

        private void WriteOutUpdatedPairingDataFile(string outputFilePath, IList<PairingHistory> updatedPairingData)
        {
            Console.WriteLine($"Writing output to {outputFilePath}.");
            File.WriteAllText(
                outputFilePath,
                JsonConvert.SerializeObject(updatedPairingData, Formatting.Indented));
        }

        private IList<PairingHistory> UpdatePairingData(string newSignupFile, IList<PairingHistory> historicalPairingData)
        {
            throw new NotImplementedException();
        }

        private IList<PairingHistory> ReadInHistoricalPairingData(string pairingHistoryFilePath)
        {
            string json = File.ReadAllText(pairingHistoryFilePath);
            return JsonConvert.DeserializeObject<IList<PairingHistory>>(json);
        }

        private void ValidateOptions()
        {
            if (String.IsNullOrWhiteSpace(this.SessionId))
            {
                throw new ArgumentException("Session Id is required!");
            }

            if (!File.Exists(this.NewSignupsFile))
            {
                string errMsg = $"No '{this.NewSignupsFile}' found. New signup file must exist.";
                throw new ArgumentException(errMsg);
            }

            if (!File.Exists(this.HistoricalParticipantsFile))
            {
                Console.WriteLine($"No '{Defaults.ParticipantsFileName}' file found in {this.HistoricalParticipantsFile}. Creating new file.");
            }
            else if (this.HistoricalParticipantsFile == this.UpdateParticipantsFile)
            {
                throw new ArgumentException($"Updated participants file path cannot be the same as historical participants file path ({this.HistoricalParticipantsFile}).");
            }
        }

        private void WriteOutUpdatedParticipantsFile(string updatedParticipantsFile, IList<Participant> updatedParticipants)
        {
            if (File.Exists(updatedParticipantsFile))
            {
                Console.Out.WriteLine($"Output file '{updatedParticipantsFile}' already exists. Overwrite? Y/N");
                if (!Program.AskUserShouldOverwrite())
                {
                    Console.Out.WriteLine("Not overwriting file. Exiting."); 
                    return;
                }
            }

            string json = JsonConvert.SerializeObject(updatedParticipants, Formatting.Indented);
            File.WriteAllText(updatedParticipantsFile, json);
            Console.WriteLine("Participant data successfully written to " + updatedParticipantsFile);
        }
    }
}

