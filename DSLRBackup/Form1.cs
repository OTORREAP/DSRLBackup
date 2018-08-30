using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


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
            CargarDirs();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                treeView1.Nodes.Clear(); 

                var dirsOrigen = Directory.GetDirectories(textBox1.Text);
                var dirsDestino = Directory.GetDirectories(textBox2.Text);

                GuardarDirs();

                foreach (string dirOrigen in dirsOrigen)
                {
                    if (!Directory.Exists(textBox2.Text + "\\" + GetDirectoryName(dirOrigen)))
                    {
                        treeView1.Nodes.Add(GetDirectoryName(dirOrigen) + "   *nuevo*");
                    }
                    else
                    {
                        CompararDirectorio(dirOrigen);
                    }

                }

                foreach (string dirDestino in dirsDestino)
                {
                    if (!Directory.Exists(textBox1.Text + "\\" + GetDirectoryName(dirDestino)))
                        treeView1.Nodes.Add(GetDirectoryName(dirDestino) + "   *borrado*");

                }

                //Comparar ficheros
                var ficherosOrigen = Directory.GetFiles(textBox1.Text);
                var ficherosDestino = Directory.GetFiles(textBox2.Text);

                foreach (string ficheroOrigen in ficherosOrigen)
                {
                    string ficheroDestino = ficheroOrigen.Replace(textBox1.Text, textBox2.Text);
                    if (File.Exists(ficheroDestino))
                    {
                        FileInfo fiOrigen = new FileInfo(ficheroOrigen);
                        FileInfo fiDest = new FileInfo(ficheroDestino);

                        if (fiOrigen.Length != fiDest.Length || fiOrigen.LastWriteTime != fiDest.LastWriteTime)
                            treeView1.Nodes.Add(ficheroOrigen.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *modificado*");
                    }
                    else
                    {
                        treeView1.Nodes.Add(ficheroOrigen.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *fichero nuevo*");
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CargarDirs()
        {
            if(File.Exists(Application.StartupPath + "\\config.txt"))
            {
                var lineas = File.ReadAllLines(Application.StartupPath + "\\config.txt");

                textBox1.Text = lineas[0];
                textBox2.Text = lineas[1];
            }
        }


        private void GuardarDirs()
        {
            if (File.Exists(Application.StartupPath + "\\config.txt"))
                File.Delete(Application.StartupPath + "\\config.txt");

            var lineas = new string[2];

            lineas[0] = textBox1.Text;
            lineas[1] = textBox2.Text;

            File.WriteAllLines(Application.StartupPath + "\\config.txt", lineas);
        }

        private string GetDirectoryName(string fullpath)
        {
            return fullpath.Substring(fullpath.LastIndexOf("\\")+1).ToUpper();
        }


        private void CompararDirectorio(string dirOrigen)
        {
            var dirDestino = dirOrigen.Replace(textBox1.Text, textBox2.Text);

            var subdirsOrigen = Directory.GetDirectories(dirOrigen);

            //Llamada recursiva
            foreach (string subDir in subdirsOrigen)
            {
                CompararDirectorio(subDir);
            }

            if(Directory.Exists(dirDestino))
            {
                //Comparar ficheros
                var ficherosOrigen = Directory.GetFiles(dirOrigen);
                var ficherosDestino = Directory.GetFiles(dirDestino);

                foreach(string ficheroOrigen in ficherosOrigen)
                {
                    string ficheroDestino = ficheroOrigen.Replace(textBox1.Text, textBox2.Text);
                    if (File.Exists(ficheroDestino))
                    {
                        FileInfo fiOrigen = new FileInfo(ficheroOrigen);
                        FileInfo fiDest = new FileInfo(ficheroDestino);

                        if(fiOrigen.Length != fiDest.Length || fiOrigen.LastWriteTime != fiDest.LastWriteTime)
                            treeView1.Nodes.Add(ficheroOrigen.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *modificado*");
                    }
                    else
                    {
                        treeView1.Nodes.Add(ficheroOrigen.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *fichero nuevo*");
                    }
                }
            }
            else
            {
                treeView1.Nodes.Add(dirOrigen.Replace(textBox1.Text, "").Substring(1).ToUpper() + "   *nuevo*");

            }
        }

        private void AgregrarNodo(string path, string accion)
        {
            var niveles = path.Split('\\');
                        
            foreach(string nivel in niveles)
            {
                

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {

                foreach (TreeNode node in treeView1.Nodes)
                {

                    if (node.Text.Contains("*nuevo*")) //nuevo directorio: copiar todo su contenido
                    {
                        string pathOrigen = textBox1.Text + "\\" + node.Text.Replace("*nuevo*", "").TrimEnd();
                        string pathDestino = textBox2.Text + "\\" + node.Text.Replace("*nuevo*", "").TrimEnd();

                        //Crear directorio
                        Directory.CreateDirectory(textBox2.Text + "\\" + node.Text.Replace("*nuevo*", "").TrimEnd());

                        //Copiar sus subdirectorios y ficheros
                        foreach (string dirPath in Directory.GetDirectories(pathOrigen, "*", SearchOption.AllDirectories))
                            Directory.CreateDirectory(dirPath.Replace(pathOrigen, pathDestino));

                        foreach (string newPath in Directory.GetFiles(pathOrigen, "*.*", SearchOption.AllDirectories))
                        {
                            label3.Text = pathOrigen.Replace(textBox1.Text, "").TrimEnd(); this.Refresh();
                            File.Copy(newPath, newPath.Replace(pathOrigen, pathDestino), true);
                        }
                    }
                    if (node.Text.Contains("*fichero nuevo*"))
                    {
                        string ficheroOrigen = textBox1.Text + "\\" + node.Text.Replace("*fichero nuevo*", "").TrimEnd();
                        string ficheroDestino = textBox2.Text + "\\" + node.Text.Replace("*fichero nuevo*", "").TrimEnd();

                        label3.Text = ficheroOrigen.Replace(textBox1.Text, ""); this.Refresh();
                        File.Copy(ficheroOrigen, ficheroDestino);
                    }
                    if (node.Text.Contains("*modificado*"))
                    {
                        string ficheroOrigen = textBox1.Text + "\\" + node.Text.Replace("*modificado*", "").TrimEnd();
                        string ficheroDestino = textBox2.Text + "\\" + node.Text.Replace("*modificado*", "").TrimEnd();

                        label3.Text = ficheroOrigen.Replace(textBox1.Text, ""); this.Refresh();
                        File.Copy(ficheroOrigen, ficheroDestino, true);
                    }
                }

                label3.Text = "";
                MessageBox.Show("Proceso completado");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
