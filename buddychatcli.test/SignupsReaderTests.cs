using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace BuddyChatCLI.test
{
    public class SignupsReaderTests
    {
        private static Stream StringToStream(string signUps)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(signUps);
            MemoryStream stream = new MemoryStream(byteArray);
            return stream;
        }
        
        [Fact]
        public static void CreateParticipantsFromNewSignupsBasicTest()
        {
            // Test signup with skipped column and line with no data
string signUps =
@"
name, email, pronouns, skipped, question 1, answer 1
first last, email@email.com, she/her,skipped, What is the meaning of life?, 42
Jane C Smith, janes@email.com,,,,
";

            // Test signup config   
            SignupsConfig config = new SignupsConfig {
                nameIndex = 0,
                emailIndex = 1,
                dataEntries = new List<SignupsConfig.ConfigEntry> {
                    new SignupsConfig.ConfigEntry { fieldName = "question1", index = 4 },
                    new SignupsConfig.ConfigEntry { fieldName = "pronouns", index = 2 },
                    new SignupsConfig.ConfigEntry { fieldName = "answer1", index = 5 }
                }
            };

            IList<Participant> actual = SignupsReader.CreateParticipantsFromNewSignUps(StringToStream(signUps), config);

            Assert.Equal(2, actual.Count);

            Participant p1 = actual[0];
            Assert.Equal("first last", p1.name);
            Assert.Equal("email@email.com", p1.email);
            Assert.Equal("she/her", p1.data["pronouns"]);
            Assert.Equal("What is the meaning of life?", p1.data["question1"]);
            Assert.False(p1.data.ContainsKey("skipped"));
            Assert.Equal("42", p1.data["answer1"]);

            Participant p2 = actual[1];
            Assert.Equal("Jane C Smith", p2.name);
            Assert.Equal(0, p2.data.Count);
        }

        [Fact]
        public static void CreateParticipantsFromNewSignupsQuotesTest()
        {
            // Test signup with quoted fields with commas inside and escaped quotes
string signUps =
@"name, email, pronouns, question 1, answer 1
first last, email@email.com, she/her,""What is the, meaning of life?"", ""Escaped characters like """"""
";

            // Test signup config   
            SignupsConfig config = new SignupsConfig {
                nameIndex = 0,
                emailIndex = 1,
                dataEntries = new List<SignupsConfig.ConfigEntry> {
                    new SignupsConfig.ConfigEntry { fieldName = "question1", index = 3 },
                    new SignupsConfig.ConfigEntry { fieldName = "pronouns", index = 2 },
                    new SignupsConfig.ConfigEntry { fieldName = "answer1", index = 4 }
                }
            };

            IList<Participant> actual = SignupsReader.CreateParticipantsFromNewSignUps(StringToStream(signUps), config);

            Participant p1 = actual[0];
            Assert.Equal("What is the, meaning of life?", p1.data["question1"]);
            Assert.Equal("Escaped characters like \"", p1.data["answer1"]);
        }

        [Fact]
        public static void CreateParticipantsFromNewSignupsNewLinesTest()
        {
            // Test signup
string signUps =
@"name, email, pronouns, question 1, answer 1
first last, email@email.com, she/her,""Newlines 

like 

this
"", ...should be handled";

            // Test signup config   
            SignupsConfig config = new SignupsConfig {
                nameIndex = 0,
                emailIndex = 1,
                dataEntries = new List<SignupsConfig.ConfigEntry> {
                    new SignupsConfig.ConfigEntry { fieldName = "question1", index = 3 },
                    new SignupsConfig.ConfigEntry { fieldName = "pronouns", index = 2 },
                    new SignupsConfig.ConfigEntry { fieldName = "answer1", index = 4 }
                }
            };

            IList<Participant> actual = SignupsReader.CreateParticipantsFromNewSignUps(StringToStream(signUps), config);

            Participant p1 = actual[0];
            
            // Note that the final newline is trimmed off automatically by textbox parser
            Assert.Equal($"Newlines {Environment.NewLine}like {Environment.NewLine}this", p1.data["question1"]);
            Assert.Equal("...should be handled", p1.data["answer1"]);
        }

        [Fact]
        public static void CreateParticipantsFromNewSignupsUnicodeTest()
        {
            // Test signup
string signUps =
@"name, email, pronouns, question 1, answer 1
first last, email@email.com, she/her,""Favorite Quotes: 
“Talent wins games, but teamwork and intelligence win championships.” ~Michael Jordan 
"", Has unicode!";

            // Test signup config   
            SignupsConfig config = new SignupsConfig {
                nameIndex = 0,
                emailIndex = 1,
                dataEntries = new List<SignupsConfig.ConfigEntry> {
                    new SignupsConfig.ConfigEntry { fieldName = "question1", index = 3 },
                    new SignupsConfig.ConfigEntry { fieldName = "pronouns", index = 2 },
                    new SignupsConfig.ConfigEntry { fieldName = "answer1", index = 4 }
                }
            };

            IList<Participant> actual = SignupsReader.CreateParticipantsFromNewSignUps(StringToStream(signUps), config);

            Participant p1 = actual[0];
            Assert.Equal(
@"Favorite Quotes: 
“Talent wins games, but teamwork and intelligence win championships.” ~Michael Jordan", p1.data["question1"]);
            Assert.Equal("Has unicode!", p1.data["answer1"]);
        }

        [Fact]
        public static void CreateParticipantsFromNewSignupsFileTest()
        {
            // expected data
            string p1ExpectedOutput = 
@"Name: John Smith
Email: user1@microsoft.com
pronouns: He
introduction: Intro 1. Intro2
question1: Question1?
answer1: Answer1
question2: Question2
answer2: Answer2
";

            string p2ExpectedOutput = 
@"Name: Fred User2
Email: user2@microsoft.com
";

            string p3ExpectedOutput = 
@"Name: Vijay User3
Email: user3@microsoft.com
pronouns: HE, HIM, HIS
introduction: I am
question1: What have you started since the pandemic?
answer1: During Pandemic, I am exploring new experiences using technologies. Some of them include:
1. I played the role of ""wedding planner"" for my nephew in India, who got married six months ago
Favorite Quotes: 
“Talent wins games, but teamwork and intelligence win championships.” ~Michael Jordan
";

            // Test signup files
            string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
            string signupsFile = Path.Combine(testDataPath, Defaults.SignupsFilename);
            string signupsConfigFile = Path.Combine(testDataPath, Defaults.SignupsConfigFilename);

            IList<Participant> actual = SignupsReader.CreateParticipantsFromNewSignUps(signupsFile: signupsFile, signupsConfigFile: signupsConfigFile);

            Assert.Equal(3, actual.Count);
            Assert.Equal(p1ExpectedOutput, actual[0].ToDetailedString());
            Assert.Equal(p2ExpectedOutput, actual[1].ToDetailedString());
            Assert.Equal(p3ExpectedOutput, actual[2].ToDetailedString());
        }
    }
}
