using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms; // For FolderBrowserDialog with alias
using ConfigHelper; // Add reference to our ConfigHelper library

namespace Interface
{
    public partial class MainWindow : Window
    {
        // Path to the configuration file
        private readonly string _configFilePath;
        
        // Configuration object
        private RepoConfig? _config;
        // Add reference to Windows Forms for the folder browser dialog
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public MainWindow()
        {
            InitializeComponent();
            
            // Set the config file path
            _configFilePath = System.IO.Path.Combine(AppContext.BaseDirectory, "sectionDataObject.json");
            
            // Load the configuration file asynchronously
            LoadConfigAsync();
        }
        
        /// <summary>
        /// Loads the configuration file asynchronously
        /// </summary>
        private async void LoadConfigAsync()
        {
            try
            {
                // Load or create the configuration file
                _config = await ConfigManager.LoadOrCreateAsync(_configFilePath);
                
                // Update the UI with the loaded configuration
                UpdateUIFromConfig();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading configuration: {ex.Message}", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Updates the UI with the loaded configuration
        /// </summary>
        private void UpdateUIFromConfig()
        {
            // Clear existing items
            RepoSelector.Items.Clear();
            
            // Add repositories to the selector
            if (_config?.Repo != null && _config.Repo.Count > 0)
            {
                foreach (var repo in _config.Repo)
                {
                    RepoSelector.Items.Add(repo.Name);
                }
                
                // Select the first repository
                if (RepoSelector.Items.Count > 0)
                {
                    RepoSelector.SelectedIndex = 0;
                }
            }
        }
        
        /// <summary>
        /// Saves the configuration file asynchronously
        /// </summary>
        private async Task SaveConfigAsync()
        {
            try
            {
                await ConfigManager.SaveAsync(_config, _configFilePath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving configuration: {ex.Message}", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── File-menu: Exit ────────────────────────────────
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();            // shut the whole app
        }

        // ── Left-panel: Add Project button ────────────────
        private async void AddProject_Click(object sender, RoutedEventArgs e)
        {
            // Create folder browser dialog
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "Select TIA Portal project folder";
                dialog.UseDescriptionForTitle = true; // This property is available in .NET 5+ (verify your .NET version)
                dialog.ShowNewFolderButton = false;
                
                // Show the dialog and check if user clicked OK
                WinForms.DialogResult result = dialog.ShowDialog();
                
                if (result == WinForms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    // User selected a folder
                    string selectedPath = dialog.SelectedPath;
                    
                    // Create a new repository entry
                    var newRepo = new Repo
                    {
                        Name = System.IO.Path.GetFileName(selectedPath),
                        Path = selectedPath,
                        Branch = "main", // Default branch
                        BuildNumber = 1,
                        Sections = new List<ConfigHelper.Section>
                        {
                            new ConfigHelper.Section { 
                                Name = "Dashboard", 
                                Icon = "", 
                                Commits = new Commits { Limit = 10, Items = new List<CommitItem>() }, 
                                Tags = new List<Tag>() 
                            },
                            new ConfigHelper.Section { 
                                Name = "Project Configuration", 
                                Icon = "", 
                                Devices = new List<Device>() 
                            },
                            new ConfigHelper.Section { 
                                Name = "Datablocks", 
                                Icon = "", 
                                Datablocks = new List<Datablock>() 
                            },
                            new ConfigHelper.Section { 
                                Name = "Memory Tags", 
                                Icon = "" 
                                // Don't initialize the Tags property at this point as it's DbTag, not Tag
                            }
                        }
                    };
                    
                    // Add the new repository to the configuration
                    if (_config.Repo == null)
                    {
                        _config.Repo = new List<Repo>();
                    }
                    _config.Repo.Add(newRepo);
                    
                    // Save the configuration
                    await SaveConfigAsync();
                    
                    // Update the UI
                    UpdateUIFromConfig();
                    
                    // Select the newly added repository
                    if (newRepo.Name != null)
                        RepoSelector.SelectedItem = newRepo.Name;
                    
                    System.Windows.MessageBox.Show($"Project added: {newRepo.Name}", "Project Added", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // ── Left-panel: repository listbox ────────────────
        private void RepoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This method is kept for backward compatibility
            // The functionality is now handled in RepoSelector_SelectionChanged
        }

        // ── Left-panel: section menu listbox ──────────────
        private void SectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SectionMenu.SelectedItem == null)
                return;
                
            // Get the selected section name
            string sectionName = SectionMenu.SelectedItem.ToString();
            
            // TODO: navigate RightFrame to the page for the chosen section
            // This will be implemented in a future update
        }

        private void RepoSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RepoSelector.SelectedItem == null || _config?.Repo == null)
                return;
                
            // Get the selected repository name
            string? repoName = RepoSelector.SelectedItem.ToString();
            if (repoName == null)
                return;
                
            // Find the repository in the configuration
            var selectedRepo = _config.Repo.FirstOrDefault(r => r.Name == repoName);
            if (selectedRepo == null)
                return;
                
            // Clear existing items in the section menu
            SectionMenu.Items.Clear();
            
            // Add sections to the section menu
            if (selectedRepo.Sections != null && selectedRepo.Sections.Count > 0)
            {
                foreach (var section in selectedRepo.Sections)
                {
                    SectionMenu.Items.Add(section.Name);
                }
                
                // Select the first section
                if (SectionMenu.Items.Count > 0)
                {
                    SectionMenu.SelectedIndex = 0;
                }
            }
        }
    }
}
