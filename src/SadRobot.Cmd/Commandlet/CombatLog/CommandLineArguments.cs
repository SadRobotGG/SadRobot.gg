using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using SadRobot.Core.Apis.BlizzardApi;
using SadRobot.Core.Apis.BlizzardApi.Models;

namespace SadRobot.Cmd.Commandlet.CombatLog
{
    public class CommandLineArguments
    {
        public Command Command { get; set; }

        public CommandLineArguments(IReadOnlyList<string> args)
        {
            if (args == null || args.Count == 0) return;
            
            var commandArg = args[0].Trim();
            
            if (!Enum.TryParse(commandArg, true, out Command command))
            {
                Console.WriteLine($"Unknown command: ${commandArg}");
                return;
            }

            Command = command;

            foreach (var arg in args)
            {
                switch (arg.Trim().ToLowerInvariant().TrimStart('-', '/'))
                {
                    
                }
            }

            switch (Command)
            {
                case Command.None:
                    break;

                case Command.Split:
                    break;

                default:
                    break;
            }
        }
    }
}
