namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class VideoFile : VideoNode
    {
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Input;

        public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/video-file";

        public override bool NoEditorOnAdd => true;

        [DefaultValue(25)]
        [NumberInt(1)]
        [Range(5, 1000)]
        public int ProbeSize { get; set; }

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public VideoFile()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "vi.Video.Codec", "hevc" },
                { "vi.Audio.Codec", "ac3" },
                { "vi.Audio.Codecs", "ac3,aac"},
                { "vi.Audio.Language", "eng" },
                { "vi.Audio.Languages", "eng, mao" },
                { "vi.Resolution", "1080p" },
                { "vi.Duration", 1800 },
                { "vi.VideoInfo", new VideoInfo() 
                    {
                        Bitrate = 10_000_000,
                        VideoStreams = new List<VideoStream> {
                            new VideoStream { }
                        },
                        AudioStreams = new List<AudioStream> {
                            new AudioStream { }
                        },
                        SubtitleStreams = new List<SubtitleStream>
                        {
                            new SubtitleStream { }
                        }
                    } 
                },
                { "vi.Width", 1920 },
                { "vi.Height", 1080 },
            };
        }

        public override int Execute(NodeParameters args)
        {
            VideoInfoHelper.ProbeSize = this.ProbeSize;

            try
            {
                var videoInfo = new VideoInfoHelper(FFMPEG, args.Logger).Read(args.WorkingFile);
                if (videoInfo.VideoStreams.Any() == false)
                {
                    args.Logger.ELog("No video streams detected.");
                    return -1;
                }
                foreach (var vs in videoInfo.VideoStreams)
                {
                    args.Logger.ILog($"Video stream '{vs.Codec}' '{vs.Index}'");
                }

                foreach (var stream in videoInfo.VideoStreams)
                {
                    if (string.IsNullOrEmpty(stream.Codec) == false)
                    {
                        args.RecordStatistic("CODEC", stream.Codec);
                        args.RecordStatistic("VIDEO_CODEC", stream.Codec);
                    }
                }

                foreach (var stream in videoInfo.AudioStreams)
                {
                    if (string.IsNullOrEmpty(stream.Codec) == false)
                    {
                        args.RecordStatistic("CODEC", stream.Codec);
                        args.RecordStatistic("AUDIO_CODEC", stream.Codec);
                    }
                }

                var resolution = ResolutionHelper.GetResolution(videoInfo);
                var resName = resolution switch
                {
                    ResolutionHelper.Resolution.r1080p => "1080p",
                    ResolutionHelper.Resolution.r480p => "480p",
                    ResolutionHelper.Resolution.r720p => "720p",
                    ResolutionHelper.Resolution.r4k => "4k",
                    _ => null
                };

                if (resName != null)
                {
                    args.Logger?.ILog("Video Resolution: " + resName);
                    args.RecordStatistic("VIDEO_RESOLUTION", resName);
                }

                string extension = new FileInfo(args.FileName).Extension.ToLower()[1..];
                var container = extension switch
                {
                    "mkv" => "MKV",
                    "mk3d" => "MKV",
                    "mp4" => "MP4",
                    "mpeg4" => "MP4",
                    "mp4v" => "MP4",
                    "m4v" => "MP4",
                    "mpg" => "MPEG",
                    "mpeg" => "MPEG",
                    "mov" => "MOV",
                    "qt" => "MOV",
                    "asf" => "ASF",
                    "wmv" => "ASF",
                    _ => extension.ToUpper()
                };
                if (string.IsNullOrEmpty(container) == false)
                {
                    args.Logger?.ILog("Video Container: " + container);
                    args.RecordStatistic("VIDEO_CONTAINER", container);
                }


                foreach (var vs in videoInfo.AudioStreams)
                {
                    args.Logger.ILog($"Audio stream '{vs.Codec}' '{vs.Index}' '{vs.Language}");
                }

                SetVideoInfo(args, videoInfo, Variables);

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
        }
    }


}