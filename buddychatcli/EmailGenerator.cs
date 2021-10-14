﻿using BuddyChatCLI;
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
                Required = true,
                HelpText = "Path to Outlook template file in .oft format.")]
        public string TemplatePath { get; set; }

        [Option(shortName: 'o',
                longName: "outputPath",
                Required = false,
                HelpText = "Path to the output folder where the emails will be created. Default is current directory.")]
        public string OutputFolder { get; set; } = Directory.GetCurrentDirectory();

        public int Execute()
        {
            (this.subject, this.htmlBody) = ReadTemplate(TemplatePath);

            return 0;
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

            // The MsgKit library doesn't officially support creating Outlook templates (*.oft), it only supports *.msg files.
            // *.msg files can't be sent directly from Outlook so we would have to forward the message if we used this file type.
            // As a workaround, we can set BodyRtf and save the file as .oft. This creates a template file that we can open and send from Outlook.
            email.BodyRtf = GetEscapedRtf(newHtmlBody);
            email.Save(file);

            Console.WriteLine($"Email has been generated: {file}");
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
