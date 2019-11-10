using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace PT2
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow : Window
    {
        public bool create = false;
        public string nameToCreate;
        public bool? createFile, createDirectory, r, a, h, s;
        public CreateWindow()
        {
            InitializeComponent();
        }

        private void CreateFileSys(object sender, EventArgs e)
        {
            create = true;
            nameToCreate = FileSystemName.GetLineText(0);
            bool isMatch = Regex.IsMatch(nameToCreate, "^[a-zA-Z_0-9~-]{1,8}\\.\\b(?:html|php|txt)");
            createFile = FileButton.IsChecked;
            createDirectory = DirectoryButton.IsChecked;
            r = readOnly.IsChecked;
            a = archive.IsChecked;
            h = hidden.IsChecked;
            s = system.IsChecked;
            if (!isMatch && createFile == true)
            {
                WarningWindow warning = new WarningWindow();
                warning.ShowDialog();
                return;
            }
            Close();
        }
        private void CloseCreateWindow (object sender, EventArgs e)
        {
            create = false;
            Close();
        }
    }
}
