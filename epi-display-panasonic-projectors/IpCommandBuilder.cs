using System;

namespace PepperDash.Essentials.Displays
{
    public class IpCommandBuilder:ICommandBuilder
    {
        private const string CommandWithParameterOnly = "00{0}:{1}";
        private const string CommandOnly = "00{0}";

        public string Delimiter
        {
            get { return "\r"; }
        }

        public string GetCommand(string cmd, string parameter)
        {
            return String.Format(CommandWithParameterOnly, cmd, parameter);
        }

        public string GetCommand(string cmd)
        {
            return String.Format(CommandWithParameterOnly, cmd);
        }
    }
}