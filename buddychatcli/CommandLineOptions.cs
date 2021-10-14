using CommandLine;

namespace BuddyChatCLI
{
    public class CommandLineOptions
    {
        // TODO: change type to command enum
        [Value(index: 0, Required = true, HelpText = "Command. See --help for list of commands")]
        public BuddyChatCommand Command { get; set; }

        [Option(shortName: 'h', 
                longName: "historicaldatapath", 
                Required = false, 
                HelpText = "Path to historical files. Expecting a participant.json file in folder. Defaults to current directory or child directory named with session")]
        public string HistoricalDataPath { get; set; }
    }
}