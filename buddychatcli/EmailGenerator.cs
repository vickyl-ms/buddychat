using CommandLine;
using MsgKit;
using MsgKit.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Message = MsgReader.Outlook.Storage.Message;

[assembly: InternalsVisibleTo("BuddyChatCLI.test")]
namespace BuddyChatCLI
{
    [Verb("CreateEmails", HelpText = "Creates the emails based on a json file with the pairings information.")]
    public class EmailGenerator
    {
        private string subject;
        private string htmlBody;

        /// <summary>
        /// Regex expression used to identify the first participant fields.
        /// Note: HTML files are encoded and the less than / greater than symbols are encoded as &lt; and &gt; respectively.
        /// </summary>
        private static readonly Regex FirstParticipantRegex = new Regex("&lt;participant1\\.(?<property>[\\w\\.]*)&gt;", RegexOptions.IgnoreCase);

        /// <summary>
        /// Regex expression used to identify the second participant fields.
        /// Note: HTML files are encoded and the less than / greater than symbols are encoded as &lt; and &gt; respectively.
        /// </summary>
        private static readonly Regex SecondParticipantRegex = new Regex("&lt;participant2\\.(?<property>[\\w\\.]*)&gt;", RegexOptions.IgnoreCase);

        [Option(shortName: 't',
                longName: "templatePath",
                Required = false,
                HelpText = "Path to Outlook template file in .oft format. Defaults to emailtemplate.oft in current directory")]
        public string TemplatePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.EmailTemplateFilename);

        [Option(shortName: 'n',
                longName: "NewPairingsFile",
                Required = false,
                HelpText = "Filename of new pairings json file generated with CreatePairing command. Defaults to NewPairings.json in current directory")]
        public string NewPairingsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.NewPairingFileName);

        [Option(shortName: 'p',
        longName: "ParticipantsFile",
        Required = false,
        HelpText = "The location of the participant json file. Default is participants.json in current directory.")]
        public string ParticipantsFile { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), Defaults.ParticipantsFileName);


        [Option(shortName: 'o',
                longName: "outputPath",
                Required = false,
                HelpText = "Path to the output folder where the emails will be created. Default is current directory.")]
        public string OutputFolder { get; set; } = Directory.GetCurrentDirectory();

        public int Execute()
        {
            ValidateOptions();
            
            (this.subject, this.htmlBody) = ReadTemplate(TemplatePath);

            PairingList pairings = GetPairingsFromFile(this.NewPairingsFile);
            
            IDictionary<string, Participant> participants = GetParticipantsFromFile(this.ParticipantsFile);

            foreach(PairingList.Entry pairing in pairings.pairings)
            {
                GenerateEmail(participants[pairing.participant1Email], participants[pairing.participant2Email]);
            }

            return 0;
        }

        private void ValidateOptions()
        {
            if (!File.Exists(this.NewPairingsFile))
            {
                string errMsg = $"No '{this.NewPairingsFile}' found. New pairings file must exist.";
                throw new ArgumentException(errMsg);
            }

            if (!File.Exists(this.ParticipantsFile))
            {
                string errMsg = $"No '{this.ParticipantsFile}' found. Participant data file must exist.";
                throw new ArgumentException(errMsg);
            }

            if (!File.Exists(this.TemplatePath))
            {
                string errMsg = $"No '{this.TemplatePath}' found. Email template file must exist.";
                throw new ArgumentException(errMsg);
            }

            if (!Directory.Exists(this.OutputFolder))
            {
                Console.WriteLine($"Output dir '{this.OutputFolder}' does not exist. Creating directory.");
                Directory.CreateDirectory(this.OutputFolder);
            }
        }

        /// <summary>
        /// Read in Participants data and convert to dictionary keyed by email
        /// </summary>
        /// <param name="participantsFile"></param>
        /// <returns></returns>
        private IDictionary<string, Participant> GetParticipantsFromFile(string participantsFile)
        {
            string participantsJson = File.ReadAllText(participantsFile);
            IList<Participant> participants = JsonConvert.DeserializeObject<IList<Participant>>(participantsJson);
            
            // convert to dictionary keyed by email
            IDictionary<string, Participant> participantDictionary = new Dictionary<string, Participant>();
            foreach (Participant p in participants)
            {
                p.Validate();
                participantDictionary.Add(p.email, p);
            }

            return participantDictionary;
        }

        /// <summary>
        /// Generates an email replacing the placeholders for each participant.
        /// </summary>
        /// <param name="templatePath">The path to the Outlook template in .oft format.</param>
        /// <param name="participant1">The information about the first participant.</param>
        /// <param name="participant2">The information about the second participant.</param>
        public void GenerateEmail(Participant participant1, Participant participant2)
        {
            string newHtmlBody = ReplacePlaceholders(htmlBody, participant1, participant2);
            string file = Path.Combine(this.OutputFolder, $"{participant1.name} - {participant2.name}.oft");

            Email email = new Email(null, subject);
            email.Recipients.AddTo(participant1.email);
            email.Recipients.AddTo(participant2.email);
            email.IconIndex = MessageIconIndex.UnreadMail;
            email.ReplyToRecipients.AddTo(participant1.email);
            email.ReplyToRecipients.AddTo(participant2.email);

            // The MsgKit library doesn't officially support creating Outlook templates (*.oft), it only supports *.msg files.
            // *.msg files can't be sent directly from Outlook so we would have to forward the message if we used this file type.
            // As a workaround, we can set BodyRtf and save the file as .oft. This creates a template file that we can open and send from Outlook.
            email.BodyRtf = GetEscapedRtf(newHtmlBody);
            email.Save(file);

            Console.WriteLine($"Email has been generated: {file}");
        }

        /// <summary>
        /// Deserializes input file into a list of <see cref="PairingEntry"/>
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>List of <see cref="PairingEntry"/></returns>
        internal static PairingList GetPairingsFromFile(string filePath)
        {
            string pairingsJson = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<PairingList>(pairingsJson);
        }

        /// <summary>
        /// Finds and replaces the placeholders in the HTML body.
        /// </summary>
        /// <param name="htmlBody">The HTML body of the template with placeholders in the form of <participant1.*>.</param>
        /// <param name="participant1">The information about the first participant.</param>
        /// <param name="participant2">The information about the second participant.</param>
        /// <returns>The updated HTML body.</returns>
        internal static string ReplacePlaceholders(string htmlBody, Participant participant1, Participant participant2)
        {
            htmlBody = ReplaceParticipantPlaceholders(htmlBody, FirstParticipantRegex, participant1);
            htmlBody = ReplaceParticipantPlaceholders(htmlBody, SecondParticipantRegex, participant2);

            // Workaround to remove duplicated emojis
            IEnumerable<string> emojis = new string[]
            {
                "&#127930;", // Trumpet
                "&#128522;" // Smiling face
            };

            foreach(string emoji in emojis)
            {
                htmlBody = htmlBody.Replace(emoji + emoji, emoji);
            }

            return htmlBody;
        }

        internal static string ReplaceParticipantPlaceholders(string htmlBody, Regex regex, Participant participant)
        {
            JObject participantJOBject = JObject.Parse(JsonConvert.SerializeObject(participant));

            foreach (Match match in regex.Matches(htmlBody))
            {
                string propertyName = match.Groups["property"].Value;
                string value = string.Empty;

                try
                {
                    value = participantJOBject.SelectToken(propertyName).ToString();
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine($"Property {propertyName} doesn't exist for Participant with email {participant.email}");
                }

                htmlBody = htmlBody.Replace(match.Value, value);
            }

            return htmlBody;
        }

        /// <summary>
        /// Reads and extracts information from an Outlook template.
        /// </summary>
        /// <param name="templatePath">The path to the .otf file.</param>
        /// <returns>The subject and the HTML body</returns>
        internal static (string subject, string htmlBody) ReadTemplate(string templatePath)
        {
            // .otf files use encoding 1252 which is not supported by .net core unless we register CodePagesEncodingProvider
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Message template = new Message(templatePath);

            string subject = template.Subject;
            string htmlBody = template.BodyHtml;

            return (subject, htmlBody);
        }

        /// <summary>
        /// This class was taken from the MsgKit library to set BodyRtf in the Email template.
        /// https://github.com/Sicos1977/MsgKit/blob/3d963e9051f0bbeaf895f9466d3aecfe9d83d6e4/MsgKit/Helpers/Strings.cs#L147
        /// </summary>
        private static string GetEscapedRtf(string str)
        {
            // Convert Unicode string to RTF according to specification
            var rtfEscaped = new StringBuilder();
            var escapedChars = new List<int>() { '{', '}', '\\' };
            foreach (var @char in str)
            {
                var intChar = Convert.ToInt32(@char);

                // Ignore control characters
                if (intChar <= 31) continue;

                if (intChar <= 127)
                {
                    if (escapedChars.Contains(intChar))
                        rtfEscaped.Append('\\');
                    rtfEscaped.Append(@char);
                }
                else if (intChar <= 255)
                {
                    rtfEscaped.Append("\\'" + intChar.ToString("x2"));
                }
                else
                {
                    rtfEscaped.Append("\\u");
                    rtfEscaped.Append(intChar);
                    rtfEscaped.Append('?');
                }
            }

            return "{\\rtf1\\ansi\\ansicpg1252\\fromhtml1 {\\*\\htmltag1 " + rtfEscaped + " }}";
        }
    }
}
