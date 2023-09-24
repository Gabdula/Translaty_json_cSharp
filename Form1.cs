using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.ComTypes;
using System.Data.SqlTypes;
using System.Dynamic;
using static System.Net.WebRequestMethods;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class ConfModel1
        {

            [JsonProperty("properties:10")]
            public ConfModel2 properties { get; set; }

        }
        public class ConfModel2
        {
            [JsonProperty("betterquesting:10")]
            public ConfModel3 betterquesting { get; set; }

        }

        public class ConfModel3
        {
            [JsonProperty("name:8")]
            public string name { get; set; }
            [JsonProperty("desc:8")]
            public string desc { get; set; }
        }


        static HttpClient client = new HttpClient();

        private async void Form1_Load(object sender, EventArgs e)
        {

        }


        public List<string> fileUrl = new List<string>();
        private void button1_Click(object sender, EventArgs e)
        {

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox1.Text = fbd.SelectedPath;
                    string[] catalog = Directory.GetDirectories(fbd.SelectedPath);
                    int catalogLength = catalog.Length;
                    
                    for (int i = 0; i < catalogLength; i++)
                    {
                        string[] files = Directory.GetFiles(catalog[i]);
                        for (int j = 0; j < files.Length; j++)
                        {
                            fileUrl.Add(files[j]);
                        }    
                    }
                }
            }
        }

        private async void button2_Click(object sender, EventArgs Exception)
        {
            for (int i = 0; i < fileUrl.Count; i++)
            {
                progressBar1.PerformStep();
                int percent = (int)(((double)progressBar1.Value / (double)progressBar1.Maximum) * 100);
                progressBar1.Refresh();
                progressBar1.CreateGraphics().DrawString(percent.ToString() + "%",
                    new Font("Arial", (float)8.25, FontStyle.Regular),
                    Brushes.Black,
                    new PointF(progressBar1.Width / 2 - 10, progressBar1.Height / 2 - 7));
                try
                {
                    var json = JsonConvert.DeserializeObject<ConfModel1>(System.IO.File.ReadAllText(@fileUrl[i]));
                    #region get desc
                    string desc = "";
                    var response = await client.GetStreamAsync($"https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl=en&tl=ru&q={json.properties.betterquesting.desc.ToString()}");
                    StreamReader reader = new StreamReader(response);
                    string text = reader.ReadToEnd();
                    string[] substrings = Regex.Split(text, "\"([^\"]*)\"");

                    for (int k = 0; k < substrings.Length; k++)
                    {
                        if (Regex.IsMatch(substrings[k], @"\p{IsCyrillic}"))
                        {
                            desc += substrings[k];
                        }
                    }
                    #endregion

                    #region get name
                    string name = "";
                    var response1 = await client.GetStreamAsync($"https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl=en&tl=ru&q={json.properties.betterquesting.name.ToString()}");
                    StreamReader reader1 = new StreamReader(response1);
                    string text1 = reader1.ReadToEnd();
                    string[] substrings1 = Regex.Split(text1, "\"([^\"]*)\"");

                    for (int k = 0; k < substrings1.Length; k++)
                    {
                        if (Regex.IsMatch(substrings1[k], @"\p{IsCyrillic}"))
                        {
                            name += substrings1[k];
                        }
                    }
                    if (name == null || name == "")
                    {

                        name = json.properties.betterquesting.name.ToString();
                    }
                    #endregion

                    richTextBox1.AppendText(Environment.NewLine + name);

                    lineChanger(getStrForEdit(name, "name:8"), fileUrl[i], lineNumber("name:8"));
                    lineChanger(getStrForEdit(desc, "desc:8"), fileUrl[i], lineNumber("desc:8"));

                    string getStrForEdit(string str, string find)
                    {
                        
                        if (find == "desc:8")
                        {
                            return "\"desc:8\": \"" + str + "\"";
                        }
                        else
                        {
                            return "\"name:8\": \"" + str + "\",";

                        }
                    }

                    int lineNumber(string find)
                    {
                        int j = 1;
                        StreamReader f = new StreamReader(@fileUrl[i]);
                        while (!f.EndOfStream)
                        {
                            string s = f.ReadLine();
                            if (s.Contains("\""+find+ "\""))
                            {
                                break;
                            }
                            j++;
                        }
                        f.Close();
                        return j;
                    }

                    void lineChanger(string newText, string fileName, int line_to_edit)
                    {
                        string[] arrLine = System.IO.File.ReadAllLines(fileName);
                        arrLine[line_to_edit - 1] = newText;
                        System.IO.File.WriteAllLines(fileName, arrLine);
                    }

                    richTextBox1.AppendText(Environment.NewLine + fileUrl[i] + " | SUCCESS");
                }
                catch (Exception)
                {
                    richTextBox1.AppendText(Environment.NewLine + Exception);
                }
            
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
    }
}
