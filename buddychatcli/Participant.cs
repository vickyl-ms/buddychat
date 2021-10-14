using System.Collections.Generic;
using System;

namespace BuddyChatCLI
{
    public class Participant : IEquatable<Participant>
    {
        // uniquely identify a participant
        public int participant_id { get; set; }
        
        // list of all the sessions the participant had participated
        public List<int> session_participated { get; set; }
        
        // participant full name
        public string name { get; set; }
        
        // participant email
        public string email { get; set; }
        
        // participant data
        public Dictionary<string, string> data { get; set; }

        public override string ToString()
        {
            return "Name: " + name + "   Email: " + email;
        }

        public void displayDictionaryData(Dictionary<string, string> data)
        {
            foreach (KeyValuePair<string, string> kvp in data)
            {

                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
        }

        public override bool Equals(object other) 
        {
            return Equals(other as Participant);
        }

        public bool Equals(Participant participant)
        {
            return participant != null && name.Equals(participant.name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}

