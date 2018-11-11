﻿namespace HomeCenter.Model.Messages.Commands.Device
{
    public class InputSetCommand : Command
    {
        public static InputSetCommand Create(string input)
        {
            var command = new InputSetCommand();
            command[CommandProperties.InputSource] = input;
            return command;
        }
    }
}