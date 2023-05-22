namespace FileFlows.VideoNodes;

using System.Linq;
using System.ComponentModel;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

public class VideoHasStream : VideoNode
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Logic;

    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/logical-nodes/video-has-stream";

    [Select(nameof(StreamTypeOptions), 1)]
    public string Stream { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new ListOption { Label = "Video", Value = "Video" },
                    new ListOption { Label = "Audio", Value = "Audio" },
                    new ListOption { Label = "Subtitle", Value = "Subtitle" }
                };
            }
            return _StreamTypeOptions;
        }
    }

    [TextVariable(2)]
    public string Title { get; set; }
    
    [TextVariable(3)]
    public string Codec { get; set; }
    
    [ConditionEquals(nameof(Stream), "Video", inverse: true)]
    [TextVariable(4)]
    public string Language { get; set; }
    
    [ConditionEquals(nameof(Stream), "Audio")]
    [NumberFloat(5)]
    public float Channels { get; set; }

    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return -1;

        bool found = false;
        string title = args.ReplaceVariables(Title, stripMissing: true);
        string codec = args.ReplaceVariables(Codec, stripMissing: true);
        string lang = args.ReplaceVariables(Language, stripMissing: true);
        if (this.Stream == "Video")
        {
            found = videoInfo.VideoStreams.Where(x =>
            {
                if (ValueMatch(title, x.Title) == MatchResult.NoMatch)
                    return false;
                if (string.IsNullOrWhiteSpace(x.CodecTag) == false && ValueMatch(codec, x.CodecTag) == MatchResult.Matched)
                    return true;
                if (ValueMatch(codec, x.Codec) == MatchResult.NoMatch)
                    return false;
                return true;
            }).Any();
        }
        else if (this.Stream == "Audio")
        {
            found = videoInfo.AudioStreams.Where(x =>
            {
                if (ValueMatch(title, x.Title) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(codec, x.Codec) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(lang, x.Language) == MatchResult.NoMatch)
                    return false;
                if (this.Channels > 0 && Math.Abs(x.Channels - this.Channels) > 0.05f)
                    return false;
                return true;
            }).Any();
        }
        else if (this.Stream == "Subtitle")
        {   
            found = videoInfo.SubtitleStreams.Where(x =>
            {
                if (ValueMatch(title, x.Title) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(codec, x.Codec) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(lang, x.Language) == MatchResult.NoMatch)
                    return false;
                return true;
            }).Any();
        }

        return found ? 1 : 2;
    }

    private MatchResult ValueMatch(string pattern, string value)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return MatchResult.Skipped;
        try
        {            

            if (string.IsNullOrEmpty(value))
                return MatchResult.NoMatch;
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            if(rgx.IsMatch(value))
                return MatchResult.Matched;

            if (value.ToLower() == "hevc" && (pattern.ToLower() == "h265" || pattern == "265" || pattern.ToLower() == "h.265"))
                return MatchResult.Matched; // special case

            return MatchResult.NoMatch;
        }
        catch (Exception)
        {
            return MatchResult.NoMatch;
        }
    }

    private enum MatchResult
    {
        NoMatch = 0,
        Matched = 1,
        Skipped = 2
    }
}
