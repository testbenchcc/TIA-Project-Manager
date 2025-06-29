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

namespace Interface
{
    public partial class MainWindow : Window
    {
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
            MessageBox.Show("Add-project dialog goes here");
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
