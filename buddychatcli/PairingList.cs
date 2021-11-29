using System.Collections.Generic;

namespace BuddyChatCLI
{
    /// <summary>
    /// Pairing Generator returns a pairing list
    /// </summary>
    public class PairingList
    {
        public string sessionId;

        /// <summary>
        /// Pairing Generator returns a list of Pairing Entries
        /// </summary>
        public struct Entry
        {
            public string participant1Name;
            public string participant1Email;
            public string participant2Name;
            public string participant2Email;
        }

        /// <summary>
        /// List of PairingEntry for this session
        /// </summary>
        public List<Entry> pairings;
    }
}
