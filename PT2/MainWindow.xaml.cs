using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace PT2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenDirectoryClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog() { Description = "Select directory" };
            dlg.ShowDialog();
            if (Directory.Exists(dlg.SelectedPath))
            {
                TreeViewItem root = new TreeViewItem()
                {
                    Header = Path.GetFileName(dlg.SelectedPath),
                    Tag = dlg.SelectedPath,
                    ContextMenu = new ContextMenu()
                };
                MenuItem menuItem1 = new MenuItem()
                {
                    Header = "Create",
                    Tag = root
                };
                menuItem1.Click += ClickCreate;
                MenuItem menuItem2 = new MenuItem()
                {
                    Header = "Delete"
                };
                root.Selected += SetRAHS;
                menuItem2.Click += ClickDelete;
                root.ContextMenu.Items.Add(menuItem1);
                root.ContextMenu.Items.Add(menuItem2);
                CreateTreeView(root, dlg.SelectedPath);
                trv.Items.Add(root);
            }
        }

        private void ExitAppClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateTreeView(TreeViewItem root, string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileSystemInfo[] sysOjects = di.GetFileSystemInfos();
            foreach (FileSystemInfo sysObj in sysOjects)
            {
                TreeViewItem item = new TreeViewItem()
                {
                    Header = Path.GetFileName(sysObj.FullName),
                    Tag = sysObj.FullName,
                    ContextMenu = new ContextMenu()
                };
                item.Selected += SetRAHS;
                root.Items.Add(item);
                if (sysObj is DirectoryInfo)
                {
                    MenuItem menuItem1 = new MenuItem() { Header = "Create", Tag = item };
                    menuItem1.Click += ClickCreate;
                    item.ContextMenu.Items.Add(menuItem1);
                    CreateTreeView(item, sysObj.FullName);
                }
                else if (sysObj is FileInfo)
                {
                    MenuItem menuItem1 = new MenuItem() { Header = "Open", Tag = item };
                    menuItem1.Click += ClickOpen;
                    item.ContextMenu.Items.Add(menuItem1);
                }
                MenuItem menuItem2 = new MenuItem() { Header = "Delete", Tag = item };
                menuItem2.Click += ClickDelete;
                item.ContextMenu.Items.Add(menuItem2);
            }
        }

        private void ClickCreate(object sender, EventArgs e)
        {
            TreeViewItem treeViewItem = (sender as MenuItem).Tag as TreeViewItem;
            string path = treeViewItem.Tag as string;
            CreateWindow window = new CreateWindow();
            window.ShowDialog();
            if (window.create == false)
                return;
            string createdFullName = path + "\\" + window.nameToCreate;
            TreeViewItem createdItem = new TreeViewItem()
            {
                Header = window.nameToCreate,
                Tag = createdFullName,
                ContextMenu = new ContextMenu()
            };
            createdItem.Selected += SetRAHS;
            if (window.createFile == true)
            {
                File.Create(createdFullName);
                MenuItem menuItem1 = new MenuItem() { Header = "Open", Tag = createdItem };
                menuItem1.Click += ClickOpen;
                createdItem.ContextMenu.Items.Add(menuItem1);  
            }
            else if (window.createDirectory == true)
            {
                Directory.CreateDirectory(createdFullName);
                MenuItem menuItem1 = new MenuItem() { Header = "Create", Tag = createdItem };
                menuItem1.Click += ClickCreate;
                createdItem.ContextMenu.Items.Add(menuItem1);
            }
            SetAttr(createdFullName, window);
            MenuItem menuItem2 = new MenuItem() { Header = "Delete", Tag = createdItem };
            menuItem2.Click += ClickDelete;
            createdItem.ContextMenu.Items.Add(menuItem2);
            treeViewItem.Items.Add(createdItem);
        }

        private void SetAttr(string path, CreateWindow window)
        {
            FileSystemInfo file;
            FileAttributes attributes = 0;
            if (window.r == true)
                attributes |= FileAttributes.ReadOnly;
            if (window.a == true)
                attributes |= FileAttributes.Archive;
            if (window.h == true)
                attributes |= FileAttributes.Hidden;
            if (window.s == true)
                attributes |= FileAttributes.System;
            if (File.Exists(path))
                file = new FileInfo(path);
            else
                file = new DirectoryInfo(path);
            file.Attributes = attributes;
        }
        private void ClickOpen(object sender, EventArgs e)
        {
            fileContent.Text = "";
            TreeViewItem treeView = (sender as MenuItem).Tag as TreeViewItem;
            string path = treeView.Tag as string;
            using (StreamReader stream = File.OpenText(path))
            {
                char[] buffer = new char[1024];
                while (stream.Read(buffer, 0, buffer.Length) > 0)
                {
                    fileContent.Text += new string(buffer);
                }
            }
        }

        private void ClickDelete(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            TreeViewItem viewItem = item.Tag as TreeViewItem;
            TreeViewItem root = viewItem.Parent as TreeViewItem;
            string path = viewItem.Tag as string;
            root.Items.Remove(viewItem);
            if (File.Exists(viewItem.Tag as string))
            {
                FileSystemInfo file = new FileInfo(path);
                RemoveReadOnly(file);
                File.Delete(path);
            }
            else if (Directory.Exists(path))
                DeleteDirectory(path);

        }

        private void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;
            DirectoryInfo di = new DirectoryInfo(path);
            FileSystemInfo[] systemInfos = di.GetFileSystemInfos();
            foreach (FileSystemInfo obj in systemInfos)
            {
                if (obj is FileInfo)
                {
                    RemoveReadOnly(obj);
                    File.Delete(obj.FullName);
                }
                else if (obj is DirectoryInfo)
                    DeleteDirectory(obj.FullName);
            }
            RemoveReadOnly(new DirectoryInfo(path));
            Directory.Delete(path);
        }

        private void RemoveReadOnly(FileSystemInfo fileSystem)
        {
            if ((fileSystem.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                fileSystem.Attributes &= ~FileAttributes.ReadOnly;
        }

        private void SetRAHS(object sender, EventArgs e)
        {
            char[] attr = { '-', '-', '-', '-' };
            FileSystemInfo file;
            string objPath = (trv.SelectedItem as TreeViewItem).Tag as string;
            if (File.Exists(objPath))
            {
                file = new FileInfo(objPath);
            }
            else
                file = new DirectoryInfo(objPath);
            if ((file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                attr[0] = 'r';
            if ((file.Attributes & FileAttributes.Archive) == FileAttributes.Archive)
                attr[1] = 'a';
            if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                attr[2] = 'h';
            if ((file.Attributes & FileAttributes.System) == FileAttributes.System)
                attr[3] = 's';
            txtBox.Text = new string(attr);
        }
    }
}
