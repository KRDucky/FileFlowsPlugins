﻿namespace FileFlows.Plex;

internal static class ExtensionMethods
{
    public static string? EmptyAsNull(this string str) => str == string.Empty ? null : str;
}
