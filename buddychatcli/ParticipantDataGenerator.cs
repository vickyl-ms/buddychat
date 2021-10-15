using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;



[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{

    [Verb("CreateParticipantData", HelpText = "Creates Participant Data combining the existing data so far with the new signup data from the csv files into a new csv file")]
    public class ParticipantDataGenerator
    {

        public static readonly string NEW_SESSION = "new_session.csv";

        ParticipantHelper helper;

        List<Participant> participantList;

        [Option(shortName: 'e',
                longName: "existingDataFilePath",
                Required = false,
                HelpText = "Path of the existing csv file which already has the participant data")]
        public string ExistingListPath { get; set; } = Directory.GetCurrentDirectory();

        [Option(shortName: 'n',
                longName: "newSessionFilePath",
                Required = false,
                HelpText = "Path of the new signups csv file ")]
        public string newSessionFilePath { get; set; } = Directory.GetCurrentDirectory();

        [Option(shortName: 'f',
                longName: "fullParticipantDataFilePath",
                Required = false,
                HelpText = "Path of the new csv file generated which has entire participant data ")]
        public string newSignupListPath { get; set; } = Directory.GetCurrentDirectory();

        public int Execute()
        {
            helper = new ParticipantHelper();
            participantList = helper.createParticipantDataFromExistingCSVFile(Defaults.SignUpFileName);
            participantList = helper.createParticipantDataFromNewSessionCSVFileAndMergeWithExisting(NEW_SESSION, participantList);
            
            string strFilePath = newSignupListPath+"\\AllParticipantData.csv";
            string strFilePathJson = newSignupListPath + "\\ParticipantData.json";
            string strSeperator = ",";
            StringBuilder sbOutput = new StringBuilder();
            sbOutput.AppendLine("name,email,pronouns,intro,first question,first answer,second question,second answer,third question,third answer");
            foreach (var participant in participantList)
            {
                StringBuilder sbInnerOutput = new StringBuilder();
                sbInnerOutput.Append(participant.name);
                sbInnerOutput.Append(strSeperator);

                sbInnerOutput.Append(participant.email);
                sbInnerOutput.Append(strSeperator);

                // Looping the data dictionary
                foreach (KeyValuePair<string, string> kvp in participant.data)
                {
                    sbInnerOutput.Append(kvp.Value);
                    sbInnerOutput.Append(strSeperator);
                }
                sbOutput.AppendLine(sbInnerOutput.ToString());
            }
            // Create and write the csv file
            if (File.Exists(strFilePath))
            {
                File.Delete(strFilePath);
            }
            File.WriteAllText(strFilePath, sbOutput.ToString());
            string json = JsonSerializer.Serialize(participantList);
            File.WriteAllText(strFilePathJson, json);
            Console.WriteLine("Participant data successfully written to " + strFilePath);
            return 0;
        }
    }
}
