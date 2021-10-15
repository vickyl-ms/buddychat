using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BuddyChatCLI.test
{
    public class PairingGeneratorTests
    {
        protected IList<Participant> testParticipants;

        protected Dictionary<string, PairingHistory> testPairingHistory;

        public PairingGeneratorTests()
        {
            // no session participant
            Participant participant1 = new Participant {
                email = "participant1@email.com",
                session_participated = new List<string>(),
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 1"}
                }
            };

            // not current session participant
            Participant participant2 = new Participant {
                email = "participant2@email.com",
                session_participated = new List<string>{"old", "older", "oldest"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 2"}
                }
            };

            // current session participant
            Participant participant3 = new Participant {
                email = "participant3@email.com",
                session_participated = new List<string>{"current"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 3"}
                }
            };
            
            // current and old session participant
            Participant participant4 = new Participant {
                email = "participant4@email.com",
                session_participated = new List<string>{"old", "older", "oldest", "current"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 4"}
                }
            };
            
            // another current session participant
            Participant participant5 = new Participant {
                email = "participant5@email.com",
                session_participated = new List<string>{"old", "current", "oldest"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 5"}
                }
            };

            // another current session participant
            Participant participant6 = new Participant {
                email = "participant6@email.com",
                session_participated = new List<string>{"current", "older", "oldest"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 6"}
                }
            };

            testParticipants = new List<Participant>() 
            {
                participant1,
                participant2,
                participant3,
                participant4,
                participant5,
                participant6
            };

            // 3 can pair with anyone
            // 4 can only pair with 5
            // 5 can pair with 3 or 4
            // 6 can only pair with 3
            // Only possible pairing is 3 with 6 and 4 with 5
            testPairingHistory = new Dictionary<string, PairingHistory>
            {
                // Entry for participant not in 'current' session
                { 
                    participant2.email,
                    new PairingHistory 
                    {
                        email = participant2.email,
                        history = new List<PairingHistory.PairingHistoryEntry>
                        {
                            new PairingHistory.PairingHistoryEntry {
                                buddy_email = participant4.email,
                                sessionId = "oldest"
                            }
                        }
                    }
                },
                {
                    participant3.email,
                    // can pair with anyone
                    new PairingHistory 
                    {
                        email = participant3.email,
                        history = new List<PairingHistory.PairingHistoryEntry>()
                    }
                },
                {
                    participant4.email,
                    // can only pair with 5
                    new PairingHistory 
                    {
                        email = participant4.email,
                        history = new List<PairingHistory.PairingHistoryEntry>
                        {
                            new PairingHistory.PairingHistoryEntry {
                                buddy_email = participant3.email,
                                sessionId = "oldest"
                            },
                            new PairingHistory.PairingHistoryEntry {
                                buddy_email = participant6.email,
                                sessionId = "old"
                            }
                        }
                    }
                },
                {
                    participant5.email,
                    // can pair with 3 or 4
                    new PairingHistory 
                    {
                        email = participant5.email,
                        history = new List<PairingHistory.PairingHistoryEntry>
                        {
                            new PairingHistory.PairingHistoryEntry {
                                buddy_email = participant6.email,
                                sessionId = "oldest"
                            }
                        }
                    }
                },
                {
                    participant6.email,
                    // Can only pair with 3
                    new PairingHistory 
                    {
                        email = participant6.email,
                        history = new List<PairingHistory.PairingHistoryEntry>
                        {
                            new PairingHistory.PairingHistoryEntry {
                                buddy_email = participant4.email,
                                sessionId = "oldest"
                            },
                            new PairingHistory.PairingHistoryEntry {
                                buddy_email = participant5.email,
                                sessionId = "old"
                            }

                        }
                    }
                }
            };
        }

        [Fact]
        public void FindAllParticipantsInSession_CurrentSession()
        {
            String[] expectedParticipants = {"participant3@email.com", "participant4@email.com", "participant5@email.com", "participant6@email.com"};

            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> actualParticipants = generator.FindAllParticipantsInSession("current", testParticipants);

            Assert.Equal(4, actualParticipants.Count());
            Assert.All(actualParticipants, participant => Assert.Contains(participant.email, expectedParticipants));
        }

        [Fact]
        public void FindAllParticipantsInSession_NonExistentSession()
        {
            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> actualParticipants = generator.FindAllParticipantsInSession("nonexistentsession", testParticipants);

            Assert.Empty(actualParticipants);
        }

        [Fact]
        public void GenerateRandomPairings_Test()
        {
            String[] expectedParticipants = {"participant3@email.com", "participant4@email.com", "participant5@email.com", "participant6@email.com"};

            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> participants = generator.FindAllParticipantsInSession("current", testParticipants);
            Assert.Equal(4, participants.Count());

            IEnumerable<PairingEntry> pairings = generator.GenerateRandomPairings(participants);
            Assert.Equal(2, pairings.Count());
            Assert.All(pairings, pair => 
                {
                    Assert.Contains(pair.participant1.email, expectedParticipants);
                    Assert.Contains(pair.participant2.email, expectedParticipants);
                });
        }

        [Fact]
        public void GenerateRandomPairings_FailsWithOddParticipants()
        {
            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> participants = generator.FindAllParticipantsInSession("old", testParticipants);
            Assert.Equal(3, participants.Count());
            Assert.Throws<Exception>(() => generator.GenerateRandomPairings(participants));
        }

        [Fact]
        public void Generate_Test()
        {
            PairingGenerator generator = new PairingGenerator();
            IEnumerable<PairingEntry> pairings = generator.Generate(sessionId: "current", testParticipants, testPairingHistory);
            Assert.Equal(2, pairings.Count());
            
            foreach (PairingEntry pair in pairings)
            {
                if (pair.participant1.email == "participant3@email.com")
                {
                    Assert.Equal("participant6@email.com", pair.participant2.email);
                }
                else if (pair.participant1.email == "participant6@email.com")
                {
                    Assert.Equal("participant3@email.com", pair.participant2.email);
                } 
                else if (pair.participant1.email == "participant4@email.com")
                {
                    Assert.Equal("participant5@email.com", pair.participant2.email);
                } else {
                    Assert.Equal("participant5@email.com", pair.participant1.email);
                    Assert.Equal("participant4@email.com", pair.participant2.email);
                }
            }
        }

        [Fact]
        public void Generate_ThrowsWithOddNumberParticipants()
        {
            PairingGenerator generator = new PairingGenerator();
            Assert.Throws<Exception>(() => generator.Generate(sessionId: "old", testParticipants, testPairingHistory));
        }
    }
}