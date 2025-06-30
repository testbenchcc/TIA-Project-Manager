using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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

        public GitInfoPage(string repoPath)
        {
            InitializeComponent();
            _repoPath = repoPath;
            
            // Set the ListView ItemSource
            CommitsList.ItemsSource = _selectedDateCommits;
            TagsList.ItemsSource = _tags;
            ReleasesList.ItemsSource = _releases;
            
            // Load data
            LoadData();
        }

        private void LoadData()
        {
            LoadGitCommits();
            LoadGitTags();
            LoadGitReleases();
            
            // Update UI
            HighlightCommitDates();
        }

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

        private void LoadGitTags()
        {
            try
            {
                _tags.Clear();
                
                // Get tags from GitHelper
                string tagsJson = GitHelper.GitHelper.GetGitTagsDetailed(_repoPath);
                var tagsContainer = JsonSerializer.Deserialize<TagsContainer>(tagsJson);
                
                if (tagsContainer?.Items == null)
                {
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
                        Date = tagDate.ToString("yyyy-MM-dd HH:mm"),
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

        private void LoadGitReleases()
        {
            try
            {
                _releases.Clear();
                
                // Get releases from GitHelper
                string releasesJson = GitHelper.GitHelper.GetGitReleasesDetailed(_repoPath);
                var releasesContainer = JsonSerializer.Deserialize<ReleasesContainer>(releasesJson);
                
                if (releasesContainer?.Items == null)
                {
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
                        Date = releaseDate.ToString("yyyy-MM-dd HH:mm"),
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
            // Clear any previous selections
            CommitsCalendar.SelectedDate = null;
            
            // BlackoutDates are used to highlight dates with commits
            CommitsCalendar.BlackoutDates.Clear();
            
            foreach (var date in _commitsByDate.Keys)
            {
                // In WPF Calendar, we use BlackoutDates for visual highlight 
                // but then handle it in the SelectedDatesChanged event
                var range = new CalendarDateRange(date);
                CommitsCalendar.BlackoutDates.Add(range);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void CommitsCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDateCommits.Clear();
            
            if (CommitsCalendar.SelectedDate.HasValue)
            {
                DateTime selectedDate = CommitsCalendar.SelectedDate.Value.Date;
                
                if (_commitsByDate.ContainsKey(selectedDate))
                {
                    // Add commits for selected date to the observable collection
                    foreach (var commit in _commitsByDate[selectedDate])
                    {
                        _selectedDateCommits.Add(commit);
                    }
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
