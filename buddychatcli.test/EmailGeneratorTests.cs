using BuddyChatCLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace buddychatcli.test
{
    public class EmailGeneratorTests
    {
        [Fact]
        public static void ReplacePlaceholdersTest()
        {
            Participant participant1 = new Participant()
            {
                name = "FirstName1 LastName1",
                email = "participant1@test.com",
                data = new Dictionary<string, string>()
                {
                    { "introduction", "Hi my name is 1" },
                    { "question1", "Question from Participant 1" }
                }
            };

            Participant participant2 = new Participant()
            {
                name = "FirstName2 LastName2",
                email = "participant2@test.com",
                data = new Dictionary<string, string>()
                {
                    { "introduction", "Hi my name is 2" }
                }
            };

            string htmlBody = @"
<html>
<body>
We would like to introduce you to:
First participant:
Name = &lt;participant1.name&gt;
FirstName = &lt;participant1.first_name&gt;
Email = &lt;participant1.email&gt;
Introduction = &lt;participant1.data.introduction&gt;
Question = &lt;participant1.data.question1&gt;

Second participant:
Name = &lt;participant2.name&gt;
FirstName = &lt;participant2.first_name&gt;
Email = &lt;participant2.email&gt;
Introduction = &lt;participant2.data.introduction&gt;
Question = &lt;participant2.data.question1&gt;

</body>
</html>
";

            string expectedHtmlBody = @$"
<html>
<body>
We would like to introduce you to:
First participant:
Name = FirstName1 LastName1
FirstName = FirstName1
Email = participant1@test.com
Introduction = Hi my name is 1
Question = Question from Participant 1

Second participant:
Name = FirstName2 LastName2
FirstName = FirstName2
Email = participant2@test.com
Introduction = Hi my name is 2
Question = 

</body>
</html>
";
            string result = EmailGenerator.ReplacePlaceholders(htmlBody, participant1, participant2);

            Assert.Equal(expectedHtmlBody, result);
        }
    }
}
