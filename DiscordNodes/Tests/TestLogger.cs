﻿#if(DEBUG)

namespace FileFlows.DiscordNodes.Tests;

using FileFlows.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class TestLogger : ILogger
{
    private List<string> Messages = new List<string>();

    public void DLog(params object[] args) => Log("DBUG", args);

    public void ELog(params object[] args) => Log("ERRR", args);

    public void ILog(params object[] args) => Log("INFO", args);

    public void WLog(params object[] args) => Log("WARN", args);
    private void Log(string type, object[] args)
    {
        if (args == null || args.Length == 0)
            return;
        string message = type + " -> " +
            string.Join(", ", args.Select(x =>
            x == null ? "null" :
            x.GetType().IsPrimitive || x is string ? x.ToString() :
            System.Text.Json.JsonSerializer.Serialize(x)));
        Messages.Add(message);
    }

    public bool Contains(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        string log = string.Join(Environment.NewLine, Messages);
        return log.Contains(message);
    }

    public override string ToString()
    {
        return String.Join(Environment.NewLine, this.Messages.ToArray());
    }

    public string GetTail(int length = 50)
    {
        if (length <= 0)
            length = 50;
        if (Messages.Count <= length)
            return string.Join(Environment.NewLine, Messages);
        return string.Join(Environment.NewLine, Messages.TakeLast(length));
    }
}

#endif