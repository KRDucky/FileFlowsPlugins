﻿using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderSubtitleTrackMerge : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/subtitle-track-merge";
    
    public override string Icon => "fas fa-comment-medical";

    public override int Outputs => 2;

    [Checklist(nameof(Options), 1)]
    [Required]
    public List<string> Subtitles { get; set; }

    private static List<ListOption> _Options;
    public static List<ListOption> Options
    {
        get
        {
            if (_Options == null)
            {
                _Options = new List<ListOption>
                {
                    new ListOption { Value = "ass", Label = "ass: Advanced SubStation Alpha"},
                    new ListOption { Value = "idx", Label = "idx: IDX"},
                    new ListOption { Value = "srt", Label = "srt: SubRip subtitle"},
                    new ListOption { Value = "ssa", Label = "ssa: SubStation Alpha"},
                    new ListOption { Value = "sub", Label = "sub: SubStation Alpha"},
                    new ListOption { Value = "sup", Label = "sup: SubPicture"},
                    new ListOption { Value = "txt", Label = "txt: Raw text subtitle"}                        
                };
            }
            return _Options;
        }
    }

    [Boolean(2)]
    [DefaultValue(true)]
    public bool UseSourceDirectory { get; set; } = true;

    [Boolean(3)]
    public bool MatchFilename { get; set; }
    
    public override int Execute(NodeParameters args)
    {

        var dir = UseSourceDirectory ? new FileInfo(args.FileName).Directory : new DirectoryInfo(args.TempPath);
        if (dir.Exists == false)
        {
            args.Logger?.ILog("Directory does not exist: " + dir.FullName);
            return 2;
        }
        foreach(var sub in Subtitles)
        {
            args.Logger.ILog("Add Subtitle Extension: " + sub);
        }

        int count = 0;
        foreach (var file in dir.GetFiles())
        {
            string ext = file.Extension;
            if (string.IsNullOrEmpty(ext) || ext.Length < 2)
                continue;
            ext = ext.Substring(1).ToLower();// remove .
            if (Subtitles.Contains(ext) == false)
                continue;

            if (MatchFilename)
            {
                var origFile = new FileInfo(args.FileName);
                string origFilename = origFile.Name.Replace(origFile.Extension, "");
                bool matchesOriginal = file.Name.Replace(file.Extension, string.Empty).ToLowerInvariant().Equals(origFilename.ToLowerInvariant());

                var workingFile = new FileInfo(args.WorkingFile);
                string workingFilename = workingFile.Name.Replace(workingFile.Extension, "");
                bool matchesWorking = file.Name.Replace(file.Extension, string.Empty).ToLowerInvariant().Equals(workingFilename.ToLowerInvariant());

                if (matchesOriginal == false && matchesWorking == false)
                    continue;
            }

            args.Logger.ILog("Adding file: " + file.FullName + " [" + ext + "]");
            this.Model.InputFiles.Add(file.FullName);
            this.Model.SubtitleStreams.Add(new FfmpegSubtitleStream
            {
                Stream = new SubtitleStream()
                {
                    InputFileIndex = this.Model.InputFiles.Count - 1,
                    TypeIndex = 0,
                    Title = file.Name.Replace(file.Extension, ""),
                    Codec = file.Extension[1..],
                    IndexString = (this.Model.InputFiles.Count - 1) + ":s:0"
                },
                
            });
            ++count;
        }
        args.Logger.ILog("Subtitles added: " + count);
        if (count > 0)
            this.Model.ForceEncode = true;
        return count > 0 ? 1 : 2;
    }
}
