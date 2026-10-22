using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace _42
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=42.accdb;";
        private OleDbDataAdapter adapter;
        private OleDbCommandBuilder builder;
        private DataTable dt;
        private string currentTable = "Scooter";

        public Form1()
        {
            InitializeComponent();
            LoadTable("Scooter");
        }

        private void LoadTable(string tableName)
        {
            try
            {
                currentTable = tableName;
                adapter = new OleDbDataAdapter($"SELECT * FROM [{tableName}]", connectionString);
                builder = new OleDbCommandBuilder(adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";

                dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (dt != null)
            {
                using (AddForm addForm = new AddForm(dt))
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            adapter.Update(dt);
                            MessageBox.Show("Запись успешно добавлена!");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка сохранения в базу: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        DataRowView drv = (DataRowView)row.DataBoundItem;
                        drv.Row.Delete();
                    }
                }
            }
            else if (dataGridView1.CurrentRow != null && !dataGridView1.CurrentRow.IsNewRow)
            {
                DataRowView drv = (DataRowView)dataGridView1.CurrentRow.DataBoundItem;
                drv.Row.Delete();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.EndEdit();
                if (adapter != null && dt != null)
                {
                    adapter.Update(dt);
                    MessageBox.Show("Изменения сохранены!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void btnScooters_Click(object sender, EventArgs e)
        {
            LoadTable("Scooter");
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            LoadTable("Users");
        }

        private void btnParking_Click(object sender, EventArgs e)
        {
            LoadTable("Parking");
        }

        private void btnRent_Click(object sender, EventArgs e)
        {
            LoadTable("Rent");
        }

        private void btnTasks_Click(object sender, EventArgs e)
        {
            LoadTable("Task");
        }

        private void btnReviews_Click(object sender, EventArgs e)
        {
            LoadTable("Review");
        }
    }
}
