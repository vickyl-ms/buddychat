using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{
    [Verb("UpdatePairingHistory", HelpText = "Updates pairing history data by combining historical data with new pairing data")]
    public class PairingHistoryUpdater
    {
        [Option(shortName: 'h',
                longName: "PairingHistoryFile",
                Required = false,
                HelpText = "Path to json file with historical pairing data. Defaults to PairingHistory.json in current directory.")]
        public string PairingHistoryFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.PairingHistoryFileName);

        [Option(shortName: 'n',
                longName: "NewPairingsFile",
                Required = false,
                HelpText = "Filename of new pairings json file generated with CreatePairing command. Defaults to NewPairings.json in current directory")]
        public string NewPairingsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.NewPairingFileName);

        [Option(shortName: 'u',
                longName: "UpdatedPairingHistoryFile",
                Required = false,
                HelpText = "Path to put the updated pairing data. Defaults to file named pairings.json in current directory")]
        public string UpdatedPairingHistoryFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.PairingHistoryFileName);

        public int Execute()
        {
            ValidateOptions();

            // Read in historical participants file if there is one
            IDictionary<string, PairingHistory> pairingHistories = ReadInPairingHistoryData(this.PairingHistoryFile);

            // Read in new pairings file
            PairingList newPairings = JsonConvert.DeserializeObject<PairingList>(File.ReadAllText(this.NewPairingsFile));

            // Validate pairing file
            PairingGenerator.ValidatePairing(newPairings.pairings, pairingHistories);

            // Update pairingHistories with new pairings
            UpdatePairingHistoriesWithNewPairings(pairingHistories, newPairings);
            
            // Write out updated participant file
            WriteOutUpdatedPairingHistoryFile(this.UpdatedPairingHistoryFile, pairingHistories);

            return (int)ReturnCode.Success;
        }

        private void UpdatePairingHistoriesWithNewPairings(IDictionary<string, PairingHistory> pairingHistories, PairingList newPairings)
        {
            foreach (PairingList.Entry pairing in newPairings.pairings)
            {
                PairingHistory history1;
                if (!pairingHistories.TryGetValue(pairing.participant1Email.ToLowerInvariant(), out history1))
                {
                    history1 = new PairingHistory {
                        email = pairing.participant1Email,
                        history = new List<PairingHistory.Entry>()
                    };

                    pairingHistories.Add(history1.email.ToLowerInvariant(), history1);
                    Console.WriteLine($"New pairing history record created for {history1.email}");
                }
                else
                {
                    Console.WriteLine($"Pairing history updated for {history1.email}");
                }

                history1.history.Insert(0, new PairingHistory.Entry {
                    buddy_email = pairing.participant2Email,
                    sessionId = newPairings.sessionId
                });

                PairingHistory history2;
                if (!pairingHistories.TryGetValue(pairing.participant2Email.ToLowerInvariant(), out history2))
                {
                    history2 = new PairingHistory {
                        email = pairing.participant2Email,
                        history = new List<PairingHistory.Entry>()
                    };

                    pairingHistories.Add(history2.email.ToLowerInvariant(), history2);
                    Console.WriteLine($"New pairing history record created for {history2.email}");
                }
                else
                {
                    Console.WriteLine($"Pairing history updated for {history2.email}");
                }

                history2.history.Insert(0, new PairingHistory.Entry {
                    buddy_email = pairing.participant1Email,
                    sessionId = newPairings.sessionId
                });
            }
        }

        private IDictionary<string, PairingHistory> ReadInPairingHistoryData(string pairingHistoryFile)
        {
            IDictionary<string, PairingHistory> pairingHistories = new Dictionary<string, PairingHistory>();
            
            if (File.Exists(pairingHistoryFile))
            {
                string pairingHistoryFileContent = File.ReadAllText(pairingHistoryFile);
                pairingHistories = JsonConvert.DeserializeObject<IDictionary<string, PairingHistory>>(pairingHistoryFileContent);
            }

            return pairingHistories;
        }
 
        private void ValidateOptions()
        {
            if (!File.Exists(this.NewPairingsFile))
            {
                string errMsg = $"No '{this.NewPairingsFile}' found. New signup file must exist.";
                throw new ArgumentException(errMsg);
            }

            if (!File.Exists(this.PairingHistoryFile))
            {
                ConsoleColor saved = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No '{this.PairingHistoryFile}' found.  This command will create a new one.");
                Console.ForegroundColor = saved;

            }
            else if (this.PairingHistoryFile.Equals(this.UpdatedPairingHistoryFile, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"Updated pairing history file path cannot be the same as historical participants file path ({this.PairingHistoryFile}).\n" +
                    "Make sure to specify either --PairingHistoryFile or --UpdatedPairingHistoryFile on the commandline.");
            }
        }

        private void WriteOutUpdatedPairingHistoryFile(string updatedPairingHistoryFile, IDictionary<string, PairingHistory> updatedPairingHistories)
        {
            if (File.Exists(updatedPairingHistoryFile))
            {
                Console.Out.WriteLine($"Output file '{updatedPairingHistoryFile}' already exists. Overwrite? Y/N");
                if (!Program.AskUserShouldOverwrite())
                {
                    Console.Out.WriteLine("Not overwriting file. Exiting."); 
                    return;
                }
            }

            string json = JsonConvert.SerializeObject(updatedPairingHistories, Formatting.Indented);
            File.WriteAllText(updatedPairingHistoryFile, json);
            Console.WriteLine("Participant data successfully written to " + updatedPairingHistoryFile);
        }
    }
}

