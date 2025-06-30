using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using ConfigHelper;
using Path = System.IO.Path;
using GitHelper;

namespace Interface
{
    public partial class GitInfoPage : Page
    {
        private string _repoPath;
        private Dictionary<DateTime, List<CommitViewModel>> _commitsByDate = new Dictionary<DateTime, List<CommitViewModel>>();
        private ObservableCollection<CommitViewModel> _selectedDateCommits = new ObservableCollection<CommitViewModel>();
        private ObservableCollection<TagViewModel> _tags = new ObservableCollection<TagViewModel>();
        private ObservableCollection<ReleaseViewModel> _releases = new ObservableCollection<ReleaseViewModel>();
        
        // Configuration file path
        private readonly string _configFilePath = "sectionDataObject.json";
        // Dashboard section name - Git information appears in this section
        private const string _dashboardSectionName = "Dashboard";

        public DependencyProperty FontWeightProperty { get; private set; }

        public GitInfoPage(string repoPath)
        {
            InitializeComponent();
            _repoPath = repoPath;
            
            // Set the ListView ItemSource
            CommitsList.ItemsSource = _selectedDateCommits;
            TagsList.ItemsSource = _tags;
            ReleasesList.ItemsSource = _releases;
            
            // Configure calendar to handle a wide range of dates
            // The default range might be too restrictive
            CommitsCalendar.DisplayDateStart = new DateTime(2000, 1, 1); // Lower bound
            CommitsCalendar.DisplayDateEnd = DateTime.Now.AddYears(10); // Upper bound
            
            // Hook the visual-refresh events for calendar highlighting
            CommitsCalendar.Loaded += (_, __) => MarkCommitDays();
            CommitsCalendar.DisplayDateChanged += (_, __) => MarkCommitDays();
            
            // Load data asynchronously
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load configuration first
                var config = await ConfigManager.LoadOrCreateAsync(_configFilePath);
                
                // Load Git data and update config simultaneously
                await LoadGitCommitsAsync(config);
                await LoadGitTagsAsync(config);
                await LoadGitReleasesAsync(config);
                
                // Save updated configuration
                await ConfigManager.SaveAsync(config, _configFilePath);
                
                // Update UI
                HighlightCommitDates();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading git data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Keep the old method for backward compatibility but make it use the async version
        private void LoadData()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadGitCommitsAsync(RepoConfig config)
        {
            try
            {
                _commitsByDate.Clear();
                
                // Get commits from GitHelper (use a higher limit like 100 to show more history)
                string commitsJson = GitHelper.GitHelper.GetGitCommitsDetailed(_repoPath, "HEAD", 100);
                var commitsContainer = JsonSerializer.Deserialize<CommitsContainer>(commitsJson);
                
                if (commitsContainer?.Items == null)
                {
                    System.Windows.MessageBox.Show("Failed to load commits or no commits found.", "Git Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Group commits by date for UI display
                foreach (var commit in commitsContainer.Items)
                {
                    // Parse the ISO date
                    if (DateTime.TryParse(commit.Date, out DateTime commitDate))
                    {
                        // Extract just the date part (no time)
                        DateTime dateOnly = commitDate.Date;
                        
                        // Create view model
                        var commitViewModel = new CommitViewModel
                        {
                            Hash = commit.Hash,
                            Author = commit.Author,
                            Title = commit.Title,
                            Body = commit.Body,
                            FullDate = commit.Date,
                            Date = dateOnly.ToShortDateString(),
                            Time = commitDate.ToString("HH:mm")
                        };
                        
                        // Add to dictionary
                        if (!_commitsByDate.ContainsKey(dateOnly))
                        {
                            _commitsByDate[dateOnly] = new List<CommitViewModel>();
                        }
                        
                        _commitsByDate[dateOnly].Add(commitViewModel);
                    }
                }
                
                // Update config file with commits data
                if (config?.Repo != null && config.Repo.Count > 0)
                {
                    // Find the repository that matches our current repo path
                    var repo = config.Repo.FirstOrDefault(r => r.Path == _repoPath);
                    
                    // If repo doesn't exist, create it
                    if (repo == null)
                    {
                        repo = new Repo
                        {
                            Name = Path.GetFileName(_repoPath),
                            Path = _repoPath,
                            Branch = "HEAD",
                            BuildNumber = 1,
                            Sections = new List<Section>()
                        };
                        config.Repo.Add(repo);
                    }
                    
                    // Find or create the Dashboard section
                    var dashboardSection = repo.Sections.FirstOrDefault(s => s.Name == _dashboardSectionName);
                    if (dashboardSection == null)
                    {
                        dashboardSection = new Section
                        {
                            Name = _dashboardSectionName,
                            Icon = ""
                        };
                        repo.Sections.Add(dashboardSection);
                    }
                    
                    // Update commits in the dashboard section
                    if (dashboardSection.Commits == null)
                    {
                        dashboardSection.Commits = new Commits();
                    }
                    
                    // Set the limit and copy commit items from the JSON response
                    dashboardSection.Commits.Limit = commitsContainer.Limit;
                    dashboardSection.Commits.Items = new List<CommitItem>();
                    
                    foreach (var commit in commitsContainer.Items)
                    {
                        dashboardSection.Commits.Items.Add(new CommitItem
                        {
                            Date = commit.Date,
                            Title = commit.Title,
                            Body = commit.Body,
                            Author = commit.Author,
                            Hash = commit.Hash
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading Git commits: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Keep the original method for backward compatibility
        private void LoadGitCommits()
        {
            try
            {
                _commitsByDate.Clear();
                
                // Get commits from GitHelper (use a higher limit like 100 to show more history)
                string commitsJson = GitHelper.GitHelper.GetGitCommitsDetailed(_repoPath, "HEAD", 100);
                var commitsContainer = JsonSerializer.Deserialize<CommitsContainer>(commitsJson);
                
                if (commitsContainer?.Items == null)
                {
                    System.Windows.MessageBox.Show("Failed to load commits or no commits found.", "Git Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Group commits by date
                foreach (var commit in commitsContainer.Items)
                {
                    // Parse the ISO date
                    if (DateTime.TryParse(commit.Date, out DateTime commitDate))
                    {
                        // Extract just the date part (no time)
                        DateTime dateOnly = commitDate.Date;
                        
                        // Create view model
                        var commitViewModel = new CommitViewModel
                        {
                            Hash = commit.Hash,
                            Author = commit.Author,
                            Title = commit.Title,
                            Body = commit.Body,
                            FullDate = commit.Date,
                            Date = dateOnly.ToShortDateString(),
                            Time = commitDate.ToString("HH:mm")
                        };
                        
                        // Add to dictionary
                        if (!_commitsByDate.ContainsKey(dateOnly))
                        {
                            _commitsByDate[dateOnly] = new List<CommitViewModel>();
                        }
                        
                        _commitsByDate[dateOnly].Add(commitViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading Git commits: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadGitTagsAsync(RepoConfig config)
        {
            try
            {
                _tags.Clear();
                
                // Get tags from GitHelper
                string tagsJson = GitHelper.GitHelper.GetGitTagsDetailed(_repoPath);
                var tagsContainer = JsonSerializer.Deserialize<TagsContainer>(tagsJson);
                
                if (tagsContainer?.Items == null || tagsContainer.Items.Count == 0)
                {
                    // Add placeholder item to indicate no tags
                    _tags.Add(new TagViewModel
                    {
                        Name = "No tags found",
                        Hash = "",
                        Date = "",
                        Message = "This repository has no tags.",
                        CommitAuthor = "",
                        Tagger = "",
                        Type = ""
                    });
                    
                    // No tags to update in config
                    return;
                }

                // Add to observable collection for UI
                foreach (var tag in tagsContainer.Items)
                {
                    DateTime.TryParse(tag.Date, out DateTime tagDate);
                    
                    _tags.Add(new TagViewModel
                    {
                        Name = tag.Name,
                        Hash = tag.Hash,
                        Date = tagDate.ToString("dd-MM-yy"),
                        Message = tag.Message,
                        CommitAuthor = tag.CommitAuthor,
                        Tagger = tag.Tagger,
                        Type = tag.Type
                    });
                }
                
                // Update configuration with tags data
                if (config?.Repo != null && config.Repo.Count > 0)
                {
                    // Find the repository that matches our current repo path
                    var repo = config.Repo.FirstOrDefault(r => r.Path == _repoPath);
                    
                    // If repo doesn't exist, create it
                    if (repo == null)
                    {
                        repo = new Repo
                        {
                            Name = Path.GetFileName(_repoPath),
                            Path = _repoPath,
                            Branch = "HEAD",
                            BuildNumber = 1,
                            Sections = new List<Section>()
                        };
                        config.Repo.Add(repo);
                    }
                    
                    // Find or create the Dashboard section
                    var dashboardSection = repo.Sections.FirstOrDefault(s => s.Name == _dashboardSectionName);
                    if (dashboardSection == null)
                    {
                        dashboardSection = new Section
                        {
                            Name = _dashboardSectionName,
                            Icon = ""
                        };
                        repo.Sections.Add(dashboardSection);
                    }
                    
                    // Initialize or clear the tags list
                    if (dashboardSection.Tags == null)
                    {
                        dashboardSection.Tags = new List<Tag>();
                    }
                    else
                    {
                        dashboardSection.Tags.Clear();
                    }
                    
                    // Add all tags from the JSON response
                    foreach (var tag in tagsContainer.Items)
                    {
                        dashboardSection.Tags.Add(new Tag
                        {
                            Name = tag.Name,
                            Date = tag.Date,
                            Hash = tag.Hash,
                            Message = tag.Message,
                            CommitAuthor = tag.CommitAuthor,
                            Tagger = tag.Tagger,
                            Type = tag.Type
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading Git tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Keep the original method for backward compatibility
        private void LoadGitTags()
        {
            try
            {
                _tags.Clear();
                
                // Get tags from GitHelper
                string tagsJson = GitHelper.GitHelper.GetGitTagsDetailed(_repoPath);
                var tagsContainer = JsonSerializer.Deserialize<TagsContainer>(tagsJson);
                
                if (tagsContainer?.Items == null || tagsContainer.Items.Count == 0)
                {
                    // Add placeholder item to indicate no tags
                    _tags.Add(new TagViewModel
                    {
                        Name = "No tags found",
                        Hash = "",
                        Date = "",
                        Message = "This repository has no tags.",
                        CommitAuthor = "",
                        Tagger = "",
                        Type = ""
                    });
                    return;
                }

                // Add to observable collection
                foreach (var tag in tagsContainer.Items)
                {
                    DateTime.TryParse(tag.Date, out DateTime tagDate);
                    
                    _tags.Add(new TagViewModel
                    {
                        Name = tag.Name,
                        Hash = tag.Hash,
                        Date = tagDate.ToString("dd-MM-yy"),
                        Message = tag.Message,
                        CommitAuthor = tag.CommitAuthor,
                        Tagger = tag.Tagger,
                        Type = tag.Type
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading Git tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadGitReleasesAsync(RepoConfig config)
        {
            try
            {
                _releases.Clear();
                
                // Get releases from GitHelper
                string releasesJson = GitHelper.GitHelper.GetGitReleasesDetailed(_repoPath);
                var releasesContainer = JsonSerializer.Deserialize<ReleasesContainer>(releasesJson);
                
                if (releasesContainer?.Items == null || releasesContainer.Items.Count == 0)
                {
                    // Add placeholder item to indicate no releases
                    _releases.Add(new ReleaseViewModel
                    {
                        Name = "No releases found",
                        Hash = "",
                        Date = "",
                        Message = "This repository has no releases.",
                        CommitAuthor = "",
                        Releaser = "",
                        Type = ""
                    });
                    
                    // No releases to update in config
                    return;
                }

                // Add to observable collection for UI
                foreach (var release in releasesContainer.Items)
                {
                    DateTime.TryParse(release.Date, out DateTime releaseDate);
                    
                    _releases.Add(new ReleaseViewModel
                    {
                        Name = release.Name,
                        Hash = release.Hash,
                        Date = releaseDate.ToString("dd-MM-yy"),
                        Message = release.Message,
                        CommitAuthor = release.CommitAuthor,
                        Releaser = release.Releaser,
                        Type = release.Type
                    });
                }
                
                // Update configuration with releases data
                if (config?.Repo != null && config.Repo.Count > 0)
                {
                    // Find the repository that matches our current repo path
                    var repo = config.Repo.FirstOrDefault(r => r.Path == _repoPath);
                    
                    // If repo doesn't exist, create it
                    if (repo == null)
                    {
                        repo = new Repo
                        {
                            Name = Path.GetFileName(_repoPath),
                            Path = _repoPath,
                            Branch = "HEAD",
                            BuildNumber = 1,
                            Sections = new List<Section>()
                        };
                        config.Repo.Add(repo);
                    }
                    
                    // Find or create the Dashboard section
                    var dashboardSection = repo.Sections.FirstOrDefault(s => s.Name == _dashboardSectionName);
                    if (dashboardSection == null)
                    {
                        dashboardSection = new Section
                        {
                            Name = _dashboardSectionName,
                            Icon = ""
                        };
                        repo.Sections.Add(dashboardSection);
                    }
                    
                    // Add releases to the dashboard section (since Releases is not part of the standard schema,
                    // we'll store them as Tags with appropriate metadata)
                    if (dashboardSection.Tags == null)
                    {
                        dashboardSection.Tags = new List<Tag>();
                    }
                    
                    // Find and remove any existing release tags to avoid duplicates
                    var releaseTags = dashboardSection.Tags.Where(t => 
                            (t.Name.StartsWith("v") && char.IsDigit(t.Name.Skip(1).FirstOrDefault())) || 
                            char.IsDigit(t.Name.FirstOrDefault()))
                        .ToList();
                    
                    foreach (var releaseTag in releaseTags)
                    {
                        dashboardSection.Tags.Remove(releaseTag);
                    }
                    
                    // Add all releases as tags with a specific format/type
                    foreach (var release in releasesContainer.Items)
                    {
                        dashboardSection.Tags.Add(new Tag
                        {
                            Name = release.Name,
                            Date = release.Date,
                            Hash = release.Hash,
                            Message = release.Message,
                            CommitAuthor = release.CommitAuthor,
                            Tagger = release.Releaser, // Use Releaser as Tagger
                            Type = "release" // Mark as a release tag specifically
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading Git releases: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Keep the original method for backward compatibility
        private void LoadGitReleases()
        {
            try
            {
                _releases.Clear();
                
                // Get releases from GitHelper
                string releasesJson = GitHelper.GitHelper.GetGitReleasesDetailed(_repoPath);
                var releasesContainer = JsonSerializer.Deserialize<ReleasesContainer>(releasesJson);
                
                if (releasesContainer?.Items == null || releasesContainer.Items.Count == 0)
                {
                    // Add placeholder item to indicate no releases
                    _releases.Add(new ReleaseViewModel
                    {
                        Name = "No releases found",
                        Hash = "",
                        Date = "",
                        Message = "This repository has no releases.",
                        CommitAuthor = "",
                        Releaser = "",
                        Type = ""
                    });
                    return;
                }

                // Add to observable collection
                foreach (var release in releasesContainer.Items)
                {
                    DateTime.TryParse(release.Date, out DateTime releaseDate);
                    
                    _releases.Add(new ReleaseViewModel
                    {
                        Name = release.Name,
                        Hash = release.Hash,
                        Date = releaseDate.ToString("dd-MM-yy"),
                        Message = release.Message,
                        CommitAuthor = release.CommitAuthor,
                        Releaser = release.Releaser,
                        Type = release.Type
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading Git releases: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HighlightCommitDates()
        {
            // Set the calendar to show the month of the most recent commit
            if (_commitsByDate.Count > 0)
            {
                // Get the most recent commit date
                var mostRecentDate = _commitsByDate.Keys.Max();
                CommitsCalendar.DisplayDate = mostRecentDate;
                
                // Select the most recent date to show its commits
                CommitsCalendar.SelectedDate = mostRecentDate;
                DisplayCommitsForDate(mostRecentDate);
            }
            
            // Apply visual highlighting to dates with commits
            MarkCommitDays();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Use the async version
            _ = LoadDataAsync();
        }

        private void CommitsCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CommitsCalendar.SelectedDate.HasValue)
            {
                DateTime selectedDate = CommitsCalendar.SelectedDate.Value.Date;
                DisplayCommitsForDate(selectedDate);
                
                // Update the UI to show we're displaying commits for this date
                CommitsListHeader.Text = $"Commits for {selectedDate.ToShortDateString()}";
            }
            else
            {
                _selectedDateCommits.Clear();
                CommitsListHeader.Text = "Commits for Selected Date";
            }
        }
        
        private void DisplayCommitsForDate(DateTime date)
        {
            _selectedDateCommits.Clear();
            
            if (_commitsByDate.ContainsKey(date))
            {
                // Add commits for selected date to the observable collection
                foreach (var commit in _commitsByDate[date])
                {
                    _selectedDateCommits.Add(commit);
                }
            }
        }

        private void CommitsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCommit = CommitsList.SelectedItem as CommitViewModel;
            if (selectedCommit != null)
            {
                DetailsHeader.Text = "Commit Details";
                DetailsTextBox.Text = $"Hash: {selectedCommit.Hash}\n" +
                                      $"Author: {selectedCommit.Author}\n" +
                                      $"Date: {selectedCommit.FullDate}\n" +
                                      $"Title: {selectedCommit.Title}\n\n" +
                                      $"Message:\n{selectedCommit.Body}";
            }
        }

        private void TagsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTag = TagsList.SelectedItem as TagViewModel;
            if (selectedTag != null)
            {
                DetailsHeader.Text = "Tag Details";
                DetailsTextBox.Text = $"Name: {selectedTag.Name}\n" +
                                      $"Hash: {selectedTag.Hash}\n" +
                                      $"Date: {selectedTag.Date}\n" +
                                      $"Message: {selectedTag.Message}\n" +
                                      $"Commit Author: {selectedTag.CommitAuthor}\n" +
                                      $"Tagger: {selectedTag.Tagger}\n" +
                                      $"Type: {selectedTag.Type}";
            }
        }

        private void ReleasesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRelease = ReleasesList.SelectedItem as ReleaseViewModel;
            if (selectedRelease != null)
            {
                DetailsHeader.Text = "Release Details";
                DetailsTextBox.Text = $"Name: {selectedRelease.Name}\n" +
                                      $"Hash: {selectedRelease.Hash}\n" +
                                      $"Date: {selectedRelease.Date}\n" +
                                      $"Message: {selectedRelease.Message}\n" +
                                      $"Commit Author: {selectedRelease.CommitAuthor}\n" +
                                      $"Releaser: {selectedRelease.Releaser}\n" +
                                      $"Type: {selectedRelease.Type}";
            }
        }
        
        /// <summary>Paints every CalendarDayButton that has commits.</summary>
        private void MarkCommitDays()
        {
            if (CommitsCalendar.Template.FindName("PART_CalendarItem", CommitsCalendar) 
                is not CalendarItem calItem) return;

            calItem.ApplyTemplate();   // ensures the buttons exist

            foreach (CalendarDayButton btn in FindDayButtons(calItem))
            {
                if (btn.DataContext is DateTime d &&
                    _commitsByDate.ContainsKey(d.Date))
                {
                    btn.Background = System.Windows.Media.Brushes.DodgerBlue;
                    btn.Foreground = System.Windows.Media.Brushes.White;
                    btn.FontWeight = FontWeights.Bold;
                }
                else   // reset normal days
                {
                    btn.ClearValue(BackgroundProperty);
                    btn.ClearValue(ForegroundProperty);
                    //btn.ClearValue(FontWeightProperty);
                }
            }
        }

        /// <summary>Depth-first search for CalendarDayButton children.</summary>
        private static IEnumerable<CalendarDayButton> FindDayButtons(DependencyObject root)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is CalendarDayButton btn) yield return btn;
                foreach (var sub in FindDayButtons(child)) yield return sub;
            }
        }
    }

    // View models for data binding
    public class CommitViewModel
    {
        public string? Hash { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? FullDate { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
    }

    public class TagViewModel
    {
        public string? Name { get; set; }
        public string? Hash { get; set; }
        public string? Date { get; set; }
        public string? Message { get; set; }
        public string? CommitAuthor { get; set; }
        public string? Tagger { get; set; }
        public string? Type { get; set; }
    }

    public class ReleaseViewModel
    {
        public string? Name { get; set; }
        public string? Hash { get; set; }
        public string? Date { get; set; }
        public string? Message { get; set; }
        public string? CommitAuthor { get; set; }
        public string? Releaser { get; set; }
        public string? Type { get; set; }
    }

    // Container classes for JSON deserialization
    public class CommitsContainer
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        
        [JsonPropertyName("items")]
        public List<GitHelper.GitHelper.CommitInfo>? Items { get; set; }
    }

    public class TagsContainer
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        
        [JsonPropertyName("items")]
        public List<GitHelper.GitHelper.GitTag>? Items { get; set; }
    }

    public class ReleasesContainer
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        
        [JsonPropertyName("items")]
        public List<GitHelper.GitHelper.GitRelease>? Items { get; set; }
    }
}
