using System;
using BuddyChatCLI;
using Xunit;

namespace BuddyChatCLI.test
{
    public class CommandLineOptionsTests
    {
        [Fact]
        public void CommandIsRequired()
        {
            String[] args = new String[0];
            CommandLineOptions options = new CommandLineOptions();
            Assert.False(options.ParseCommandline(args));
        }
    }
}
