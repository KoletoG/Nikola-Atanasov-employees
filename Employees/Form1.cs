using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Employees
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        DataTable table = new DataTable();
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Trqbva datata da ne se vmestva v drugata, vsichki proekti na top pichagite zaedno
            var loadDialog = new OpenFileDialog { Filter = "Text File|*.txt" };
            loadDialog.ShowDialog();
            string[] lines = File.ReadAllLines($@"{loadDialog.FileName}");
            string[] data;

            table.Columns.Add("Employee ID #1", typeof(string));
            table.Columns.Add("Employee ID #2", typeof(string));
            table.Columns.Add("Project ID", typeof(string));
            table.Columns.Add("Days Worked", typeof(string));
            Dictionary<int, int> projId = new Dictionary<int, int>();
            string[] formats = { "MM/dd/yyyy", "yyyy-MM-dd", "MMMM dd", "dddd, dd MMMM yyyy", "yyyy MMMM" };
            for (int i = 0; i < lines.Length; i++)
            {
                data = lines[i].ToString().Split(',');
                DateTimeOffset dateStart = new DateTimeOffset();
                DateTimeOffset dateEnd = new DateTimeOffset();

                dateStart = DateTimeOffset.ParseExact(data[2].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);
                if (data[3].Trim() != "NULL")
                {
                    dateEnd = DateTimeOffset.ParseExact(data[3].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);
                }
                else
                {
                    dateEnd = DateTimeOffset.UtcNow;
                }
                TimeSpan timeSpan = dateEnd - dateStart;
                int days = timeSpan.Days;

                if (!projId.ContainsKey(int.Parse(data[1].Trim())))
                {
                    projId.Add(int.Parse(data[1].Trim()), days);
                }
                else
                {
                    projId[int.Parse(data[1].Trim())] += days;
                }
            }
            int max = 0;
            int keyMax = 0;
            foreach (KeyValuePair<int, int> kvp in projId)
            {
                if (projId[kvp.Key] > max)
                {
                    max = projId[kvp.Key];
                    keyMax = kvp.Key;
                }
            }
            bool firstPassed = false;
            string[] row = new string[4];
            for (int i = 0; i < lines.Length; i++)
            {
                data = lines[i].ToString().Split(',');
                if (data[1].Trim() == keyMax.ToString() && firstPassed == false)
                {
                    row[0] = data[0];
                    firstPassed = true;
                }
                else if (data[1].Trim() == keyMax.ToString())
                {
                    row[1] = data[0];
                    break;
                }
            }
            int empl1 = int.Parse(row[0]);
            int empl2 = int.Parse(row[1]);
            row[2] = keyMax.ToString();
            row[3] = max.ToString();
            table.Rows.Add(row);
            string[] data1;
            List<int> passedIDs = new List<int>();
            passedIDs.Add(keyMax);
            for (int i = 0; i < lines.Length; i++)
            {
                data = lines[i].ToString().Split(',');
                if (int.Parse(data[0].Trim()) == empl1)
                {
                    bool passed = false;
                    for (int k = 0; k < passedIDs.Count(); k++)
                    {
                        if (int.Parse(data[1].Trim()) == passedIDs[k])
                        {
                            passed = true;
                        }

                    }
                    if (passed == false)
                    {
                        for (int g = 0; g < lines.Length; g++)
                        {
                            data1 = lines[g].ToString().Split(',');
                            if (int.Parse(data1[0].Trim()) == empl2 && data[1] == data1[1])
                            {
                                row[0] = empl1.ToString();
                                row[1] = empl2.ToString();
                                row[2] = data[1].Trim();
                                row[3] = projId[int.Parse(data[1].Trim())].ToString();
                                passedIDs.Add(int.Parse(data[1].Trim()));
                                table.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            dataGridView1.DataSource = table;
        }
    }
}
