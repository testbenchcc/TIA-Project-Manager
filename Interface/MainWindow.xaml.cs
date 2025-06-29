using System.Text;
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

namespace Interface
{
    public partial class MainWindow : Window
    {
        // Add reference to Windows Forms for the folder browser dialog
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public MainWindow()
        {
            InitializeComponent();
        }

        // ── File-menu: Exit ────────────────────────────────
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();            // shut the whole app
        }

        // ── Left-panel: Add Project button ────────────────
        private void AddProject_Click(object sender, RoutedEventArgs e)
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
                    System.Windows.MessageBox.Show($"Selected folder: {selectedPath}\nImplement project loading logic here.");
                    
                    // TODO: Add logic to load the selected project
                }
            }
        }

        // ── Left-panel: repository listbox ────────────────
        private void RepoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Todo: load sections for the picked repo
        }

        // ── Left-panel: section menu listbox ──────────────
        private void SectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Todo: navigate RightFrame to the page for the chosen section
        }

        private void RepoSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Todo: load sections for the chosen repo
        }
    }
}
