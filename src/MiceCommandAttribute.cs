using System;

[AttributeUsage(AttributeTargets.Method)]
public class MiceCommandAttribute : Attribute
{
    public string Command { get; private set; }

    public MiceCommandAttribute(string command)
    {
        this.Command = command;
    }
}