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

namespace YASEL_Exporter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox1.Text = folderBrowserDialog1.SelectedPath;
            RefreshList();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBox1.Text;
            RefreshList();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
        private void RefreshList()
        {
            listBox1.Items.Clear();
            if (textBox1.Text == "")
                return;
            var dirs = Directory.EnumerateDirectories(textBox1.Text,"Programs",SearchOption.AllDirectories);
            foreach(string d in dirs)
            {
                AddFilesToList(d);
            }
            if (listBox1.Items.Count >= (int)listBox1.Tag)
                listBox1.SelectedIndex = (int)listBox1.Tag;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CopyCode();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CopyCode();
        }

        private void AddFilesToList(string d)
        {

            DirectoryInfo di = new DirectoryInfo(d);
            var files = di.EnumerateFiles("*.cs");
            foreach (FileInfo file in files)
            {
                listBox1.Items.Add(d.Replace(textBox1.Text + "\\", "") + "\\" + file.ToString());
            }
        }

        private void CopyCode()
        {
            if (listBox1.SelectedIndex == -1)
                return;
            string output = "";
            Dictionary<string, string> includeFiles = new Dictionary<string,string>();

            output = GetFileCode(new FileInfo(textBox1.Text + "\\" + listBox1.SelectedItem.ToString()));

            
            
           // try
           // {
                includeFiles = GetRequires(output);
           // } catch (Exception e)
           // {
             //   MessageBox.Show(e.Message,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                //return;
          //  }
            var fEnum = includeFiles.GetEnumerator();
            while(fEnum.MoveNext())
            {
                output += fEnum.Current.Value;
            }
            
            Clipboard.SetText(Clean(output));
        }
        private string GetFileCode(FileInfo codeFile)
        {
            var sr = codeFile.OpenText();
            string text = sr.ReadToEnd();
            text = text.Remove(0, text.IndexOf("#region " + codeFile.Name.Replace(".cs", "")) + ("#region " + codeFile.Name.Replace(".cs", "")).Length);
            text = text.Remove(text.IndexOf("#endregion " + codeFile.Name.Replace(".cs", ""))!= -1 ?
                text.IndexOf("#endregion " + codeFile.Name.Replace(".cs", "")) :
                text.IndexOf("#endregion"));
            sr.Close();
            return text;
        }
        private Dictionary<string, string> GetRequires(string text)
        {
            Dictionary<string, string> includeFiles = new Dictionary<string, string>();
            int lastIndex = 0;
            int idx = text.IndexOf("//# Requires ", lastIndex);
            while (idx != -1)
            {
                
                string fileName = text.Substring(idx + ("//# Requires ").Length, text.IndexOf("\r\n", idx) - idx - ("//# Requires ").Length) + ".cs";
                if (!includeFiles.ContainsKey(fileName))
                {
                    var fList = Directory.EnumerateFiles(textBox1.Text, "*", SearchOption.AllDirectories).ToList();
                    bool found = false;
                    foreach (string fName in fList)
                    {
                        if (!fName.Contains("\\"+fileName))
                            continue;
                        found = true;
                        string code = GetFileCode(new FileInfo(fName));
                        includeFiles.Add(fileName, code);
                        includeFiles.Intersect(GetRequires(code));
                    }
                    if (!found) throw new Exception("Unable to load file:" + fileName);
                   
                }
                lastIndex = idx + "//# Requires ".Length;
                idx = text.IndexOf("//# Requires ", lastIndex);

            }
            return includeFiles;
        }
        private string Clean(string text)
        {
            string cleanText = text;
            while (text.IndexOf("//# Requires ") != -1)
            {
                int idx = text.IndexOf("//# Requires ");
                text = text.Replace(text.Substring(idx, text.IndexOf("\r\n", idx) - idx), "");
            }
            cleanText = text;
            if (checkBox1.Checked)
            {
                cleanText = cleanText.Replace("\t", " ");

                while(cleanText.Contains("  "))
                {
                    cleanText = cleanText.Replace("  ", " ");
                }

                cleanText = cleanText.Replace("\r\n ", "\r\n");

                while (cleanText.Contains("\r\n\r\n"))
                {
                    cleanText = cleanText.Replace("\r\n\r\n", "\r\n");
                }
                
            }
            return cleanText;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Tag = listBox1.SelectedIndex;
        }
    }
}
