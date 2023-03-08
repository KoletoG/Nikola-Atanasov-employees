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
            table.Columns.Clear();
            table.Rows.Clear();
            var loadDialog = new OpenFileDialog { Filter = "Text File|*.txt" };
            loadDialog.ShowDialog();
            string[] lines = File.ReadAllLines($@"{loadDialog.FileName}");
            string[] data;
            table.Columns.Add("Employee ID #1", typeof(string));
            table.Columns.Add("Employee ID #2", typeof(string));
            table.Columns.Add("Project ID", typeof(string));
            table.Columns.Add("Days Worked", typeof(int));
            Dictionary<int, int> projId = new Dictionary<int, int>();
            Dictionary<int, DateTimeOffset> dateStarted = new Dictionary<int, DateTimeOffset>();
            Dictionary<int, DateTimeOffset> dateEnded = new Dictionary<int, DateTimeOffset>();
            // Declaring more formats that can be DateTime / DateTimeOffset parsed
            string[] formats = { "MM/dd/yyyy", "yyyy-MM-dd","d/MM/yyyy",
                "MMMM dd","dd.MM.yyyy", "dd/MM/yyyy","dd-MM-yyyy",
                "dddd, dd MMMM yyyy", "yyyy MMMM","M/d/yyyy h:mm:ss tt",
                "M/d/yyyy h:mm tt", "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                     "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                     "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                     "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm" };
            // Calculates time worked between dates + neglects overlapping dates 
            for (int i = 0; i < lines.Length; i++)
            {
                data = lines[i].ToString().Split(',');
                DateTimeOffset dateStart = new DateTimeOffset();

                DateTimeOffset dateEnd = new DateTimeOffset();
                int projectId = int.Parse(data[1].Trim());
                dateStart = DateTimeOffset.ParseExact(data[2].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);
                if (data[3].Trim() != "NULL")
                {
                    dateEnd = DateTimeOffset.ParseExact(data[3].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);
                }
                else
                {
                    dateEnd = DateTimeOffset.UtcNow;
                }
                TimeSpan timeSpan;
                int days = 0;
                if (!projId.ContainsKey(int.Parse(data[1].Trim())))
                {
                    projId.Add(projectId, days);
                }
                if (!dateStarted.ContainsKey(int.Parse(data[1].Trim())))
                {
                    dateStarted.Add(projectId, dateStart);
                    dateEnded.Add(projectId, dateEnd);
                }
                else if (dateStarted[projectId] > dateStart && dateEnd >= dateEnded[projectId])
                {
                    dateStarted[projectId] = dateStart;
                    dateEnded[projectId] = dateEnd;
                }
                else if (dateStarted[projectId] > dateStart && dateEnd >= dateStarted[projectId])
                {
                    dateStarted[projectId] = dateStart;
                }
                else if (dateStarted[projectId] <= dateStart && dateStart <= dateEnded[projectId])
                {
                    dateEnded[projectId] = dateEnd;
                }
                else if (dateEnd < dateStarted[projectId])
                {
                    timeSpan = dateStarted[projectId] - dateEnd;
                    days = timeSpan.Days;
                    projId[projectId] -= days;
                    dateStarted[projectId] = dateStart;
                }
                else if (dateStart > dateEnded[projectId])
                {
                    timeSpan = dateStart - dateEnded[projectId];
                    days = timeSpan.Days;
                    projId[projectId] -= days;
                }
            }
            foreach (KeyValuePair<int, DateTimeOffset> kvp in dateStarted)
            {
                foreach (KeyValuePair<int, DateTimeOffset> kvp1 in dateEnded)
                {
                    if (kvp.Key == kvp1.Key)
                    {
                        TimeSpan timeSpan;
                        int days = 0;
                        timeSpan = dateEnded[kvp1.Key] - dateStarted[kvp.Key];
                        days = timeSpan.Days;
                        projId[kvp.Key] += days;
                    }
                }
            }
            int max = 0;
            int keyMax = 0;
            // Checks which project has the most worked on time
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
            // Adds a row with the employees and the most worked on project
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
            string row1 = row[0];
            string row2 = row[1];
           
            table.Rows.Add(row);
            string[] data1;
            List<int> passedIDs = new List<int>
            {
                keyMax
            };
            // Adds every other project that is worked on by the previous pair of employees
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

            // Below is a solution if all employee pairs should appear in the datagrid who worked on common projects
            /*
            for (int i = 0; i < lines.Length; i++)
            {
                data = lines[i].ToString().Split(',');
                    bool passed = false;
                    row[0] = data[0];
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
                            if (data1[0] != data[0] && data[1] == data1[1])
                            {
                                row[1] = data1[0];
                                row[2] = data[1].Trim();
                                row[3] = projId[int.Parse(data[1].Trim())].ToString();
                                passedIDs.Add(int.Parse(data[1].Trim()));
                                table.Rows.Add(row);
                            }
                        }
                    }
                }
            */

            // The code below is a probably unsuccessful try at making the sample output (i really didn't understand what was that '8')
           
            /*
            DateTimeOffset startDateSol3 = new DateTimeOffset();
            DateTimeOffset endDateSol3 = new DateTimeOffset();
            DateTimeOffset startDate1Sol3 = new DateTimeOffset();
            DateTimeOffset endDate1Sol3 = new DateTimeOffset();
            bool secondAttempt = false;
            for(int i = 0; i < lines.Length; i++)
            {
                data = lines[i].ToString().Split(',');
                if (int.Parse(data[1].Trim()) == keyMax && secondAttempt==false)
                {
                    startDateSol3 = DateTimeOffset.ParseExact(data[2].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);
                    if (data[3].Trim()!="NULL")
                    {
                        endDateSol3 = DateTimeOffset.ParseExact(data[3].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);

                    }
                    else
                    {
                        endDateSol3 = DateTimeOffset.UtcNow;
                    }
                    secondAttempt = true;
                }
                else if(int.Parse(data[1].Trim()) == keyMax && secondAttempt == true)
                {
                    startDate1Sol3 = DateTimeOffset.ParseExact(data[2].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);
                    if (data[3].Trim() != "NULL")
                    {
                        endDate1Sol3 = DateTimeOffset.ParseExact(data[3].Trim(), formats, new CultureInfo("en-GB"), DateTimeStyles.None);

                    }
                    else
                    {
                        endDate1Sol3 = DateTimeOffset.UtcNow;
                    }
                    break;
                }
            }
            TimeSpan t1 = endDateSol3 - startDateSol3;
            TimeSpan t2 = endDate1Sol3 - startDate1Sol3;
            int years=0;
            if (t1.CompareTo(t2) < 0)
            {
                years = (t2.Days - t1.Days) / 30 / 12;
            }
            else if(t1.CompareTo(t2)>0)
            {
                years = (t1.Days - t2.Days) / 30 / 12;
            }
            else
            {
                years = t1.Days/30/12;
            }
            label1.Text = row1 + ", " + row2 + ", " + years;  
            */
            dataGridView1.DataSource = table;
            table.Columns.Cast<int>();
            dataGridView1.Sort(dataGridView1.Columns[3], ListSortDirection.Descending);
        }
    }
}
