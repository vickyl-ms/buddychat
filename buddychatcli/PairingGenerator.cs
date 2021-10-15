using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{
    /// <summary>
    /// Pairing Generator returns a list of Pairing Entries
    /// </summary>
    public class PairingEntry
    {
        public Participant participant1;
        public Participant participant2;
    }

    /// <summary>
    /// Generates random pairs of participants, making sure that each pair has never
    /// been paired before.
    /// </summary>
    [Verb("CreatePairings", HelpText = "Creates the random assignments.")]
    public class PairingGenerator
    {
        private Random rng = new Random();  

        [Option(shortName: 's',
                longName: "sessionId",
                Required = true,
                HelpText = "The id for this specific session.")]
        public string SessionId { get; set; }

        [Option(shortName: 'p',
                longName: "pathToHistoricalData",
                Required = false,
                HelpText = "The location of participant and history json. Default is current directory.")]
        public string PathToHistoricalData { get; set; } = Directory.GetCurrentDirectory();

        [Option(shortName: 'o',
                longName: "outputPath",
                Required = false,
                HelpText = "Output location. Default is current directory.")]
        public string OutputPath { get; set; } = Directory.GetCurrentDirectory();

        public int Execute()
        {
            return 0;
        }

        /// <summary>
        /// Validate and generate default options. Read in historical data.
        /// </summary>
        /// <returns>List of Pairing entries</returns>
        public ReturnCode Generate()
        {
            ValidateOptions();
            IEnumerable<Participant> participants = ReadInParticipantFile();
            Dictionary<string, PairingHistory> pairingHistories = ReadInPairingHistoryFile();
            IEnumerable<PairingEntry> pairings = Generate(this.SessionId, participants, pairingHistories);
            WritePairingsToFile(pairings);
            return ReturnCode.Success;
        }

        /// <summary>
        /// Write out list of Pairing Entries to file
        /// </summary>
        private void WritePairingsToFile(IEnumerable<PairingEntry> pairings)
        {
            File.WriteAllText(
                Path.Combine(this.OutputPath, Defaults.NewPairingFileName),
                JsonConvert.SerializeObject(pairings, Formatting.Indented));
        }

        private Dictionary<string, PairingHistory> ReadInPairingHistoryFile()
        {
            string json = File.ReadAllText(Path.Combine(this.PathToHistoricalData, Defaults.PairingHistoryFileName));
            return JsonConvert.DeserializeObject<Dictionary<string, PairingHistory>>(json);
        }

        private IEnumerable<Participant> ReadInParticipantFile()
        {
            string json = File.ReadAllText(Path.Combine(this.PathToHistoricalData, Defaults.ParticipantsFileName));
            return JsonConvert.DeserializeObject<List<Participant>>(json);
        }

        private void ValidateOptions()
        {
            if (String.IsNullOrWhiteSpace(this.SessionId))
            {
                throw new ArgumentException("Session id is required!");
            }

            if (!File.Exists(Path.Combine(this.PathToHistoricalData, Defaults.ParticipantsFileName)))
            {
                string errMsg = $"{Defaults.ParticipantsFileName} could not be found in {this.PathToHistoricalData}.";
                throw new ArgumentException(errMsg);
            }

            if (!File.Exists(Path.Combine(this.PathToHistoricalData, Defaults.PairingHistoryFileName)))
            {
                string errMsg = $"{Defaults.PairingHistoryFileName} could not be found in {this.PathToHistoricalData}.";
                throw new ArgumentException(errMsg);
            }

            if (!Directory.Exists(this.OutputPath))
            {
                Console.WriteLine($"Output directory '{this.OutputPath}' does not exist. Creating directory.");
                Directory.CreateDirectory(this.OutputPath);
            }
        }

        /// <summary>
        /// Generate list of Pairing Entries for the set of participants that are
        /// in the specified session. Use the pairingHistories to make sure no one
        /// is paired with the same person twice.
        /// </summary>
        /// <param name="sessionId">Session Id used to identify participants to pair</param>
        /// <param name="participants">List of all participants</param>
        /// <param name="pairingHistories">List of pairing history entries that say who all a participant was paired with in the past </param>
        /// <returns></returns>
        public IEnumerable<PairingEntry> Generate(
            string sessionId, 
            IEnumerable<Participant> participants, 
            Dictionary<string, PairingHistory> pairingHistories)
        {
            IEnumerable<Participant> sessionParticipants = FindAllParticipantsInSession(sessionId, participants);
            ValidateEvenNumberParticipants(sessionParticipants);
            
            // Generate pairings until there is one without collisions
            IEnumerable<PairingEntry> candidatePairing;

            do {
                candidatePairing = GenerateRandomPairings(sessionParticipants);
            } while (!ValidatePairing(candidatePairing, pairingHistories));
            
            return candidatePairing;
        }

        /// <summary>
        /// Check that participant # is even. Throw exception otherwise
        /// </summary>
        /// <param name="sessionParticipants">list of participants to pair</param>
        private void ValidateEvenNumberParticipants(IEnumerable<Participant> sessionParticipants)
        {
            if (sessionParticipants.Count() % 2 != 0)
            {
                StringBuilder exceptionMsg = new StringBuilder();
                int i = 0;
                exceptionMsg.AppendLine("Odd numbers of participants can't be paired!");
                exceptionMsg.AppendLine("Participants: ");
                foreach (Participant participant in sessionParticipants)
                {
                    exceptionMsg.AppendLine($"{i}: {participant.email}");
                }

                throw new Exception(exceptionMsg.ToString());
            }
        }

        /// <summary>
        /// Validate that pairs have never been paired up before using the pairing histories for reference
        /// </summary>
        /// <param name="candidatePairing">List of Pairing Entries</param>
        /// <param name="pairingHistory">List of PairingHistory entries</param>
        /// <returns></returns>
        public bool ValidatePairing(
            IEnumerable<PairingEntry> candidatePairing, 
            Dictionary<string, PairingHistory> pairingHistory)
        {
            // For each candidate pairing entry, check the pairing history for a duplicate pairing.
            foreach (var pair in candidatePairing)
            {
                PairingHistory history;
                if (pairingHistory.TryGetValue(pair.participant1.email, out history))
                {
                    foreach (var historyEntry in history.history)
                    {
                        if (historyEntry.buddy_email.Equals(pair.participant2.email, StringComparison.OrdinalIgnoreCase))
                        {
                            // Pairing conflict found
                            return false;
                        }
                    }
                }

                // This technically should not be possible if the data is correct but adding just in case.
                if (pairingHistory.TryGetValue(pair.participant2.email, out history))
                {
                    foreach (var historyEntry in history.history)
                    {
                        if (historyEntry.buddy_email.Equals(pair.participant1.email, StringComparison.OrdinalIgnoreCase))
                        {
                            // Pairing conflict found
                            return false;
                        }
                    }
                }
            }
            
            // No pairing conflict found!
            return true;
        }

        /// <summary>
        /// Shuffles a IList<T> inplace
        /// </summary>
        /// <param name="list">List to shuffle</param>
        /// <param name="rng">Random number generator</param>
        /// <typeparam name="T">Type of list</typeparam>
        /// <returns>IEnumerable of the shuffled list</returns>
        public static IEnumerable<T> ShuffleList<T>(IList<T> list, Random rng) 
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  

            return list;
        }

        /// <summary>
        /// Generate random paris of partipants
        /// </summary>
        /// <param name="sessionParticipants"></param>
        /// <returns>List of Pairing Entries</returns>
        public IEnumerable<PairingEntry> GenerateRandomPairings(
            IEnumerable<Participant> sessionParticipants)
        {
            // Assign rand value to each session participant
            List<Participant> participants = sessionParticipants.ToList<Participant>();
            
            // Error if participants list is not even
            if (participants.Count() % 2 != 0)
            {
                throw new Exception("Cannot generate pairing for an odd number of participants");
            }

            participants = ShuffleList<Participant>(participants, this.rng).ToList();
            
            List<PairingEntry> pairings = new List<PairingEntry>();
            for (int i = 0; i < participants.Count() - 1; i += 2)
            {
                PairingEntry entry = new PairingEntry();
                entry.participant1 = participants[i];
                entry.participant2 = participants[i+1];
                pairings.Add(entry);
            }

            return pairings;
        }

        /// <summary>
        /// Filter down partipant list to just those who are part of the specified session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="participants"></param>
        /// <returns></returns>
        public IEnumerable<Participant> FindAllParticipantsInSession(
            string sessionId, 
            IEnumerable<Participant> participants)
        {
            return participants.Where(
                p => p.session_participated.Any(
                    s => s.Equals(sessionId, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}