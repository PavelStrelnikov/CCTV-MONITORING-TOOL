using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace AppsDemo
{
    public partial class FormLogs : DockContent
    {
        public FormLogs()
        {
            InitializeComponent();
            this.richTextBoxDetails.BackColor = this.BackColor;

            m_dtLogs = new DataTable();
            m_dtLogs.Columns.Add("time");
            m_dtLogs.Columns.Add("level");
            m_dtLogs.Columns.Add("module");
            m_dtLogs.Columns.Add("message");
            m_dtLogs.Columns.Add("details");
            m_dtLogs.Columns.Add("color");

            this.dataGridViewLogs.DataSource = m_dtLogs;
            this.dataGridViewLogs.AutoGenerateColumns = false;
        }
        private DataTable m_dtLogs = null;

        public delegate void AddLogsHandler(string level, Color color, string module, string message, string details);

        public void AddLogs(string level, Color color, string module, string message, string details = "")
        {
            string strDatetime = DateTime.Now.ToString();
            if (this.InvokeRequired)
            {
                AddLogsHandler handler = new AddLogsHandler(this.AddLogs);
                this.BeginInvoke(handler, level, module, message, details);
            }
            else
            {
                if (this.m_dtLogs.Rows.Count > 1000)
                {
                    this.m_dtLogs.Rows.RemoveAt(0);
                }
                DataRow row = m_dtLogs.NewRow();
                row["time"] = strDatetime;
                row["level"] = level;
                row["module"] = module;
                row["message"] = message;
                row["details"] = details;
                row["color"] = color.ToArgb();
                this.dataGridViewLogs.ClearSelection();
                
                m_dtLogs.Rows.Add(row);
            }
        }

        private void dataGridViewLogs_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            for (int index = 0; index < dataGridViewLogs.Rows.Count; ++index)
            {
                Color color = Color.FromArgb(Convert.ToInt32(m_dtLogs.Rows[index]["color"]));
                dataGridViewLogs.Rows[index].DefaultCellStyle.BackColor = color;
            }
        }

        private void dataGridViewLogs_SelectionChanged(object sender, EventArgs e)
        {
            this.richTextBoxDetails.Clear();
            this.richTextBoxDetails.BackColor = this.BackColor;
            if (this.dataGridViewLogs.CurrentRow == null)
            {
                return;
            }
            int index = this.dataGridViewLogs.CurrentRow.Index;
            if (index >= 0)
            {
                this.richTextBoxDetails.BackColor = dataGridViewLogs.Rows[index].DefaultCellStyle.BackColor;
                this.richTextBoxDetails.Text = "Time:" + m_dtLogs.Rows[index]["time"].ToString() + "\n"
                    + "Level:" + m_dtLogs.Rows[index]["level"].ToString() + "\n"
                    + "Module:" + m_dtLogs.Rows[index]["module"].ToString() + "\n"
                    + "Message:" + m_dtLogs.Rows[index]["message"].ToString() + "\n"
                    + "Details:\n" + m_dtLogs.Rows[index]["details"].ToString();
            }
        }

        private void FormLogs_Load(object sender, EventArgs e)
        {
            this.richTextBoxDetails.AutoWordSelection = false;
            dataGridViewLogs.ClearSelection();
        }
    }
}
