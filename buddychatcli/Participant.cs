using System.Collections.Generic;

namespace BuddyChatCLI
{
    public class Participant
    {
        // uniquely identify a participant
        public int participant_id { get; set; }
        
        // list of all the sessions the participant had participated
        public List<string> session_participated { get; set; }
        
        // participant full name
        public string name { get; set; }
        
        // participant email
        public string email { get; set; }
        
        // participant data
        public Dictionary<string, string> data { get; set; }
    }
}

