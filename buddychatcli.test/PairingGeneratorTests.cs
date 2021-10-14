using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BuddyChatCLI.test
{
    public class PairingGeneratorTests
    {
        public IList<Participant> testParticipants = new List<Participant>() {
            
            // no session participant
            new Participant {
                email = "participant1@email.com",
                session_participated = new List<string>(),
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 1"}
                }
            },
            // not current session participant
            new Participant {
                email = "participant2@email.com",
                session_participated = new List<string>{"old", "older", "oldest"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 2"}
                }
            },
            // current session participant
            new Participant {
                email = "participant3@email.com",
                session_participated = new List<string>{"current"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 3"}
                }
            },
            // current and old session participant
            new Participant {
                email = "participant4@email.com",
                session_participated = new List<string>{"old", "older", "oldest", "current"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 4"}
                }
            },
            // another current session participant
            new Participant {
                email = "participant5@email.com",
                session_participated = new List<string>{"old", "current", "oldest"},
                data = new Dictionary<string, string>() {
                    { "intro", "Hi my name is participant 5"}
                }
            }
        };

        [Fact]
        public void FindAllParticipantsInSession_currentsession()
        {
            String[] expectedParticipants = {"participant3@email.com", "participant4@email.com", "participant5@email.com"};

            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> actualParticipants = generator.FindAllParticipantsInSession("current", testParticipants);

            Assert.Equal(3, actualParticipants.Count());
            Assert.All(actualParticipants, participant => Assert.Contains(participant.email, expectedParticipants));
        }

        [Fact]
        public void FindAllParticipantsInSession_nonexistentsession()
        {
            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> actualParticipants = generator.FindAllParticipantsInSession("nonexistentsession", testParticipants);

            Assert.Empty(actualParticipants);
        }
    }
}