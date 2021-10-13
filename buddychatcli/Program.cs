using System;
using CommandLine;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
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
            options.ParseCommandline(args);

            Console.WriteLine("Command: " + options.Command);

            createParticipantData(SignUpFileName);

            
            return 0;
        }

        // Creating Participant Data using Sign up sheet and new signup information
        private static void createParticipantData (string filename) 
        {
            using (TextFieldParser csvParser = new TextFieldParser(filename))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                List<Person> totalparticipantData = new List<Person>();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    
                    string email = fields[1];
                    string name = fields[2];
                    string pronoun = fields[5];
                    //bool audioChat = Convert.ToBoolean(fields[6]);
                    string intro = fields[7];


                    
                    string question1 = fields[8];
                    string answer1 = fields[9];

                    string question2 = fields[10];
                    string answer2 = fields[11];

                    string question3 = fields[12];
                    string answer3 = fields[13]; 
                    
                    
                    Dictionary<string, string> questionAndAnswers = new Dictionary<string, string>();
                    questionAndAnswers.Add(question1, answer1); 
                    questionAndAnswers.Add(question2, answer2); 
                    questionAndAnswers.Add(question3, answer3); 


                    // Creating Person object
                    Person person = new Person();
                    person.username = email;
                    person.name = name;
                    person.pronoun = pronoun;
                    //person.audioChatOnly = audioChat;
                    person.intro = intro;
                    person.questionAndAnswers = questionAndAnswers;

                    //Console.WriteLine(person);
                    totalparticipantData.Add(person);
                }

                // Display person data
                totalparticipantData.ForEach(item => Console.Write(item));

                // Todo
                // Logic to incorporate new signup information into one sign up sheet

            }
        }
    }
}
