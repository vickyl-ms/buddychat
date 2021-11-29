using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace BuddyChatCLI.test
{
    public class ParticipantUpdaterTests
    {
        [Fact]
        public static void MergeNewSignupWithHistoricalDataTest()
        {
            Participant historical1 = new Participant {
                email = "user1@a.com",
                name = "first1 last1",
                session_participated = new List<string> {"oldsession"},
                data = new Dictionary<string, string> {
                    {"key1", "value1"}
                }
            };

            Participant historical2 = new Participant {
                email = "user2@a.com",
                name = "first2 last2",
                session_participated = new List<string> {"oldsession"},
                data = new Dictionary<string, string> {
                    {"key1", "value1"}
                }
            };

            IList<Participant> historicalParticipants = new List<Participant> {historical1, historical2};

            Participant signup1 = new Participant {
                email = "user2@a.com",
                name = "first2 last2",
                data = new Dictionary<string, string> {
                    {"key2", "value2"}
                }
            };

            Participant signup2 = new Participant {
                email = "user3@a.com",
                name = "first3 last3",
                data = new Dictionary<string, string> {
                    {"key1", "value1"}
                }
            };

            IList<Participant> newParticipants = new List<Participant> {signup1, signup2};

            IList<Participant> updatedParticipants = ParticipantUpdater.MergeNewSignupWithHistoricalData(
                historicalParticipants: historicalParticipants, newParticipants: newParticipants, "newsession");

            Assert.Equal(3, updatedParticipants.Count);
            Assert.Equal(
@"Name: first1 last1
Email: user1@a.com
Sessions: oldsession
key1: value1
",
                        updatedParticipants[0].ToDetailedString());

            Assert.Equal(
@"Name: first2 last2
Email: user2@a.com
Sessions: newsession, oldsession
key2: value2
",
                        updatedParticipants[1].ToDetailedString());

            Assert.Equal(
@"Name: first3 last3
Email: user3@a.com
Sessions: newsession
key1: value1
",
                        updatedParticipants[2].ToDetailedString());
        }
    }
}
