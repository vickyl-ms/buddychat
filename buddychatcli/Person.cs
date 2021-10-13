using System;
using System.Collections.Generic;

using CommandLine;

namespace BuddyChatCLI
{

    public class Person
    {
        
        public string username { get; set; }

        public string name { get; set; }

        public string pronoun { get; set; }

        public bool audioChatOnly { get; set; }

        public string intro { get; set; }

        public Dictionary<string, string> questionAndAnswers { get; set;}

        // Todo: Add the to String function for all
        public override string ToString()
        {
            return "username: " + username + "   Name: " + name + " Pronoun: "+ pronoun + "intro: "+ intro + "Audio chat preference :"+ audioChatOnly;
        }       
    }
}