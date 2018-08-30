using System;
using System.IO;
using System.Windows.Forms;


namespace DSLRBackup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFolders();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Compare();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MakeBackup();
        }

        private void LoadFolders()
        {
            if(File.Exists(Application.StartupPath + "\\lastUse.txt"))
            {
                var lineas = File.ReadAllLines(Application.StartupPath + "\\lastUse.txt");

                textBox1.Text = lineas[0];
                textBox2.Text = lineas[1];
            }
        }
        
        private void SaveFolders()
        {
            if (File.Exists(Application.StartupPath + "\\lastUse.txt"))
                File.Delete(Application.StartupPath + "\\lastUse.txt");

            var lineas = new string[2];

            lineas[0] = textBox1.Text;
            lineas[1] = textBox2.Text;

            File.WriteAllLines(Application.StartupPath + "\\lastUse.txt", lineas);
        }

        private string GetDirectoryName(string fullpath)
        {
            return fullpath.Substring(fullpath.LastIndexOf("\\")+1).ToUpper();
        }

        private void Compare()
        {
            try
            {
                treeView1.Nodes.Clear();

                var originFolders = Directory.GetDirectories(textBox1.Text);
                var targetFolders = Directory.GetDirectories(textBox2.Text);

                SaveFolders();

                foreach (string folder in originFolders)
                {
                    if (!Directory.Exists(textBox2.Text + "\\" + GetDirectoryName(folder)))
                    {
                        treeView1.Nodes.Add(GetDirectoryName(folder) + "   *new*");
                    }
                    else
                    {
                        CompareFolder(folder);
                    }

                }

                foreach (string folder in targetFolders)
                {
                    if (!Directory.Exists(textBox1.Text + "\\" + GetDirectoryName(folder)))
                        treeView1.Nodes.Add(GetDirectoryName(folder) + "   *deleted*");

                }

                //Compare files
                var originFiles = Directory.GetFiles(textBox1.Text);
                var targetFiles = Directory.GetFiles(textBox2.Text);

                foreach (string file in originFiles)
                {
                    string targetFile = file.Replace(textBox1.Text, textBox2.Text);
                    if (File.Exists(targetFile))
                    {
                        FileInfo fiOrigen = new FileInfo(file);
                        FileInfo fiDest = new FileInfo(targetFile);

                        if (fiOrigen.Length != fiDest.Length || fiOrigen.LastWriteTime != fiDest.LastWriteTime)
                            treeView1.Nodes.Add(file.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *modified*");
                    }
                    else
                    {
                        treeView1.Nodes.Add(file.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *new file*");
                    }
                }

                foreach (string file in targetFiles)
                {
                    string originFile = file.Replace(textBox2.Text, textBox1.Text);
                    if (!File.Exists(originFile))
                    {
                        treeView1.Nodes.Add(file.Replace(textBox2.Text, "").Substring(1).ToUpper() + "   *deleted file*");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CompareFolder(string originPath)
        {
            var targetPath = originPath.Replace(textBox1.Text, textBox2.Text);

            var originSubFolders = Directory.GetDirectories(originPath);

            //recursive call
            foreach (string subFolder in originSubFolders)
            {
                CompareFolder(subFolder);
            }

            if (Directory.Exists(targetPath))
            {
                //Compare files
                var originFiles = Directory.GetFiles(originPath);
                var targetFiles = Directory.GetFiles(targetPath);

                foreach (string file in originFiles)
                {
                    string targetFile = file.Replace(textBox1.Text, textBox2.Text);
                    if (File.Exists(targetFile))
                    {
                        FileInfo fiOrigen = new FileInfo(file);
                        FileInfo fiDest = new FileInfo(targetFile);

                        if (fiOrigen.Length != fiDest.Length || fiOrigen.LastWriteTime != fiDest.LastWriteTime)
                            treeView1.Nodes.Add(file.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *modified*");
                    }
                    else
                    {
                        treeView1.Nodes.Add(file.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *new file*");
                    }
                }

                foreach (string file in targetFiles)
                {
                    string originFile = file.Replace(textBox2.Text, textBox1.Text);
                    if (!File.Exists(originFile))
                    {
                        treeView1.Nodes.Add(file.Replace(textBox2.Text, "").Substring(1).ToUpper() + "   *deleted file*");
                    }
                }
            }
            else
            {
                treeView1.Nodes.Add(originPath.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *new*");

            }
        }

        private void MakeBackup()
        {
            try
            {

                foreach (TreeNode node in treeView1.Nodes)
                {

                    if (node.Text.Contains("*new*")) //new folder: copy all content
                    {
                        string pathOrigen = textBox1.Text + "\\" + node.Text.Replace("*new*", "").TrimEnd();
                        string pathDestino = textBox2.Text + "\\" + node.Text.Replace("*new*", "").TrimEnd();

                        //Create folder
                        Directory.CreateDirectory(textBox2.Text + "\\" + node.Text.Replace("*new*", "").TrimEnd());

                        //Copy all folder files and subfolders to the new folder
                        foreach (string dirPath in Directory.GetDirectories(pathOrigen, "*", SearchOption.AllDirectories))
                            Directory.CreateDirectory(dirPath.Replace(pathOrigen, pathDestino));

                        foreach (string newPath in Directory.GetFiles(pathOrigen, "*.*", SearchOption.AllDirectories))
                        {
                            label3.Text = pathOrigen.Replace(textBox1.Text, "").TrimEnd(); this.Refresh();
                            File.Copy(newPath, newPath.Replace(pathOrigen, pathDestino), true);
                        }
                    }
                    if (node.Text.Contains("*new file*"))
                    {
                        string ficheroOrigen = textBox1.Text + "\\" + node.Text.Replace("*new file*", "").TrimEnd();
                        string ficheroDestino = textBox2.Text + "\\" + node.Text.Replace("*new file*", "").TrimEnd();

                        label3.Text = ficheroOrigen.Replace(textBox1.Text, ""); this.Refresh();
                        File.Copy(ficheroOrigen, ficheroDestino);
                    }
                    if (node.Text.Contains("*modified*"))
                    {
                        string ficheroOrigen = textBox1.Text + "\\" + node.Text.Replace("*modified*", "").TrimEnd();
                        string ficheroDestino = textBox2.Text + "\\" + node.Text.Replace("*modified*", "").TrimEnd();

                        label3.Text = ficheroOrigen.Replace(textBox1.Text, ""); this.Refresh();
                        File.Copy(ficheroOrigen, ficheroDestino, true);
                    }

                    if(radioButton2.Checked && node.Text.Contains("*deleted*"))
                    {
                        string targetFolder = textBox2.Text + "\\" + node.Text.Replace("*deleted*", "").TrimEnd();
                        Directory.Delete(targetFolder);
                    }

                    if (radioButton2.Checked && node.Text.Contains("*deleted file*"))
                    {
                        string targetFile = textBox2.Text + "\\" + node.Text.Replace("*deleted file*", "").TrimEnd();
                        File.Delete(targetFile);
                    }
                }

                label3.Text = "";
                MessageBox.Show("Job done!");
                Compare();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        

    }
}
