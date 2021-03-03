using System;

namespace PepperDash.Essentials.Displays
{
    public class SerialCommandBuilder:ICommandBuilder
    {
        public string Delimiter
        {
            get { return "\x03"; }
        }

        private readonly string _id;

        private const string CommandWithParameterFormat = "\x02AD{0};{1}:{2}\x03";
        private const string CommandWithoutParameterFormat = "\x02AD{0};{1}\x03";

        public SerialCommandBuilder(string id)
        {
            _id = id;
        }

        public string GetCommand(string cmd, string parameter)
        {
            return String.Format(CommandWithParameterFormat, _id, cmd, parameter);
        }

        public string GetCommand(string cmd)
        {
            return String.Format(CommandWithoutParameterFormat, _id, cmd);
        }
    }
}