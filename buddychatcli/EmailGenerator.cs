using MsgKit;
using MsgKit.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Message = MsgReader.Outlook.Storage.Message;

namespace buddychatcli
{
    public class EmailGenerator
    {
        /// <summary>
        /// Generates the emails
        /// </summary>
        /// <param name="templatePath">The path to the template in .oft format.</param>
        public void GenerateEmails(string templatePath, List<Participant> participants)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Message msg = new Message("C:\\Users\\isleal\\Documents\\BuddyChatTemplate.oft");

            string subject = msg.Subject;
            string htmlBody = msg.BodyHtml;

            var utf = Encoding.UTF8;
            var estEncoding = Encoding.GetEncoding(1252);
            var newHtmlBody = utf.GetString(Encoding.Convert(estEncoding, utf, estEncoding.GetBytes(htmlBody)));

            File.WriteAllTextAsync("C:\\Users\\isleal\\source\\test.html", htmlBody);

            var email = new Email(null, subject);
            email.Recipients.AddTo("vickyl@microsoft.com", "Vicky Liu");
            email.Recipients.AddCc("savor@microsoft.com", "Sagar Rajesh");
            email.IconIndex = MessageIconIndex.UnreadMail;

            // The MsgKit library doesn't officially support creating Outlook templates (*.oft), it only supports *.msg files.
            // *.msg files can't be sent directly from Outlook so we would have to forward the message if we used this file type.
            // As a workaround, we can set BodyRtf and save the file as .oft. This creates a template file that we can open and send from Outlook.
            email.BodyRtf = GetEscapedRtf(newHtmlBody);
            email.Save(@"C:\Users\isleal\source\newemail.oft");

            Console.WriteLine("Emails have been created");
        }

        /// <summary>
        /// This class was taken from the MsgKit library to set BodyRtf in the Email template.
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
