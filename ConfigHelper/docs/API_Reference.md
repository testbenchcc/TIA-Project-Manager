# ConfigHelper API Reference

## ConfigManager Class

The `ConfigManager` class provides static methods for loading and saving configuration data.

### Methods

#### LoadOrCreateAsync

```csharp
public static async Task<RepoConfig> LoadOrCreateAsync(string filePath)
```

Loads the configuration from the specified file path. If the file does not exist, creates a new configuration file with default settings.

**Parameters:**
- `filePath` (string): The path to the configuration file.

**Returns:**
- `Task<RepoConfig>`: A task that represents the asynchronous operation. The task result contains the loaded or created configuration.

**Exceptions:**
- `IOException`: Thrown when an I/O error occurs during file operations.
- `JsonException`: Thrown when the JSON in the file is invalid.

**Example:**
```csharp
var config = await ConfigManager.LoadOrCreateAsync("sectionDataObject.json");
```

#### LoadAsync

```csharp
public static async Task<RepoConfig> LoadAsync(string filePath)
```

Loads the configuration from the specified file path. The file must exist.

**Parameters:**
- `filePath` (string): The path to the configuration file.

**Returns:**
- `Task<RepoConfig>`: A task that represents the asynchronous operation. The task result contains the loaded configuration.

**Exceptions:**
- `FileNotFoundException`: Thrown when the file does not exist.
- `JsonException`: Thrown when the JSON in the file is invalid.
- `IOException`: Thrown when an I/O error occurs during file operations.

**Example:**
```csharp
var config = await ConfigManager.LoadAsync("sectionDataObject.json");
```

#### SaveAsync

```csharp
public static async Task SaveAsync(RepoConfig config, string filePath)
```

Saves the configuration to the specified file path.

**Parameters:**
- `config` (RepoConfig): The configuration to save.
- `filePath` (string): The path to the configuration file.

**Returns:**
- `Task`: A task that represents the asynchronous operation.

**Exceptions:**
- `IOException`: Thrown when an I/O error occurs during file operations.
- `ArgumentNullException`: Thrown when the config parameter is null.

**Example:**
```csharp
await ConfigManager.SaveAsync(config, "sectionDataObject.json");
```

## RepoConfig Class

The `RepoConfig` class is the root configuration class that contains a list of repositories.

### Properties

#### Repo

```csharp
public List<Repo> Repo { get; set; }
```

Gets or sets the list of repositories.

**Type:**
- `List<Repo>`: A list of repository configurations.

**Example:**
```csharp
var repositories = config.Repo;
```

## Repo Class

The `Repo` class represents a single repository configuration.

### Properties

#### Name

```csharp
public string Name { get; set; }
```

Gets or sets the name of the repository.

**Type:**
- `string`: The repository name.

#### Path

```csharp
public string Path { get; set; }
```

Gets or sets the file path to the repository.

**Type:**
- `string`: The repository path.

#### Branch

```csharp
public string Branch { get; set; }
```

Gets or sets the current branch name.

**Type:**
- `string`: The branch name.

#### BuildNumber

```csharp
public int BuildNumber { get; set; }
```

Gets or sets the current build number.

**Type:**
- `int`: The build number.

#### Sections

```csharp
public List<Section> Sections { get; set; }
```

Gets or sets the list of sections within the repository.

**Type:**
- `List<Section>`: A list of section configurations.

## Section Class

The `Section` class represents a section within a repository.

### Properties

#### Name

```csharp
public string Name { get; set; }
```

Gets or sets the name of the section.

**Type:**
- `string`: The section name.

#### Icon

```csharp
public string Icon { get; set; }
```

Gets or sets the icon associated with the section.

**Type:**
- `string`: The icon identifier.

#### Commits

```csharp
public Commits? Commits { get; set; }
```

Gets or sets the commit information for the section.

**Type:**
- `Commits?`: The commit information, or null if not applicable.

#### Tags

```csharp
public List<Tag>? Tags { get; set; }
```

Gets or sets the tags associated with the section.

**Type:**
- `List<Tag>?`: A list of tags, or null if not applicable.

#### Devices

```csharp
public List<Device>? Devices { get; set; }
```

Gets or sets the devices associated with the section.

**Type:**
- `List<Device>?`: A list of devices, or null if not applicable.

#### DNumber

```csharp
public string? DNumber { get; set; }
```

Gets or sets the D-number for the section.

**Type:**
- `string?`: The D-number, or null if not applicable.

#### TNumber

```csharp
public string? TNumber { get; set; }
```

Gets or sets the T-number for the section.

**Type:**
- `string?`: The T-number, or null if not applicable.

#### ProjectName

```csharp
public string? ProjectName { get; set; }
```

Gets or sets the project name for the section.

**Type:**
- `string?`: The project name, or null if not applicable.

#### Datablocks

```csharp
public List<Datablock>? Datablocks { get; set; }
```

Gets or sets the datablocks associated with the section.

**Type:**
- `List<Datablock>?`: A list of datablocks, or null if not applicable.

## Commits Class

The `Commits` class represents commit information.

### Properties

#### Limit

```csharp
public int Limit { get; set; }
```

Gets or sets the maximum number of commits to store.

**Type:**
- `int`: The commit limit.

#### Items

```csharp
public List<CommitItem> Items { get; set; }
```

Gets or sets the list of commit items.

**Type:**
- `List<CommitItem>`: A list of commit items.

## CommitItem Class

The `CommitItem` class represents a single commit.

### Properties

#### Date

```csharp
public string Date { get; set; }
```

Gets or sets the date of the commit.

**Type:**
- `string`: The commit date.

#### Title

```csharp
public string Title { get; set; }
```

Gets or sets the title of the commit.

**Type:**
- `string`: The commit title.

#### Body

```csharp
public string Body { get; set; }
```

Gets or sets the body text of the commit.

**Type:**
- `string`: The commit body.

#### Author

```csharp
public string Author { get; set; }
```

Gets or sets the author of the commit.

**Type:**
- `string`: The commit author.

#### Hash

```csharp
public string Hash { get; set; }
```

Gets or sets the hash of the commit.

**Type:**
- `string`: The commit hash.

## Tag Class

The `Tag` class represents a tag.

### Properties

#### Name

```csharp
public string Name { get; set; }
```

Gets or sets the name of the tag.

**Type:**
- `string`: The tag name.

#### Date

```csharp
public string Date { get; set; }
```

Gets or sets the date of the tag.

**Type:**
- `string`: The tag date.

#### Hash

```csharp
public string Hash { get; set; }
```

Gets or sets the hash of the tag.

**Type:**
- `string`: The tag hash.

#### Message

```csharp
public string Message { get; set; }
```

Gets or sets the message of the tag.

**Type:**
- `string`: The tag message.

#### CommitAuthor

```csharp
public string CommitAuthor { get; set; }
```

Gets or sets the author of the commit.

**Type:**
- `string`: The commit author.

#### Tagger

```csharp
public string Tagger { get; set; }
```

Gets or sets the tagger.

**Type:**
- `string`: The tagger.

#### Type

```csharp
public string Type { get; set; }
```

Gets or sets the type of the tag.

**Type:**
- `string`: The tag type.

## Device Class

The `Device` class represents a device.

### Properties

#### Name

```csharp
public string Name { get; set; }
```

Gets or sets the name of the device.

**Type:**
- `string`: The device name.

#### Ip

```csharp
public string Ip { get; set; }
```

Gets or sets the IP address of the device.

**Type:**
- `string`: The device IP address.

## Datablock Class

The `Datablock` class represents a datablock.

### Properties

#### Name

```csharp
public string Name { get; set; }
```

Gets or sets the name of the datablock.

**Type:**
- `string`: The datablock name.

#### Db

```csharp
public string Db { get; set; }
```

Gets or sets the database identifier of the datablock.

**Type:**
- `string`: The database identifier.

#### Tags

```csharp
public List<DbTag> Tags { get; set; }
```

Gets or sets the list of tags within the datablock.

**Type:**
- `List<DbTag>`: A list of database tags.

## DbTag Class

The `DbTag` class represents a database tag.

### Properties

#### Name

```csharp
public string Name { get; set; }
```

Gets or sets the name of the tag.

**Type:**
- `string`: The tag name.

#### Type

```csharp
public string Type { get; set; }
```

Gets or sets the type of the tag.

**Type:**
- `string`: The tag type.

#### Location

```csharp
public string Location { get; set; }
```

Gets or sets the location of the tag.

**Type:**
- `string`: The tag location.

#### Comment

```csharp
public string Comment { get; set; }
```

Gets or sets the comment associated with the tag.

**Type:**
- `string`: The tag comment.
