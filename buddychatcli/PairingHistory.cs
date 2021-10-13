using System.Collections.Generic;

namespace BuddyChatCLI
{
    // This class represents the pairing history for a participant.
    public class PairingHistory
    {
        public string participant_id;

        public struct PairingHistoryEntry
        {
            public string buddy_participant_id;
            public string sessionId;
        }

        // List of Pairing History entries that represent the buddy someone
        // was paired with for each session
        public List<PairingHistoryEntry> history;
    }
}
