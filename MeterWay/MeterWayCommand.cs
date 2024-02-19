
using System;

namespace MeterWay.Commands;

public class MeterWayCommand
{
    public string Argument;
    public string Help;
    public Action Function;

    public MeterWayCommand(string argument, string help, Action function)
    {
        this.Argument = argument;
        this.Help = help;
        this.Function = function;
    }
}