using System;
using BuddyChatCLI;
using Xunit;

namespace BuddyChatCLI.test
{
    public class ProgramTests
    {
        [Fact]
        public void Main_CommandIsRequired()
        {
            int expectedReturnCode = Convert.ToInt32(ReturnCode.ErrorParsingCommandLine);
            
            String[] args = new String[0];
            int returnCode = Program.Main(args);

            Assert.Equal(returnCode, expectedReturnCode);
        }
    }
}
