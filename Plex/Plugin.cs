namespace FileFlows.Plex;

public class Plugin : FileFlows.Plugin.IPlugin
{
    public Guid Uid => new Guid("5be72267-7574-4ba9-a958-f3dda0d6c2dc");
    public string Name => "Plex";
    public string MinimumVersion => "1.0.0.1864";

    public void Init()
    {
    }
}
