﻿namespace FileFlows.ImageNodes;

public class ImageInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool IsPortrait => Width < Height;
    public bool IsLandscape => Height < Width;
}
