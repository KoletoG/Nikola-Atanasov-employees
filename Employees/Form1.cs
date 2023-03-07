using System;
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
            var loadDialog = new OpenFileDialog { Filter = "Text File|*.txt" };
            loadDialog.ShowDialog();
            string[] lines = File.ReadAllLines($@"{loadDialog.FileName}");
            string[] data;

            table.Columns.Add("Employee ID #1", typeof(string));
            table.Columns.Add("Employee ID #2", typeof(string));
            table.Columns.Add("Project ID", typeof(string));
            table.Columns.Add("Days Worked", typeof(string));
            Dictionary<int,int> projId = new Dictionary<int, int>();
            for (int i = 0; i < lines.Length; i++)
            {
                data = lines[i].ToString().Split(',');
                DateTimeOffset dateStart = new DateTimeOffset();
                DateTimeOffset dateEnd = new DateTimeOffset();
                dateStart = DateTimeOffset.Parse(data[2]);
                if (data[3] != "NULL")
                {
                    dateEnd = DateTimeOffset.Parse(data[3]);
                }
                else
                {
                    dateEnd = DateTimeOffset.UtcNow;
                }
                TimeSpan timeSpan = dateEnd - dateStart;
                int days = timeSpan.Days;
               
                if (!projId.ContainsKey(int.Parse(data[1])))
                {
                    projId.Add(int.Parse(data[1]), days);
                }
                else
                {
                    projId[int.Parse(data[1])] += days;
                }
                string[] row = new string[data.Length];
                for (int j = 0; j < data.Length; j++)
                {
                    row[j] = data[j].Trim();
                }
                table.Rows.Add(row);
            }
            dataGridView1.DataSource = table;
        }
    }
}
