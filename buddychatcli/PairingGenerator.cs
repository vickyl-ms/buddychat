using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BuddyChatCLI
{
    public class PairingEntry
    {
        public Participant participant1;
        public Participant participant2;
    }

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

        // Validate & generate default path values
        // read in data
        public IEnumerable<PairingEntry> Generate()
        {
            return null;
        }

        // Generate pairs:
        // Inputs are:
        //  - Participant data json
        //  - Pairing history json
        //  - Session Id
        public IEnumerable<PairingEntry> Generate(
            string sessionId, 
            IEnumerable<Participant> participants, 
            Dictionary<string, PairingHistory> pairingHistories)
        {
            IEnumerable<Participant> sessionParticipants = FindAllParticipantsInSession(sessionId, participants);
            
            IEnumerable<PairingEntry> candidatePairing;

            do {
                candidatePairing = GenerateRandomPairings(sessionParticipants);
            } while (ValidatePairing(candidatePairing, pairingHistories));
            
            return candidatePairing;
        }

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
            }
            
            // No pairing conflict found!
            return true;
        }

        // function on the internet for shuffling a list in place
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

        private IEnumerable<PairingEntry> GenerateRandomPairings(
            IEnumerable<Participant> sessionParticipants)
        {
            // Assign rand value to each session participant
            List<Participant> participants = sessionParticipants.ToList<Participant>();
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

        public IEnumerable<Participant> FindAllParticipantsInSession(
            string sessionId, 
            IEnumerable<Participant> participants)
        {
            return participants.Where(p => p.session_participated.Contains(sessionId));
        }
    }
}