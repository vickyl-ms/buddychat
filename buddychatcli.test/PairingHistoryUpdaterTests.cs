using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace BuddyChatCLI.test
{
    public class PairingHistoryUpdaterTests
    {
        public PairingHistoryUpdaterTests()
        {
            // set current directory to test data dir
            string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
            Directory.SetCurrentDirectory(testDataPath);
        }

        [Fact]
        public void Execute_ReadFromFiles()
        {
            PairingHistoryUpdater updater = new PairingHistoryUpdater {
                NewPairingsFile = Defaults.NewPairingFileName,
                PairingHistoryFile = Defaults.PairingHistoryFileName,
                UpdatedPairingHistoryFile = "updated_" + Defaults.PairingHistoryFileName
            };

            if (File.Exists(updater.UpdatedPairingHistoryFile))
            {
                File.Delete(updater.UpdatedPairingHistoryFile);
            }
            
            updater.Execute();

            string updatedJson = File.ReadAllText(updater.UpdatedPairingHistoryFile);
            IDictionary<string, PairingHistory> updatedPairings = 
                JsonConvert.DeserializeObject<IDictionary<string, PairingHistory>>(updatedJson);

            Assert.Equal(5, updatedPairings.Count);

            PairingHistory history1 = updatedPairings["participant1@email.com"];
            Assert.Equal(2, history1.history.Count);
            Assert.Equal("participant2@email.com", history1.history[0].buddy_email);
            Assert.Equal("New", history1.history[0].sessionId);
            Assert.Equal("participant5@email.com", history1.history[1].buddy_email);
            Assert.Equal("old", history1.history[1].sessionId);

            PairingHistory history2 = updatedPairings["participant2@email.com"];
            Assert.Equal(2, history2.history.Count);
            Assert.Equal("participant1@email.com", history2.history[0].buddy_email, ignoreCase: true);
            Assert.Equal("New", history2.history[0].sessionId);
            Assert.Equal("participant5@email.com", history2.history[1].buddy_email, ignoreCase: true);
            Assert.Equal("old", history2.history[1].sessionId);            // Expect participant 1 and 2 to be paired and 4 and 5 to be paired.

            PairingHistory history6 = updatedPairings["participant6@email.com"];
            Assert.Single(history6.history);
            Assert.Equal("participant3@email.com", history6.history[0].buddy_email, ignoreCase: true);
            Assert.Equal("New", history6.history[0].sessionId);

           // Cleanup test file
            File.Delete(updater.UpdatedPairingHistoryFile);
        }
    }
}