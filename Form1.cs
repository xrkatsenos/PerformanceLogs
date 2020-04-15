using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace PerformanceMeter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Dictionary<string, int> endPointsTime = new Dictionary<string, int>();

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            lblEndpointsNumber.Text = "";

            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open csv File";
            theDialog.Filter = "csv files|*.csv";
            //theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {

                String fileName = theDialog.FileName.ToString();
                lblFileName.Text = fileName;

                try
                {
                    ReadCSV csv = new ReadCSV(fileName);

                    try
                    {
                        dataGridView1.DataSource = csv.readCSV;

                        //Count rows and columns
                        lblRows.Text = dataGridView1.Rows.Count.ToString();
                        lblColumns.Text = dataGridView1.ColumnCount.ToString();

                        //Find message column
                        lblMessageColumn.Text = FindMessageColumn(dataGridView1);
                        if (lblMessageColumn.Text != "Not found!")
                        {
                            btnAnalyze.Visible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private String FindMessageColumn(DataGridView datagridView)
        {
            int i = 0;
            foreach (DataGridViewColumn column in datagridView.Columns)
            {
                if (column.Name == "message")
                {
                    return i.ToString();
                }
                i++;
            }
            return "Not found!";
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            this.Update();
            ClearMessage(dataGridView1);
        }


        private void ClearMessage(DataGridView datagridView)
        {
            String tmpMessage = "";
            Int16 columnNumber = Int16.Parse(lblMessageColumn.Text);

            String action = "";
            String time = "";
            Int16 timeInt = 0;

            foreach (DataGridViewRow row in datagridView.Rows)
            {
                if (row.Cells[columnNumber].Value != null)
                {
                    tmpMessage = row.Cells[columnNumber].Value.ToString();
                    tmpMessage = tmpMessage.Replace("##PERFORMANCE##", "");

                    try
                    {
                        ///FIND VALUES
                        JObject json = JObject.Parse(tmpMessage);

                        action = (string)json["action"];
                        time = (string)json["time"];
                        timeInt = Int16.Parse(time);

                        if (CheckIfExist(action))
                        {
                            UpdateEndpointTime(action, timeInt);
                        }
                        else
                        {
                            AddEndpointTime(action, timeInt);
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                        //throw;
                    }

                }
            }

            CreateReport();

        }

        private Boolean CheckIfExist(string action)
        {
            if (endPointsTime.ContainsKey(action))
            {
                return true;
            }
            return false;
        }

        private void UpdateEndpointTime (string action, int actiontime)
        {
            Int16 newTime;

            //Get time value from dictionary
            var currentTime = endPointsTime[action];

            if (radioAverage.Checked)
            {
                //Calculate average value
                newTime = (Int16)((currentTime + actiontime) / 2);
            }
            else
            {
                if (actiontime > currentTime) { 
                    //Replace value with bigger
                    newTime = (Int16)actiontime;
                } else {
                    newTime = (Int16)currentTime;
                }
            }

            //Update value in dictionary
            endPointsTime[action] = newTime;

        }

       
        private void AddEndpointTime(string action, int time)
        {
            endPointsTime.Add(action, time);
        }


        private void CreateReport()
        {
            lblEndpointsNumber.Text = endPointsTime.Count.ToString();

            //Sort Dictionary
            var items = from pair in endPointsTime
                        orderby pair.Value descending
                        select pair;

            //Show results
            textBox1.Lines = items.Select(x => x.Key + " : " + x.Value).ToArray();


        }


    }
}
