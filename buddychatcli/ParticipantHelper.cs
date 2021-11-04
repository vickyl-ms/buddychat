using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System;

namespace BuddyChatCLI
{
    public static class ParticipantHelper
    {
        public static readonly string KEY_INTRO = "intro";
        public static readonly string KEY_PRONOUNS = "pronouns";
        public static readonly string KEY_QUESTION1 = "question1";
        public static readonly string KEY_ANSWER1 = "answer1";
        public static readonly string KEY_QUESTION2 = "question2";
        public static readonly string KEY_ANSWER2 = "answer2";
        public static readonly string KEY_QUESTION3 = "question3";
        public static readonly string KEY_ANSWER3 = "answer3";

        public static readonly string STRING_YES_SIGNED_BEFORE = "Yes, I have signed up before.";
        public static readonly string STRING_YES_REUSE = "Yes, please reuse the intro and questions from when I signed up in the past.";
        public static readonly string STRING_NO_NEW_ANSWERS = "No, I want to enter my introduction and new questions and answers.";

        /// <summary>
        /// This method creates a list of all participants who have already participated in buddy chat program
        /// Reads from .csv file and creates a list of participants
        /// </summary>
        /// <param name="filename">The name of the file to be parsed </param>
        public static IList<Participant> ReadInHistoricalParticipantData(string filename) 
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
                    Participant participant = CreateParticipantData(fields,1,2,4,6,7,9,11);
                    totalparticipantData.Add(participant);
                }
            }
            return totalparticipantData;
        }

        /// <summary>
        /// This method updates the exisiting list of participants with new people who have participated for the first time
        /// or updates their data with new data which they have submitted it. 
        /// Reads from .csv file and creates a list of participants
        /// </summary>
        /// <param name="filename">The name of the file to be parsed </param>
        /// <param name="existingParticipants">The list of existing participants.</param>
        public static IList<Participant> MergeNewSignupWithHistoricalData(string filename, IList<Participant> existingParticipants)
        {
            using (TextFieldParser csvParser = new TextFieldParser(filename))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                csvParser.ReadLine(); // skipping the 1st line which is the column title
                List<Participant> totalparticipantData = new List<Participant>();
                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string email = fields[2];
                    
                    Participant person = new Participant();
                    person.email = email;
           
                    if (existingParticipants.Contains(person))
                    {
                        if ((!string.IsNullOrEmpty(fields[4])) && (STRING_YES_SIGNED_BEFORE.Equals(fields[4])))
                        {
                            if ((!string.IsNullOrEmpty(fields[5])) && (STRING_YES_REUSE.Equals(fields[5])))
                            {
                                continue; // The same participant has signed up again with the same answers
                            }
                            else if ((!string.IsNullOrEmpty(fields[5])) && (STRING_NO_NEW_ANSWERS.Equals(fields[5])))
                            {

                                // The same participant has signed up again with the new answers
                                existingParticipants.Remove(person); // Remove the current one since we are going to add a new one
                            }  
                        }
                        
                    }

                    // Create a new entry
                    Participant participant = CreateParticipantData(fields, 2, 3, 6, 8, 9, 11, 13);
                    existingParticipants.Add(participant);
                }
            }
            return existingParticipants;
        }

        /// <summary>
        /// This method returns a participant data object from the fields array which has the parsed csv data
        /// </summary>
        /// <param name="fields">String array of the parsed csv data in column format </param>
        /// <param name="emailFieldNum">The index position of email field value in the fields array </param>
        /// <param name="nameFieldNum">The index position of name field value in the fields array </param>
        /// <param name="pronounFieldNum">The index position of pronouns field value in the fields array if present </param>
        /// <param name="introFieldNum">The index position of intro field value in the fields array if present </param>
        /// <param name="question1FieldNum">The index position of question1 field value in the fields array if present </param>
        /// <param name="question2FieldNum">The index position of question2 field value in the fields array if present </param>
        /// <param name="question3FieldNum">The index position of question3 field value in the fields array if present </param>
        private static Participant CreateParticipantData(String[] fields, int emailFieldNum, int nameFieldNum, int pronounsFieldNum, int introFieldNum, int question1FieldNum, int question2FieldNum, int question3FieldNum)
        {
            string email = fields[emailFieldNum];
            string name = fields[nameFieldNum];

            Dictionary<string, string> participantDataDict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(fields[pronounsFieldNum])) // pronoun 
            {
                string pronouns = fields[pronounsFieldNum];
                string replacePronouns = pronouns.Replace(',', ' ');
                participantDataDict.Add(KEY_PRONOUNS, replacePronouns);
            }

            if (!string.IsNullOrEmpty(fields[introFieldNum])) // intro
            {
                string intro = fields[introFieldNum];
                participantDataDict.Add(KEY_INTRO, intro);
            }

            if (!string.IsNullOrEmpty(fields[question1FieldNum])) //questions1
            {
                string question1 = fields[question1FieldNum]; 
                string answer1 = fields[question1FieldNum + 1];
                participantDataDict.Add(KEY_QUESTION1, question1);
                participantDataDict.Add(KEY_ANSWER1, answer1);
            }

            if (!string.IsNullOrEmpty(fields[question2FieldNum])) // question2
            {
                string question2 = fields[question2FieldNum];
                string answer2 = fields[question2FieldNum + 1];
                participantDataDict.Add(KEY_QUESTION2, question2);
                participantDataDict.Add(KEY_ANSWER2, answer2);
            }

            if (!string.IsNullOrEmpty(fields[question3FieldNum])) // question3
            {
                string question3 = fields[question3FieldNum];
                string answer3 = fields[question3FieldNum + 1];
                participantDataDict.Add(KEY_QUESTION3, question3);
                participantDataDict.Add(KEY_ANSWER3, answer3);
            }

            // Creating Person object
            Participant participant = new Participant {
                email = email,
                name = name,
                data = participantDataDict
            };
            return participant;
        }


    }  
}

