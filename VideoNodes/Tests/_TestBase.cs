﻿#if(DEBUG)

using System.Runtime.InteropServices;
using FileFlows.VideoNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.IO;

namespace VideoNodes.Tests;

[TestClass]
public abstract class TestBase
{
    /// <summary>
    /// The test context instance
    /// </summary>
    private TestContext testContextInstance;

    internal TestLogger Logger = new();

    /// <summary>
    /// Gets or sets the test context
    /// </summary>
    public TestContext TestContext
    {
        get => testContextInstance;
        set => testContextInstance = value;
    }

    public string TestPath { get; private set; }
    public string TempPath { get; private set; }
    public string FfmpegPath { get; private set; }
    public string FfprobePath { get; private set; }
    
    public readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    [TestInitialize]
    public void TestInitialize()
    {
        Logger.Writer = (msg) => TestContext.WriteLine(msg);
        
        if (System.IO.File.Exists("../../../test.settings.dev.json"))
        {
            LoadSettings("../../../test.settings.dev.json");
        }
        else if (System.IO.File.Exists("../../../test.settings.json"))
        {
            LoadSettings("../../../test.settings.json");
        }
        this.TestPath = this.TestPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/test-files/videos" : @"d:\videos\testfiles");
        this.TempPath = this.TempPath?.EmptyAsNull() ?? (IsLinux ? "~/src/ff-files/temp" : @"d:\videos\temp");
        this.FfmpegPath = this.FfmpegPath?.EmptyAsNull() ?? (IsLinux ? "/usr/local/bin/ffmpeg" :  @"C:\utils\ffmpeg\ffmpeg.exe");
        this.FfprobePath = this.FfmpegPath?.EmptyAsNull() ?? (IsLinux ? "/usr/local/bin/ffprobe" :  @"C:\utils\ffprobe\ffprobe.exe");
        
        this.TestPath = this.TestPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.TempPath = this.TempPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.FfmpegPath = this.FfmpegPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        this.FfprobePath = this.FfprobePath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        
        if (Directory.Exists(this.TempPath) == false)
            Directory.CreateDirectory(this.TempPath);
    }

    [TestCleanup]
    public void CleanUp()
    {
        TestContext.WriteLine(Logger.ToString());
    }

    private void LoadSettings(string filename)
    {
        try
        {
            if (File.Exists(filename) == false)
                return;
            
            string json = File.ReadAllText(filename);
#pragma warning disable IL2026
            var settings = JsonSerializer.Deserialize<TestSettings>(json);
#pragma warning restore IL2026
            this.TestPath = settings.TestPath;
            this.TempPath = settings.TempPath;
            this.FfmpegPath = settings.FfmpegPath;
        }
        catch (Exception) { }
    }

    protected virtual void TestStarting()
    {

    }

    protected string TestFile_MovText_Mp4 => Path.Combine(TestPath, "movtext.mp4");
    protected string TestFile_BasicMkv => Path.Combine(TestPath, "basic.mkv");
    protected string TestFile_Corrupt => Path.Combine(TestPath, "corrupt.mkv");
    protected string TestFile_Webvtt => Path.Combine(TestPath, "webvtt4.mkv");
    protected string TestFile_Tag => Path.Combine(TestPath, "tag.mp4");
    protected string TestFile_Sitcom => Path.Combine(TestPath, "sitcom.mkv");
    protected string TestFile_Pgs => Path.Combine(TestPath, "pgs.mkv");
    protected string TestFile_Subtitle => Path.Combine(TestPath, "subtitle.mkv");
    protected string TestFile_Error => Path.Combine(TestPath, "error.mkv");
    protected string TestFile_Font => Path.Combine(TestPath, "font.mkv");
    protected string TestFile_DefaultSub => Path.Combine(TestPath, "default-sub.mkv");
    protected string TestFile_ForcedDefaultSub => Path.Combine(TestPath, "sub-forced-default.mkv");
    protected string TestFile_DefaultIsForcedSub => Path.Combine(TestPath, "sub-default-is-forced.mkv");
    protected string TestFile_5dot1 => Path.Combine(TestPath, "5.1.mkv");
    protected string TestFile_TwoPassNegInifinity => Path.Combine(TestPath, "audio_normal_neg_infinity.mkv");
    protected string TestFile_4k_h264mov => Path.Combine(TestPath, "4k_h264.mov");
    protected string TestFile_4k_h264mkv => Path.Combine(TestPath, "4k_h264.mkv");

    protected string TestFile_50_mbps_hd_h264 => Path.Combine(TestPath, "50-mbps-hd-h264.mkv");
    protected string TestFile_120_mbps_4k_uhd_hevc_10bit => Path.Combine(TestPath, "120-mbps-4k-uhd-hevc-10bit.mkv");

    private class TestSettings
    {
        public string TestPath { get; set; }
        public string TempPath { get; set; }
        public string FfmpegPath { get; set; }
    }
}

#endif