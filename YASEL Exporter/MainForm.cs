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
            var dirs = Directory.EnumerateDirectories(textBox1.Text, "Programs", SearchOption.AllDirectories);
            foreach (string d in dirs)
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
            Dictionary<string, string> includeFiles = new Dictionary<string, string>();

            output = GetFileCode(new FileInfo(textBox1.Text + "\\" + listBox1.SelectedItem.ToString()));



            try
            {
                GetRequires(output, includeFiles);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (output.Contains("MyGridProgram"))
            {
                output = output.Remove(0, output.IndexOf("{", output.IndexOf("MyGridProgram")) + 1);
                output = output.Remove(output.LastIndexOf("}") - 1);
            }
            output = output.Replace("\r\n    ", "\r\n");
            output = Clean(output);
            var fEnum = includeFiles.GetEnumerator();
            string includes = "";
            while (fEnum.MoveNext())
            {
                includes += "\r\n\r\n" + fEnum.Current.Value;
            }
            output += minify(Clean(includes));
            Clipboard.SetText(output);
        }
        private string GetFileCode(FileInfo codeFile)
        {
            var sr = codeFile.OpenText();
            string text = sr.ReadToEnd();
            sr.Close();
            text = text.Remove(0, text.IndexOf("{", text.IndexOf("namespace")) + 1);
            text = text.Remove(text.LastIndexOf("}") - 1);

            return text;
        }
        private void GetRequires(string text, Dictionary<string, string> includeFiles)
        {
            int lastIndex = 0;
            int idx = text.IndexOf("using ", lastIndex);
            while (idx != -1)
            {

                string fileName = text.Substring(idx + ("using ").Length, text.IndexOf(";", idx) - idx - ("using ").Length) + ".cs";
                if (!fileName.Contains("\r\n") && !includeFiles.ContainsKey(fileName))
                {
                    var fList = Directory.EnumerateFiles(textBox1.Text, "*", SearchOption.AllDirectories).ToList();
                    bool found = false;
                    foreach (string fName in fList)
                    {
                        if (!fName.Contains("\\" + fileName))
                            continue;
                        found = true;
                        string code = GetFileCode(new FileInfo(fName));
                        includeFiles.Add(fileName, code);
                        GetRequires(code, includeFiles);
                    }
                    if (!found) throw new Exception("Unable to load file:" + fileName);

                }
                lastIndex = idx + "using ".Length;
                idx = text.IndexOf("using ", lastIndex);

            }

        }
        private string Clean(string text)
        {
            string cleanText = text;
            while (text.IndexOf("using ") != -1)
            {
                int idx = text.IndexOf("using ");
                text = text.Remove(idx, text.IndexOf(";", idx) + 1 - idx);

            }
            while (text.IndexOf("///") != -1)
            {
                int idx = text.IndexOf("///");
                text = text.Remove(idx, text.IndexOf("\r\n", idx) - idx);
            }
            text = text.Replace("\r\n    ", "\r\n");
            while (text.Contains("\r\n    \r\n"))
            {
                text = text.Replace("\r\n    \r\n", "\r\n");
            }
            while (text.Contains("\r\n\r\n"))
            {
                text = text.Replace("\r\n\r\n", "\r\n");
            }
            cleanText = text;

            return cleanText;
        }
        string minify(string input)
        {
            if (checkBox1.Checked)
            {
                while (input.IndexOf("//") != -1)
                {
                    int idx = input.IndexOf("//");
                    input = input.Remove(idx, input.IndexOf("\r\n", idx) - idx);
                }
                while (input.IndexOf("/*") != -1)
                {
                    int idx = input.IndexOf("/*");
                    input = input.Remove(idx, input.IndexOf("*/", idx)+2 - idx);
                }
                
                input = input.Replace("\r\n", " ");
                input = input.Replace("\r", " ");
                input = input.Replace("\n", " ");
                while (input.Contains("  "))
                {
                    input = input.Replace("  ", " ");
                }
                for (int i = 0; i < input.Length; i++)
                {
                    string spaceNeighbours = "()=;<>{}+,.?!:[]";
                    if (input.ToCharArray()[i] == ' ' &&
                        ((i > 0 && spaceNeighbours.Contains(input.ToCharArray()[i - 1])) ||
                        (i < input.Length - 1 && spaceNeighbours.Contains(input.ToCharArray()[i + 1]))))
                        input = input.Remove(i, 1);
                }
                input = addLines(input);
            }
            return input;
        }
        string addLines(string input)
        {
            string output = "";
            int charsSinceLastNewLine = 0;
            for (int idx = 0; idx < input.Length; idx++)
            {
                if (charsSinceLastNewLine > 80)
                {
                    string workingText = input.Remove(0, (output.Replace("\r\n", "").Length));
                    int workingIdx = idx - (output.Replace("\r\n", "").Length);
                    if (" {};().,".Contains(workingText.Substring(workingIdx, 1)) &&
                        ((countNonEscapedQuotes(workingText.Substring(0, workingIdx)) % 2) == 0) &&
                        ((countNonEscapedQuotes(workingText.Substring(0, workingIdx), '\'') % 2) == 0))
                    {
                        output += (workingText.Substring(0, workingIdx)) + "\r\n";
                        charsSinceLastNewLine = -1;
                    }
                }
                charsSinceLastNewLine++;
            }
            output += input.Replace(output.Replace("\r\n", ""), "");
            output = output.Replace("\r\n ", "\r\n");
            return output.Trim();
        }
        int countNonEscapedQuotes(string searchString, char quote = '"')
        {
            int numberOfMatches = 0;
            for (int i = 0; i < searchString.Length; i++)
                if (searchString.ToCharArray()[i] == quote && i > 0 && searchString.ToCharArray()[i - 1] != '\\' && (countNonEscapedQuotes(searchString.Substring(0, i), quote=='"'?'\'':'"') % 2) == 0)
                    numberOfMatches++;
            return numberOfMatches;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Tag = listBox1.SelectedIndex;
        }
    }
}
