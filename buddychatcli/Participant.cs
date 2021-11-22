using System.Collections.Generic;
using System;
using System.Text;

namespace BuddyChatCLI
{
    public class Participant : IEquatable<Participant>
    {
        // participant full name
        public string name { get; set; }
        public string first_name => name.Split(' ')[0];

        // participant email
        public string email { get; set; }
        
        // participant data
        public IDictionary<string, string> data { get; set; }

        // list of all the sessions the participant had participated
        public IList<string> session_participated { get; set; }

        public override string ToString()
        {
            return "Name: " + name + "; Email: " + email;
        }

        public string ToDetailedString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine($"Name: {name}");
            output.AppendLine($"Email: {email}");

            if(session_participated?.Count > 0)
            {
                output.Append("Sessions: ");
                output.AppendJoin(", ", session_participated);
                output.AppendLine();
            }

            foreach (KeyValuePair<string, string> kvp in data)
            {
                output.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            return output.ToString();
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("Email cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Name cannot be empty.");
            }

            if (this.session_participated != null)
            {
                HashSet<string> set = new HashSet<string>();
                foreach(string s in this.session_participated)
                {
                    if (!set.Add(s))
                    {
                        throw new Exception($"Participated session contains sessionId '{s}' more than once.");
                    }
                }
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

        public void AddSession(string sessionId)
        {
            if (this.session_participated == null)
            {
                this.session_participated = new List<string>();
            }

            if (this.session_participated.Contains(sessionId))
            {
                throw new Exception("Participated Session already contains sessionId: " + sessionId);
            }
            
            session_participated.Insert(0, sessionId);
        }
    }
}

