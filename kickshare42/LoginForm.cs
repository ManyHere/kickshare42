using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace _42v2
{
    public partial class LoginForm : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=42.accdb;";
        private TextBox txtPhone;
        private TextBox txtPassword;
        private Button btnLogin;

        public LoginForm()
        {
            InitializeComponent();
            EnsurePasswordColumnExists();
            BuildUi();
        }

        private void EnsurePasswordColumnExists()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, "Users", "password" });
                    if (schema == null || schema.Rows.Count == 0)
                    {
                        // Добавляем колонку password в таблицу Users
                        using (OleDbCommand alterCmd = new OleDbCommand("ALTER TABLE [Users] ADD COLUMN [password] TEXT(50)", conn))
                        {
                            alterCmd.ExecuteNonQuery();
                        }

                        // Устанавливаем индивидуальные пароли по умолчанию
                        using (OleDbCommand cmd = new OleDbCommand("UPDATE [Users] SET [password] = 'admin123' WHERE role = 'Admin'", conn))
                            cmd.ExecuteNonQuery();

                        using (OleDbCommand cmd = new OleDbCommand("UPDATE [Users] SET [password] = 'tech123' WHERE role = 'Technician'", conn))
                            cmd.ExecuteNonQuery();

                        using (OleDbCommand cmd = new OleDbCommand("UPDATE [Users] SET [password] = 'client123' WHERE role = 'Client'", conn))
                            cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private void BuildUi()
        {
            this.Text = "Вход в систему — Kicksharing";
            this.Size = new Size(360, 290);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitle = new Label()
            {
                Text = "Авторизация",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Location = new Point(25, 18),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            Label lblPhone = new Label()
            {
                Text = "Номер телефона:",
                Location = new Point(25, 55),
                Size = new Size(290, 18),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(lblPhone);

            txtPhone = new TextBox()
            {
                Location = new Point(25, 76),
                Size = new Size(290, 26),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(txtPhone);

            Label lblPass = new Label()
            {
                Text = "Пароль:",
                Location = new Point(25, 114),
                Size = new Size(290, 18),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(lblPass);

            txtPassword = new TextBox()
            {
                Location = new Point(25, 135),
                Size = new Size(290, 26),
                PasswordChar = '●',
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(txtPassword);

            btnLogin = new Button()
            {
                Text = "Войти",
                Location = new Point(25, 185),
                Size = new Size(290, 40),
                BackColor = Color.FromArgb(43, 47, 58),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string phone = txtPhone.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Введите номер телефона и пароль!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, name, role, phone, [password] FROM [Users] WHERE phone = ?";
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("?", phone);
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string role = reader["role"].ToString();
                                string dbPass = reader["password"] != DBNull.Value ? reader["password"].ToString() : "";

                                // Если в базе поле пароля было пустое, берем дефолтный по роли
                                if (string.IsNullOrEmpty(dbPass))
                                {
                                    dbPass = (role == "Admin") ? "admin123" :
                                             (role == "Technician") ? "tech123" : "client123";
                                }

                                if (pass == dbPass)
                                {
                                    CurrentUser.Id = Convert.ToInt32(reader["id"]);
                                    CurrentUser.Name = reader["name"].ToString();
                                    CurrentUser.Role = role;
                                    CurrentUser.Phone = reader["phone"].ToString();

                                    this.DialogResult = DialogResult.OK;
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Неверный пароль!", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пользователь с таким номером не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка авторизации: " + ex.Message);
            }
        }
    }
}
