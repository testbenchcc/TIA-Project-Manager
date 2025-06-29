namespace ConfigHelper;

public sealed class RepoConfig
{
    public List<Repo> Repo { get; set; } = [];
}

public sealed class Repo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Branch { get; set; } = "";
    public int BuildNumber { get; set; }
    public List<Section> Sections { get; set; } = [];
}

public sealed class Section
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public Commits? Commits { get; set; }
    public List<Tag>? Tags { get; set; }
    public List<Device>? Devices { get; set; }
    public string? DNumber { get; set; }
    public string? TNumber { get; set; }
    public string? ProjectName { get; set; }
    public List<Datablock>? Datablocks { get; set; }
}

public sealed class Commits
{
    public int Limit { get; set; }
    public List<CommitItem> Items { get; set; } = [];
}

public sealed class CommitItem
{
    public string Date { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Author { get; set; } = "";
    public string Hash { get; set; } = "";
}

public sealed class Tag
{
    public string Name { get; set; } = "";
    public string Date { get; set; } = "";
    public string Hash { get; set; } = "";
    public string Message { get; set; } = "";
    public string CommitAuthor { get; set; } = "";
    public string Tagger { get; set; } = "";
    public string Type { get; set; } = "";
}

public sealed class Device
{
    public string Name { get; set; } = "";
    public string Ip { get; set; } = "";
}

public sealed class Datablock
{
    public string Name { get; set; } = "";
    public string Db { get; set; } = "";
    public List<DbTag> Tags { get; set; } = [];
}

public sealed class DbTag
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Location { get; set; } = "";
    public string Comment { get; set; } = "";
}
