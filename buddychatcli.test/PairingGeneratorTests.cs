using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace BuddyChatCLI.test
{
    public class PairingGeneratorTests
    {
        public PairingGeneratorTests()
        {
            // set current directory to test data dir
            string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
            Directory.SetCurrentDirectory(testDataPath);
        }

        [Fact]
        public void FindAllParticipantsInSession_CurrentSession()
        {
            String[] expectedParticipants = {"participant1@email.com", "participant2@email.com", "participant4@email.com", "participant5@email.com"};

            string fileContent = File.ReadAllText(Defaults.ParticipantsFileName);
            IEnumerable<Participant> allParticipants = JsonConvert.DeserializeObject<IEnumerable<Participant>>(fileContent);

            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> actualParticipants = generator.FindAllParticipantsInSession("current", allParticipants);

            Assert.Equal(4, actualParticipants.Count());
            Assert.All(actualParticipants, participant => Assert.Contains(participant.email, expectedParticipants));
        }

        [Fact]
        public void FindAllParticipantsInSession_NonExistentSession()
        {

            string fileContent = File.ReadAllText(Defaults.ParticipantsFileName);
            IEnumerable<Participant> allParticipants = JsonConvert.DeserializeObject<IEnumerable<Participant>>(fileContent);

            PairingGenerator generator = new PairingGenerator();
            IEnumerable<Participant> actualParticipants = generator.FindAllParticipantsInSession("nonexistentsession", allParticipants);

            Assert.Empty(actualParticipants);
        }

        [Fact]
        public void Generate_ThrowsWithOddNumberParticipants()
        {
            string fileContent = File.ReadAllText(Defaults.ParticipantsFileName);
            IEnumerable<Participant> allParticipants = JsonConvert.DeserializeObject<IEnumerable<Participant>>(fileContent);

            PairingGenerator generator = new PairingGenerator();
            Assert.Throws<Exception>(() => generator.Generate(sessionId: "old", allParticipants, new Dictionary<string, PairingHistory>()));
        }

        [Fact]
        public void Execute_ReadFromFiles()
        {
            PairingGenerator generator = new PairingGenerator {
                ParticipantsFile = Defaults.ParticipantsFileName,
                PairingHistoryFile = Defaults.PairingHistoryFileName,
                SessionId = "Current"
            };

            string expectedOutputFile = Path.Combine(Directory.GetCurrentDirectory(), Defaults.NewPairingFileName);
            if (File.Exists(expectedOutputFile))
            {
                File.Delete(expectedOutputFile);
            }

            generator.Execute();

            string actualPairingsContent = File.ReadAllText(expectedOutputFile);
            PairingList pairings = JsonConvert.DeserializeObject<PairingList>(actualPairingsContent);

            // Expect participant 1 and 2 to be paired and 4 and 5 to be paired.
            foreach (PairingList.Entry pair in pairings.pairings)
            {
                if (pair.participant1Email == "participant1@email.com")
                {
                    Assert.Equal("participant2@email.com", pair.participant2Email);
                }
                else if (pair.participant1Email == "participant2@email.com")
                {
                    Assert.Equal("participant1@email.com", pair.participant2Email);
                } 
                else if (pair.participant1Email == "participant4@email.com")
                {
                    Assert.Equal("participant5@email.com", pair.participant2Email);
                } else {
                    Assert.Equal("participant5@email.com", pair.participant1Email);
                    Assert.Equal("participant4@email.com", pair.participant2Email);
                }
            }

            // Cleanup test file
            File.Delete(expectedOutputFile);
        }
    }
}