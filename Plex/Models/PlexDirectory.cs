﻿namespace FileFlows.Plex.Models;

public class PlexDirectory
{
    public string? Key { get; set; }
    public PlexDirectoryLocation[]? Location { get; set; }
}

public class PlexDirectoryLocation
{
    public int Id { get; set; }
    public string? Path { get; set; }
}