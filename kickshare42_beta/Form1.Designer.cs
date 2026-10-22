namespace _42
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnReviews = new System.Windows.Forms.Button();
            this.btnTasks = new System.Windows.Forms.Button();
            this.btnRent = new System.Windows.Forms.Button();
            this.btnParking = new System.Windows.Forms.Button();
            this.btnUsers = new System.Windows.Forms.Button();
            this.btnScooters = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 50);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(987, 511);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnAdd);
            this.panel1.Controls.Add(this.btnReviews);
            this.panel1.Controls.Add(this.btnTasks);
            this.panel1.Controls.Add(this.btnRent);
            this.panel1.Controls.Add(this.btnParking);
            this.panel1.Controls.Add(this.btnUsers);
            this.panel1.Controls.Add(this.btnScooters);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(987, 50);
            this.panel1.TabIndex = 1;
            // 
            // btnReviews
            // 
            this.btnReviews.Location = new System.Drawing.Point(432, 12);
            this.btnReviews.Name = "btnReviews";
            this.btnReviews.Size = new System.Drawing.Size(75, 23);
            this.btnReviews.TabIndex = 5;
            this.btnReviews.Text = "Отзывы";
            this.btnReviews.UseVisualStyleBackColor = true;
            this.btnReviews.Click += new System.EventHandler(this.btnReviews_Click);
            // 
            // btnTasks
            // 
            this.btnTasks.Location = new System.Drawing.Point(351, 12);
            this.btnTasks.Name = "btnTasks";
            this.btnTasks.Size = new System.Drawing.Size(75, 23);
            this.btnTasks.TabIndex = 4;
            this.btnTasks.Text = "Задачи";
            this.btnTasks.UseVisualStyleBackColor = true;
            this.btnTasks.Click += new System.EventHandler(this.btnTasks_Click);
            // 
            // btnRent
            // 
            this.btnRent.Location = new System.Drawing.Point(270, 12);
            this.btnRent.Name = "btnRent";
            this.btnRent.Size = new System.Drawing.Size(75, 23);
            this.btnRent.TabIndex = 3;
            this.btnRent.Text = "Поездки";
            this.btnRent.UseVisualStyleBackColor = true;
            this.btnRent.Click += new System.EventHandler(this.btnRent_Click);
            // 
            // btnParking
            // 
            this.btnParking.Location = new System.Drawing.Point(189, 12);
            this.btnParking.Name = "btnParking";
            this.btnParking.Size = new System.Drawing.Size(75, 23);
            this.btnParking.TabIndex = 2;
            this.btnParking.Text = "Парковки";
            this.btnParking.UseVisualStyleBackColor = true;
            this.btnParking.Click += new System.EventHandler(this.btnParking_Click);
            // 
            // btnUsers
            // 
            this.btnUsers.Location = new System.Drawing.Point(93, 12);
            this.btnUsers.Name = "btnUsers";
            this.btnUsers.Size = new System.Drawing.Size(90, 23);
            this.btnUsers.TabIndex = 1;
            this.btnUsers.Text = "Пользователи";
            this.btnUsers.UseVisualStyleBackColor = true;
            this.btnUsers.Click += new System.EventHandler(this.btnUsers_Click);
            // 
            // btnScooters
            // 
            this.btnScooters.Location = new System.Drawing.Point(12, 12);
            this.btnScooters.Name = "btnScooters";
            this.btnScooters.Size = new System.Drawing.Size(75, 23);
            this.btnScooters.TabIndex = 0;
            this.btnScooters.Text = "Самокаты";
            this.btnScooters.UseVisualStyleBackColor = true;
            this.btnScooters.Click += new System.EventHandler(this.btnScooters_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(738, 12);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "+ Добавить";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(819, 12);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "- Удалить";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(900, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 561);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "42";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnUsers;
        private System.Windows.Forms.Button btnScooters;
        private System.Windows.Forms.Button btnReviews;
        private System.Windows.Forms.Button btnTasks;
        private System.Windows.Forms.Button btnRent;
        private System.Windows.Forms.Button btnParking;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
    }
}
