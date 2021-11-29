using System.Collections.Generic;
using System.Linq;

namespace BuddyChatCLI
{
    // This class represents the pairing history for a participant.
    public class PairingHistory
    {
        public string email;

        public struct Entry
        {
            public string buddy_email;
            public string sessionId;
        }

        // List of Pairing History entries that represent the buddy someone
        // was paired with for each session
        public List<Entry> history;

        /// <summary>
        /// Gets a list of emails for all previous buddies
        /// </summary>
        /// <returns>List of emails</returns>
        public IEnumerable<string> GetAllPreviousBuddyEmails()
        {
            return history.Select(h => h.buddy_email);
        }

        /// <summary>
        /// Checks if specified buddy email was ever a paired with this participant
        /// </summary>
        /// <param name="buddyEmail"></param>
        /// <returns></returns>
        public bool WasBuddyWith(string buddyEmail)
        {
            return history.Any(h => h.buddy_email.Equals(buddyEmail, System.StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
