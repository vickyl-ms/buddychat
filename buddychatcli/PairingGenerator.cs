using System;
using System.Collections.Generic;
using System.Linq;

namespace BuddyChatCLI
{
    public class PairingEntry
    {
        public Participant participant1;
        public Participant participant2;
    }

    public class PairingGenerator
    {
        private Random rng = new Random();  

        private string sessionId;

        private string pathToHistoricalData;

        private string outputPath;

        // Takes in commandline options and returns a success int code
        public static int ExecutePairingGenerator(CommandLineOptions options)
        {
            return -1;
        }

        // Inputs
        // - location of participant and history json - default to current dir or current dir + session id
        // - session id
        // - output location - default to session id + number of tries
        public PairingGenerator(string sessionId = "", string pathToHistoricalData = "", string outputPath = "")
        {
            this.sessionId = sessionId;
            this.pathToHistoricalData = pathToHistoricalData;
            this.outputPath = outputPath;
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