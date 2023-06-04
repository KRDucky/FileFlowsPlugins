﻿using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

public class FfmpegSubtitleStream : FfmpegStream
{
    /// <summary>
    /// Gets or sets the source subtitle stream
    /// </summary>
    public SubtitleStream Stream { get; set; }

    /// <summary>
    /// Gets or sets if this stream has changed
    /// </summary>
    public override bool HasChange => false;

    /// <summary>
    /// Gets the parameters for this stream
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the parameters to pass to FFmpeg for this stream</returns>
    public override string[] GetParameters(GetParametersArgs args)
    {
        if (Deleted)
            return new string[] { };

        bool containerSame =
            string.Equals(args.SourceExtension, args.DestinationExtension, StringComparison.InvariantCultureIgnoreCase);
        
        string destCodec;
        if(containerSame)
            destCodec = "copy";
        else
        {
            destCodec = SubtitleHelper.GetSubtitleCodec(args.DestinationExtension, Stream.Codec);
            if (string.IsNullOrEmpty(destCodec))
            {
                // this subtitle is not supported by the new container, remove it.
                args.Logger?.WLog($"Subtitle stream is not supported in destination container, removing: {Stream.Codec} {Stream.Title ?? string.Empty}");
                return new string[] { };
            }
        }

        List<string> results= new List<string> { "-map", Stream.InputFileIndex + ":s:{sourceTypeIndex}", "-c:s:{index}", destCodec };

        if (string.IsNullOrWhiteSpace(this.Title) == false)
        {
            // first s: means stream specific, this is suppose to have :s:s
            // https://stackoverflow.com/a/21059838
            results.Add($"-metadata:s:s:{args.OutputTypeIndex}");
            results.Add($"title={(this.Title == FfmpegStream.REMOVED ? "" : this.Title)}");
        }
        if (string.IsNullOrWhiteSpace(this.Language) == false)
        {
            results.Add($"-metadata:s:s:{args.OutputTypeIndex}");
            results.Add($"language={(this.Language == FfmpegStream.REMOVED ? "" : this.Language)}");
        }

        if (Metadata.Any())
            results.AddRange(Metadata.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));
        
        if (args.UpdateDefaultFlag)
            results.AddRange(new[] { "-disposition:a:" + args.OutputTypeIndex, this.IsDefault ? "default" : "0" });

        return results.ToArray();
    }
}