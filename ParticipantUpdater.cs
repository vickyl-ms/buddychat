using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{
    [Verb("Update", HelpText = "Updates participant data by combining historical data with new signup data")]
    public class ParticipantUpdater
    {
        [Option(shortName: 'p',
                longName: "PathToHistoricalData",
                Required = false,
                HelpText = "Path to json file with historical participant data (participants.json) and " +
                    "historical pairing data (pairinghistory.json). Defaults to current directory.")]
        public string PathToHistoricalData { get; set; } = Directory.GetCurrentDirectory();

        [Option(shortName: 'n',
                longName: "NewSignupsFile",
                Required = false,
                HelpText = "Filename of csv with new signups data csv file. Defaults signup.csv in current directory")]
        public string NewSignupFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.SignUpFileName);

        [Option(shortName: 's',
                longName: "SessionId",
                Required = true,
                HelpText = "The id for the new session.")]
        public string SessionId { get; set; }

        [Option(shortName: 'o',
                longName: "OutputPath",
                Required = false,
                HelpText = "Path to put the updated participant data and pairing history data. Defaults to <current directory>\\<session id>")]
        public string OutputPath { get; set; }

        public int Execute()
        {
            List<Participant> historicalParticipants = ParticipantHelper.ReadInHistoricalParticipantData(Path.Combine(this.PathToHistoricalData, Defaults.ParticipantsFileName));
            List<Participant> updatedParticipants = ParticipantHelper.MergeNewSignupWithHistoricalData(NewSignupFile, historicalParticipants);

            string strFilePath = OutputPath+"\\AllParticipantData.csv";
            string strFilePathJson = OutputPath + "\\ParticipantData.json";
            
            string strSeperator = ",";
            StringBuilder sbOutput = new StringBuilder();
            sbOutput.AppendLine("name,email,pronouns,intro,first question,first answer,second question,second answer,third question,third answer");
            foreach (var participant in historicalParticipants)
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
            string json = JsonSerializer.Serialize(historicalParticipants);
            File.WriteAllText(strFilePathJson, json);
            Console.WriteLine("Participant data successfully written to " + strFilePath);
            return 0;
        }
    }
}
