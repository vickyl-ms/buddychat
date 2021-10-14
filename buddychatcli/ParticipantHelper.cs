using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System;

namespace BuddyChatCLI
{
    public class ParticipantHelper
    {
        public static readonly string KEY_INTRO = "intro";
        public static readonly string KEY_PRONOUNS = "pronouns";
        public static readonly string STRING_YES_SIGNED_BEFORE = "Yes, I have signed up before.";
        public static readonly string STRING_YES_REUSE = "Yes, please reuse the intro and questions from when I signed up in the past.";
        public static readonly string STRING_NO_NEW_ANSWERS = "No, I want to enter my introduction and new questions and answers.";
        public List<Participant> createParticipantDataFromExistingCSVFile(string filename) 
        {
            List<Participant> totalparticipantData = new List<Participant>();
            using (TextFieldParser csvParser = new TextFieldParser(filename))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                csvParser.ReadLine(); // skipping the 1st line which is the column title

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    string email = fields[1];
                    string name = fields[2];

                    Dictionary<string, string> participantDataDict = new Dictionary<string, string>();
                    
                    if (!string.IsNullOrEmpty(fields[4])){
                        string pronouns = fields[4];
                        participantDataDict.Add(KEY_PRONOUNS, pronouns);
                    }
                    
                    if(!string.IsNullOrEmpty(fields[6])) {
                        string intro = fields[6];
                        participantDataDict.Add(KEY_INTRO, intro);
                    }

                    if (!string.IsNullOrEmpty(fields[7]))
                    {
                        string question1 = fields[7];
                        string answer1 = fields[8];
                        participantDataDict.Add(question1, answer1);
                    }

                    if (!string.IsNullOrEmpty(fields[9]))
                    {
                        string question2 = fields[9];
                        string answer2 = fields[10];
                        participantDataDict.Add(question2, answer2);
                    }

                    if (!string.IsNullOrEmpty(fields[11]))
                    {
                        string question3 = fields[11];
                        string answer3 = fields[12];
                        participantDataDict.Add(question3, answer3);
                    }
                    
                    // Creating Person object
                    Participant person = new Participant();
                    person.email = email;
                    person.name = name;
                    person.data = participantDataDict;
                    person.displayDictionaryData(person.data);

                    Console.WriteLine();

                    //Console.WriteLine(person);
                    totalparticipantData.Add(person);
                }

                // Display person data
                //totalparticipantData.ForEach(item => Console.Write(item));
            }
            return totalparticipantData;
        }

        public void createParticipantDataFromNewSessionCSVFile(string filename, List<Participant> existingParticipants)
        {
            Console.WriteLine(" calling");
            using (TextFieldParser csvParser = new TextFieldParser(filename))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                csvParser.ReadLine(); // skipping the 1st line which is the column title
                int i = 0;
                List<Participant> totalparticipantData = new List<Participant>();
                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    i++;
                    Console.WriteLine(i);
                    string[] fields = csvParser.ReadFields();

                    string email = fields[2];
                    string name = fields[3];

                    Participant person = new Participant();
                    person.email = email;
                    person.name = name;

                    Console.WriteLine(" email found: "+person.email);

                    if (existingParticipants.Contains(person))
                    {
                        if ((!string.IsNullOrEmpty(fields[4])) && (STRING_YES_SIGNED_BEFORE.Equals(fields[4])))
                        {
                                if ((!string.IsNullOrEmpty(fields[5])) && (STRING_YES_REUSE.Equals(fields[5])))
                                {
                                    Console.WriteLine("Reusing the same details and continuing");
                                    continue;
                                }
                                else if((!string.IsNullOrEmpty(fields[5])) && (STRING_NO_NEW_ANSWERS.Equals(fields[5])))
                                {
                                    Console.WriteLine("found one and new answers needed");
                                }
                          
                               
                        }
                        
                    }
                    else
                    {

                    }

                    /*
                    Dictionary<string, string> participantDataDict = new Dictionary<string, string>();

                    if (!string.IsNullOrEmpty(fields[4]))
                    {
                        string pronouns = fields[4];
                        participantDataDict.Add(KEY_PRONOUNS, pronouns);
                    }

                    if (!string.IsNullOrEmpty(fields[6]))
                    {
                        string intro = fields[6];
                        participantDataDict.Add(KEY_INTRO, intro);
                    }

                    if (!string.IsNullOrEmpty(fields[7]))
                    {
                        string question1 = fields[7];
                        string answer1 = fields[8];
                        participantDataDict.Add(question1, answer1);
                    }

                    if (!string.IsNullOrEmpty(fields[9]))
                    {
                        string question2 = fields[9];
                        string answer2 = fields[10];
                        participantDataDict.Add(question2, answer2);
                    }

                    if (!string.IsNullOrEmpty(fields[11]))
                    {
                        string question3 = fields[11];
                        string answer3 = fields[12];
                        participantDataDict.Add(question3, answer3);
                    }

                    // Creating Person object
                    
                    person.data = participantDataDict;
                    person.displayDictionaryData(person.data);

                    Console.WriteLine();

                    //Console.WriteLine(person);
                    totalparticipantData.Add(person);
                }

                // Display person data
                //totalparticipantData.ForEach(item => Console.Write(item));
                    */
                }
            }
        }
    }  
}

