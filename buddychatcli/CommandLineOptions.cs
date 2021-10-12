using System;
using System.Collections.Generic;

using CommandLine;

namespace BuddyChatCLI
{
    // Set of commands recognized by BuddyChatCLI
    public enum BuddyChatCommand {
        Update,
        CreatePairings,
        CreateEmails
    }

    public class CommandLineOptions
    {
        // TODO: change type to command enum
        [Value(index: 0, Required = true, HelpText = "Command. See --help for list of commands")]
        public BuddyChatCommand Command { get; set; }

        [Option(shortName: 'h', 
                longName: "historicaldatapath", 
                Required = false, 
                HelpText = "Path to historical files. Expecting a participant.json file in folder. Defaults to current directory")]
        public string HistoricalDataPath { get; set; }
        
        public void ParseCommandline(string[] args)
        {
            var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true);

            parser.ParseArguments<CommandLineOptions>(args)
                .WithNotParsed<CommandLineOptions>((errs) => this.ReportErrors(errs));
        }

        public void ReportErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine("Err: " + error.ToString());
            } 
        }
    }
}