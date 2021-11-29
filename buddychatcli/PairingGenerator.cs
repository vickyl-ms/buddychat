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
    /// Generates random pairs of participants, making sure that each pair has never
    /// been paired before.
    /// </summary>
    [Verb("CreatePairings", HelpText = "Creates the random assignments.")]
    public class PairingGenerator
    {
        private Random rng = new Random();  

        [Option(shortName: 's',
                longName: "SessionId",
                Required = true,
                HelpText = "The id for this specific session.")]
        public string SessionId { get; set; }

        [Option(shortName: 'p',
                longName: "ParticipantsFile",
                Required = false,
                HelpText = "The location of the participant json file. Default is participants.json in current directory.")]
        public string ParticipantsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.ParticipantsFileName);

        [Option(shortName: 'h',
                longName: "PairingHistoryFile",
                Required = false,
                HelpText = "The location of participant and pairing history json. Default is PairingHistory.json in the current directory.")]
        public string PairingHistoryFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.PairingHistoryFileName);

        [Option(shortName: 'n',
                longName: "NewPairingsFile",
                Required = false,
                HelpText = "Output location for new pairings file. Default is pairings.json in current directory.")]
        public string NewPairingsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.NewPairingFileName);

        /// <summary>
        /// Validate and generate default options. Read in historical data.
        /// </summary>
        /// <returns>List of Pairing entries</returns>
        public int Execute()
        {
            ValidateOptions();
            
            // Read in participants file
            string participantsFileContent = File.ReadAllText(this.ParticipantsFile);
            IEnumerable<Participant> participants = JsonConvert.DeserializeObject<List<Participant>>(participantsFileContent);

            // Read in pairing history file if there is one
            IDictionary<string, PairingHistory> pairingHistories = new Dictionary<string, PairingHistory>();
            if (File.Exists(this.PairingHistoryFile))
            {
                string pairingHistoryFileContent = File.ReadAllText(this.PairingHistoryFile);
                pairingHistories = JsonConvert.DeserializeObject<Dictionary<string, PairingHistory>>(pairingHistoryFileContent);
            }

            PairingList pairings = Generate(this.SessionId, participants, pairingHistories);

            ValidatePairing(pairings.pairings, pairingHistories);
            
            WritePairingsToFile(pairings, this.NewPairingsFile);
            
            return Convert.ToInt32(ReturnCode.Success);
        }

        /// <summary>
        /// Write out list of Pairing Entries to file
        /// </summary>
        private void WritePairingsToFile(PairingList pairings, string newPairingsFile)
        {
            Console.WriteLine($"Writing output to {newPairingsFile}.");

            if (File.Exists(newPairingsFile))
            {
                Console.Out.WriteLine($"Output file '{newPairingsFile}' already exists. Overwrite? Y/N");
                if (!Program.AskUserShouldOverwrite())
                {
                    Console.Out.WriteLine("Not overwriting file. Exiting."); 
                    return;
                }
            }

            File.WriteAllText(
                newPairingsFile,
                JsonConvert.SerializeObject(pairings, Formatting.Indented));
        }

        private void ValidateOptions()
        {
            if (String.IsNullOrWhiteSpace(this.SessionId))
            {
                throw new ArgumentException("Session id is required!");
            }

            if (!File.Exists(this.ParticipantsFile))
            {
                string errMsg = $"No '{this.ParticipantsFile}' found. File is required.";
                throw new ArgumentException(errMsg);
            }

            if (!File.Exists(this.PairingHistoryFile))
            {
                ConsoleColor saved = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLine($"No '{this.PairingHistoryFile}' found. Assuming there is no pairing history.");
                Console.ForegroundColor = saved;
            }

            string outputDirectory = Path.GetDirectoryName(this.NewPairingsFile);
            if (!Directory.Exists(outputDirectory))
            {
                Console.WriteLine($"Output directory '{outputDirectory}' does not exist. Creating directory.");
                Directory.CreateDirectory(outputDirectory);
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
        public PairingList Generate(
            string sessionId, 
            IEnumerable<Participant> participants, 
            IDictionary<string, PairingHistory> pairingHistories)
        {
            IEnumerable<Participant> sessionParticipants = FindAllParticipantsInSession(sessionId, participants);
            if (!sessionParticipants.Any())
            {
                throw new Exception($"No participants found for session '{sessionId}'");
            }

            ValidateEvenNumberParticipants(sessionParticipants);
            Console.WriteLine($"{sessionParticipants.Count()} participants found for session '{sessionId}'.");

            // Strip participant data to just their name & email
            sessionParticipants = sessionParticipants.Select(p => new Participant{ name = p.name, email = p.email});

            // Identify all potential buddies for each participant
            IDictionary<string, IList<string>> potentialBuddyMap = GetAllPotentialBuddyEmails(sessionParticipants, pairingHistories);

            return GeneratePairings(potentialBuddyMap, sessionParticipants, sessionId);
        }

        private PairingList GeneratePairings(
            IDictionary<string, IList<string>> potentialBuddyMap, 
            IEnumerable<Participant> sessionParticipants,
            string sessionId)
        {
            PairingList pairingList = new PairingList {
                sessionId = SessionId,
                pairings = new List<PairingList.Entry>()
            };

            // Shuffle list of potential buddies
            foreach (string key in potentialBuddyMap.Keys)
            {
                potentialBuddyMap[key] = ShuffleList(potentialBuddyMap[key], this.rng);
            }

            // Convert dictionary to a list sorted by number of potential buddies
            IList<KeyValuePair<string, IList<string>>> sortedPotentialBuddiesList = potentialBuddyMap.OrderBy(kvp => kvp.Value.Count).ToList();

            HashSet<string> pairedParticipants = new HashSet<string>();
            Stack<Tuple<int, int>> stack = new Stack<Tuple<int, int>>();
            //IEnumerator<KeyValuePair<string, IList<string>>> enumerator = sortedPotentialBuddiesList.GetEnumerator();
            Tuple<int, int> currentNode = new Tuple<int, int>(0, 0);

            // For each person not already paired, pick the first available buddy from the randomized list.
            // Loop until we've gone through each KVP in sortedPotentialBuddiesList
            do
            {
                // First handle the case where the buddy to pair has already been chosen by a previous
                // participant on the list.
                if (pairedParticipants.Contains(sortedPotentialBuddiesList[currentNode.Item1].Key))
                {
                    if (currentNode.Item2 != 0)
                    {
                        throw new Exception("This check should only ever be true when looking at a new KVP");
                    }

                    // this buddy has already been paired up. Increment current node and keep looking at next KVP
                    currentNode = new Tuple<int, int>(currentNode.Item1 + 1, 0);
                    continue;
                }

                
                // Next, handle the case where this potential buddy has already been claimed. 
                // Move on to next node but check if we've reached the end of the list. 
                // If so, backtrack with the stack
                IList<string> potentialBuddies = sortedPotentialBuddiesList[currentNode.Item1].Value;
                string potentialBuddy = potentialBuddies[currentNode.Item2];

                if (pairedParticipants.Contains(potentialBuddy))
                {
                    while (currentNode.Item2 + 1 == sortedPotentialBuddiesList[currentNode.Item1].Value.Count)
                    {
                        // There are no options to backtrack to
                        if (stack.Count == 0)
                        {
                            throw new Exception("No valid pairings could be found.");
                        }

                        currentNode = stack.Pop();
                        pairedParticipants.Remove(sortedPotentialBuddiesList[currentNode.Item1].Key);
                        pairedParticipants.Remove(sortedPotentialBuddiesList[currentNode.Item1].Value[currentNode.Item2]);
                    }

                    currentNode = new Tuple<int, int>(currentNode.Item1, currentNode.Item2 + 1);
                    continue;
                }

                // Claim this potential buddy and update stack and set of claimed buddies and current Node
                stack.Push(currentNode);
                pairedParticipants.Add(sortedPotentialBuddiesList[currentNode.Item1].Key);
                pairedParticipants.Add(sortedPotentialBuddiesList[currentNode.Item1].Value[currentNode.Item2]);

                currentNode = new Tuple<int, int>(currentNode.Item1 + 1, 0);

            } while (currentNode.Item1 < sortedPotentialBuddiesList.Count);

            int i = 0;

            Console.WriteLine();
            Console.WriteLine("New pairings:");
            foreach (Tuple<int, int> node in stack)
            {
                string p1Email = sortedPotentialBuddiesList[node.Item1].Key;
                string p1Name = sessionParticipants.First(p => p.email.Equals(p1Email, StringComparison.InvariantCultureIgnoreCase)).name;
                string p2Email = sortedPotentialBuddiesList[node.Item1].Value[node.Item2];
                string p2Name = sessionParticipants.First(p => p.email.Equals(p2Email, StringComparison.InvariantCultureIgnoreCase)).name;

                PairingList.Entry entry = new PairingList.Entry {
                    participant1Email = p1Email,
                    participant1Name = p1Name,
                    participant2Email = p2Email,
                    participant2Name = p2Name
                };
                    
                pairingList.pairings.Add(entry);
                Console.WriteLine($"\t{++i}: '{entry.participant1Email}' and '{entry.participant2Email}'");
            }

            return pairingList;
        }

        private IDictionary<string, IList<string>> GetAllPotentialBuddyEmails(IEnumerable<Participant> sessionParticipants, IDictionary<string, PairingHistory> pairingHistories)
        {
            IDictionary<string, IList<string>> potentialBuddyMap = new Dictionary<string, IList<string>>();
            IList<string> sessionParticipantEmails = sessionParticipants.Select(p => p.email).ToList();
            int i = 0;

            Console.WriteLine();
            Console.WriteLine("Potential buddies:");
            
            foreach (Participant p in sessionParticipants)
            {
                // Get pairing history for p if available
                PairingHistory history;
                pairingHistories.TryGetValue(p.email.ToLowerInvariant(), out history);

                // Get potential buddies by filtering out p itself and anyone on p's pairing history
                List<string> potentialBuddies = sessionParticipantEmails
                    .Where(email => !email.Equals(p.email, StringComparison.InvariantCultureIgnoreCase) && 
                                (history == null || !history.WasBuddyWith(email))).ToList();

                // Error out if no potential buddies
                if (potentialBuddies.Count == 0)
                {
                    StringBuilder errMsg = new StringBuilder();
                    errMsg.AppendLine($"No possible buddies found for '{p.name} ({p.email})'.");
                    errMsg.AppendLine("All previous buddies:");
                    if (history != null)
                    {
                        errMsg.Append("\t");
                        errMsg.AppendJoin("\n\t", history.GetAllPreviousBuddyEmails());
                    }
                    else
                    {
                        errMsg.AppendLine("\tNo pairing history.");
                    }
                    
                    errMsg.AppendLine("All participants:");
                    errMsg.Append("\t");
                    errMsg.AppendJoin("\n\t", sessionParticipants);

                    throw new Exception(errMsg.ToString());
                }

                string logMsg = $"{++i}. {p.email}: " + (history != null? string.Join(", ", potentialBuddies) : "All");
                Console.WriteLine(logMsg);

                potentialBuddyMap.Add(p.email.ToLowerInvariant(), potentialBuddies);
            }

            return potentialBuddyMap;
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
                    exceptionMsg.AppendLine($"{++i}: {participant.email}");
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
        public static bool ValidatePairing(
            IEnumerable<PairingList.Entry> candidatePairing, 
            IDictionary<string, PairingHistory> pairingHistory)
        {
            // For each candidate pairing entry, check the pairing history for a duplicate pairing.
            foreach (var pair in candidatePairing)
            {
                PairingHistory history;
                if (pairingHistory.TryGetValue(pair.participant1Email.ToLowerInvariant(), out history))
                {
                    foreach (var historyEntry in history.history)
                    {
                        if (historyEntry.buddy_email.Equals(pair.participant2Email, StringComparison.OrdinalIgnoreCase))
                        {
                            // Pairing conflict found
                            return false;
                        }
                    }
                }

                // This technically should not be possible if the data is correct but adding just in case.
                if (pairingHistory.TryGetValue(pair.participant2Email.ToLowerInvariant(), out history))
                {
                    foreach (var historyEntry in history.history)
                    {
                        if (historyEntry.buddy_email.Equals(pair.participant1Email, StringComparison.OrdinalIgnoreCase))
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
        public static IList<T> ShuffleList<T>(IList<T> list, Random rng) 
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

        // /// <summary>
        // /// Generate random pairs of partipants
        // /// </summary>
        // /// <param name="sessionParticipants"></param>
        // /// <returns>List of Pairing Entries</returns>
        // public IEnumerable<PairingEntry> GenerateRandomPairings(
        //     IEnumerable<Participant> sessionParticipants)
        // {
        //     // Assign rand value to each session participant
        //     List<Participant> participants = sessionParticipants.ToList<Participant>();
            
        //     // Error if participants list is not even
        //     if (participants.Count() % 2 != 0)
        //     {
        //         throw new Exception("Cannot generate pairing for an odd number of participants");
        //     }

        //     participants = ShuffleList<Participant>(participants, this.rng).ToList();
            
        //     List<PairingEntry> pairings = new List<PairingEntry>();
        //     for (int i = 0; i < participants.Count() - 1; i += 2)
        //     {
        //         PairingEntry entry = new PairingEntry();
        //         entry.participant1Name = participants[i].name;
        //         entry.participant1Email = participants[i].email;
        //         entry.participant2Name = participants[i+1].name;
        //         entry.participant2Email = participants[i+1].email;
        //         pairings.Add(entry);
        //     }

        //     return pairings;
        // }

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