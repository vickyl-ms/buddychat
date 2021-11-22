using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using Newtonsoft.Json;

namespace BuddyChatCLI
{
    public static class SignupsReader
    {
        public static IList<Participant> CreateParticipantsFromNewSignUps(string signupsFile, string signupsConfigFile)
        {
            StreamReader signupsStream = new StreamReader(signupsFile);

            string configText = File.ReadAllText(signupsConfigFile);
            SignupsConfig signupsConfig = JsonConvert.DeserializeObject<SignupsConfig>(configText);
            return CreateParticipantsFromNewSignUps(signupsStream.BaseStream, signupsConfig);
        }

        public static IList<Participant> CreateParticipantsFromNewSignUps(Stream signupsStream, SignupsConfig config)
        {
            IList<Participant> newParticipants = new List<Participant>();

            using (TextFieldParser csvParser = new TextFieldParser(signupsStream))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                string[] headers = {};
                int headerCount = 0;

                IDictionary<String, Object> duplicateDetection = new Dictionary<String, Object>();

                Console.Out.WriteLine("Processing signups...");

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    // skip empty lines
                    if (fields.Length == 0) {
                        continue;
                    }

                    // Register first line as headers
                    if (headerCount == 0)
                    {
                        headers = fields;
                        headerCount = fields.Length;
                        continue;
                    }

                    if (fields.Length != headers.Length)
                    {
                        string errMsg = "Current csv line does not contain same number of elements as header line:\n" + 
                            "Headers: " + string.Join(", ", headers) + "\n" +
                            "Current line: " + string.Join(", ", fields) + "\n";
                        throw new Exception(errMsg);
                    }
                    
                    Participant participant = CreateParticipantData(fields, config);

                    // Check for duplicate entries!
                    if (duplicateDetection.ContainsKey(participant.email))
                    {
                        throw new Exception($"Signup list contains a duplicate entry for email: {participant.email}");
                    }

                    duplicateDetection.Add(participant.email, null);

                    Console.Out.WriteLine("-----------------------------------");
                    Console.Out.WriteLine(participant.ToDetailedString());

                    newParticipants.Add(participant);
                }
            }

            Console.Out.WriteLine("-----------------------------------");
            Console.Out.WriteLine($"{newParticipants.Count} signups processed.");
            return newParticipants;
        }

        /// <summary>
        /// This method returns a participant data object from the fields array which has the parsed csv data
        /// </summary>
        /// <param name="fields">String array of the parsed csv data in column format </param>
        /// <param name="config">A SignupsConfig object that gives the name and indexes 
        /// for the entries in fields used to populate a new Participant object</param>
        private static Participant CreateParticipantData(string[] fields, SignupsConfig config)
        {
            string email = fields[config.emailIndex];
            string name = fields[config.nameIndex];

            IDictionary<string, string> participantData = new Dictionary<string, string>();

            foreach (SignupsConfig.ConfigEntry entry in config.dataEntries)
            {
                if (!string.IsNullOrWhiteSpace(fields[entry.index]))
                {
                    participantData.Add(entry.fieldName, fields[entry.index]);
                }
            }

            // Creating Person object
            Participant participant = new Participant {
                email = email,
                name = name,
                data = participantData
            };

            participant.Validate();

            return participant;
        }
    }  
}

