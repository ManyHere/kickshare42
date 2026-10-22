using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace _42
{
    public partial class AddForm : Form
    {
        private DataTable dataTable;
        private Dictionary<string, Control> inputControls = new Dictionary<string, Control>();

        private Dictionary<string, string> fieldLabels = new Dictionary<string, string>()
        {
            { "name", "ФИО пользователя" },
            { "phone", "Номер телефона" },
            { "role", "Роль (Client / Technician / Admin)" },
            { "cardToken", "Токен карты" },
            { "coordinates", "Координаты (Широта, Долгота)" },
            { "capacity", "Вместимость парковки" },
            { "hasLock", "Наличие замка" },
            { "isHighTraffic", "Высокая проходимость" },
            { "qrCode", "QR-код самоката" },
            { "locationLat", "Координаты нахождения" },
            { "batteryLevel", "Заряд батареи (%)" },
            { "status", "Статус" },
            { "errorCode", "Код ошибки (0 если исправен)" },
            { "parking_id", "ID парковки" },
            { "user_id", "ID пользователя" },
            { "scooter_id", "ID самоката" },
            { "startTime", "Время начала" },
            { "endTime", "Время окончания" },
            { "totalCost", "Стоимость поездки" },
            { "parkingPhoto", "Фото парковки" },
            { "taskType", "Тип задачи (SwapBattery / Repair / Relocation)" },
            { "photoProofUrl", "Фото отчета" },
            { "checklistPassed", "Чек-лист пройден" },
            { "createdAt", "Дата и время создания" },
            { "rent_id", "ID поездки" },
            { "rating", "Оценка (1 - 5)" },
            { "comment", "Текст отзыва" }
        };

        public AddForm(DataTable dt)
        {
            InitializeComponent();
            this.dataTable = dt;
            BuildDynamicForm();
        }

        private void BuildDynamicForm()
        {
            this.Text = "Добавление новой записи";
            this.Width = 460;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            Panel scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.AutoScroll = true;
            this.Controls.Add(scrollPanel);

            int currentTop = 15;

            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.ColumnName.ToLower() == "id")
                {
                    continue;
                }

                Label lbl = new Label();
                string labelText = column.ColumnName;
                if (fieldLabels.ContainsKey(column.ColumnName))
                {
                    labelText = fieldLabels[column.ColumnName];
                }

                lbl.Text = labelText + ":";
                lbl.Location = new Point(15, currentTop);
                lbl.Size = new Size(400, 18);
                scrollPanel.Controls.Add(lbl);

                currentTop += 22;

                if (column.DataType == typeof(bool))
                {
                    CheckBox chk = new CheckBox();
                    chk.Text = "Да";
                    chk.Location = new Point(15, currentTop);
                    chk.Size = new Size(100, 24);
                    scrollPanel.Controls.Add(chk);
                    inputControls.Add(column.ColumnName, chk);
                    currentTop += 30;
                }
                else
                {
                    TextBox txt = new TextBox();
                    txt.Location = new Point(15, currentTop);
                    txt.Size = new Size(390, 22);

                    if (column.DataType == typeof(DateTime))
                    {
                        txt.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    }

                    scrollPanel.Controls.Add(txt);
                    inputControls.Add(column.ColumnName, txt);
                    currentTop += 32;
                }
            }

            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 55;
            this.Controls.Add(bottomPanel);

            Button btnSave = new Button();
            btnSave.Text = "Добавить";
            btnSave.Size = new Size(110, 30);
            btnSave.Location = new Point(110, 12);
            btnSave.Click += BtnSave_Click;
            bottomPanel.Controls.Add(btnSave);

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Size = new Size(110, 30);
            btnCancel.Location = new Point(235, 12);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            bottomPanel.Controls.Add(btnCancel);

            int windowHeight = currentTop + 110;
            if (windowHeight > 620)
            {
                windowHeight = 620;
            }
            this.Height = windowHeight;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow newRow = dataTable.NewRow();

                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.ColumnName.ToLower() == "id")
                    {
                        continue;
                    }

                    if (inputControls.ContainsKey(column.ColumnName))
                    {
                        Control ctrl = inputControls[column.ColumnName];

                        if (ctrl is CheckBox chk)
                        {
                            newRow[column.ColumnName] = chk.Checked;
                        }
                        else if (ctrl is TextBox txt)
                        {
                            string val = txt.Text.Trim();
                            if (string.IsNullOrEmpty(val))
                            {
                                newRow[column.ColumnName] = DBNull.Value;
                            }
                            else
                            {
                                newRow[column.ColumnName] = Convert.ChangeType(val, column.DataType);
                            }
                        }
                    }
                }

                dataTable.Rows.Add(newRow);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка заполнения данных: " + ex.Message);
            }
        }
    }
}
