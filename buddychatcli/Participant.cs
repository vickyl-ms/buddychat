using System.Collections.Generic;
using System;

namespace BuddyChatCLI
{
    public class Participant : IEquatable<Participant>
    {
        // list of all the sessions the participant had participated
        public List<string> session_participated { get; set; }
        
        // participant full name
        public string name { get; set; }
        public string first_name => name.Split(' ')[0];

        // participant email
        public string email { get; set; }
        
        // participant data
        public Dictionary<string, string> data { get; set; }

        public override string ToString()
        {
            displayDictionaryData(data);
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
            return participant != null && email.Equals(participant.email);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}

