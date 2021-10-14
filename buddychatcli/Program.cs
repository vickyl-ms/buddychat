using System;
using CommandLine;
using System.Threading.Tasks;
using buddychatcli;
using System.Collections.Generic;

namespace BuddyChatCLI
{
    class Program
    {
        public static readonly string SignUpFileName = "signup.csv";
        public static readonly string ParticipantsFileName = "participants.json";
        public static readonly string PairingHistoryFileName = "pairing_history.json";

        static int Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            CommandLineOptions options = new CommandLineOptions();
            if (!options.ParseCommandline(args))
            {
                return -1;
            }

            Console.WriteLine("Command: " + options.Command);

            Participant participant1 = new Participant()
            {
                email = "user1@microsoft.com",
                name = "User1 LastName1",
                data = new Dictionary<string, string>()
                {
                    { "introduction", "Hi" },
                    { "pronouns", "He\\Him\\His" },
                    { "question1", "Question 1 for participant 1" },
                    { "answer1", "Answer 1 for participant 1" },
                    { "question2", "Question 2 for participant 1" },
                    { "answer2", "Answer 2 for participant 1" },
                    { "question3", "Question 3 for participant 1" },
                    { "answer3", "Answer 3 for participant 1" },
                }
            };

            Participant participant2 = new Participant()
            {
                email = "user2@microsoft.com",
                name = "User2 LastName2",
                data = new Dictionary<string, string>()
                {
                    { "introduction", "Hi 2" },
                    { "pronouns", "She\\Her\\Hers" },
                    { "question1", "Question 1 for participant 2" },
                    { "answer1", "Answer 1 for participant 2" },
                    { "question2", "Question 2 for participant 2" },
                    { "answer2", "Answer 2 for participant 2. No question 3 for this participant :)" }
                }
            };

            EmailGenerator emailGenerator = new EmailGenerator("C:\\Users\\isleal\\Documents\\BuddyChatTemplate.oft");
            emailGenerator.GenerateEmail(participant1, participant2);

            return 0;
        }
    }
}
