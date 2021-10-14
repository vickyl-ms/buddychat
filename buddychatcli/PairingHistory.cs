using System.Collections.Generic;

namespace BuddyChatCLI
{
    // This class represents the pairing history for a participant.
    public class PairingHistory
    {
        public string email;

        public struct PairingHistoryEntry
        {
            public string buddy_email;
            public string sessionId;
        }

        // List of Pairing History entries that represent the buddy someone
        // was paired with for each session
        public List<PairingHistoryEntry> history;
    }
}
