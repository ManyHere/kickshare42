using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace _42v2
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=42.accdb;";
        private DataTable dtDb;
        private OleDbDataAdapter adapter;

        // Флаг выхода для смены пользователя
        public bool IsLoggedOut { get; private set; } = false;

        // Иконка самоката 123.ico
        private Image scooterImg = null;

        // Панели интерфейса
        private Panel mapPanel;
        private Panel pnlClientSheet;
        private Panel pnlRemoteControl;
        private TextBox txtSearchScooter;

        // Живой таймер поездки клиента
        private Timer rideTimer;
        private DateTime? tripStartTime = null;
        private int currentActiveRentId = 0;
        private int currentActiveScooterId = 0;

        // Текущий выбранный самокат
        private int selectedScooterId = 0;
        private string selectedScooterQr = "";
        private int selectedBattery = 0;
        private string selectedStatus = "";
        private string selectedCoords = "";

        // Виджеты клиентской шторки (Urent)
        private Label lblClientQr;
        private Label lblClientBat;
        private Label lblClientTime;
        private Label lblLiveTimer;
        private Button btnClientAction;
        private PictureBox pbClientScooter;
        private Panel pnlTariffs;

        // Виджеты пульта техника
        private Label lblGear;
        private Label lblTechQr;
        private Label lblTechBatVal;
        private Panel pnlBatteryBar;
        private Label lblTechGpsCoords;
        private Label lblLockStatus;
        private Button btnSwapBattery;
        private Button btnToggleLock;

        // Модель точки самоката на карте
        private class ScooterPoint
        {
            public int Id;
            public string Qr;
            public int Battery;
            public string Status;
            public Point Location;
        }
        private List<ScooterPoint> mapScooters = new List<ScooterPoint>();

        public Form1()
        {
            InitializeComponent();
            LoadScooterIcon();
            InitRideTimer();
            SetupApp();
        }

        private void LoadScooterIcon()
        {
            try
            {
                string iconPath = Path.Combine(Application.StartupPath, "123.ico");
                if (File.Exists(iconPath))
                {
                    using (Icon ico = new Icon(iconPath, 64, 64))
                    {
                        scooterImg = ico.ToBitmap();
                    }
                }
            }
            catch { }
        }

        private void InitRideTimer()
        {
            rideTimer = new Timer();
            rideTimer.Interval = 1000;
            rideTimer.Tick += RideTimer_Tick;
        }

        private void RideTimer_Tick(object sender, EventArgs e)
        {
            if (tripStartTime.HasValue)
            {
                TimeSpan elapsed = DateTime.Now - tripStartTime.Value;
                int minutes = Math.Max(1, (int)Math.Ceiling(elapsed.TotalMinutes));
                int cost = 50 + (int)(minutes * 9.99);

                lblLiveTimer.Text = $"⏱ Поездка: {elapsed:mm\\:ss}  •  К оплате: {cost} ₽";
            }
        }

        private void SetupApp()
        {
            this.Text = $"Кикшеринг — [{CurrentUser.Role}] {CurrentUser.Name}";
            this.Size = new Size(1220, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Clear();

            // Верхняя шапка
            Panel topBar = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White
            };
            this.Controls.Add(topBar);

            Label lblUser = new Label()
            {
                Text = $"● {CurrentUser.Name}  |  Роль: {CurrentUser.Role}  |  Тел: {CurrentUser.Phone}",
                ForeColor = Color.FromArgb(40, 44, 52),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(14, 12),
                AutoSize = true
            };
            topBar.Controls.Add(lblUser);

            // Кнопка Выйти справа сверху
            Button btnLogout = new Button()
            {
                Text = "🚪 Выйти",
                Size = new Size(95, 28),
                Location = new Point(topBar.Width - 110, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(250, 250, 252),
                ForeColor = Color.FromArgb(220, 53, 69),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(220, 53, 69);
            btnLogout.Click += BtnLogout_Click;
            topBar.Controls.Add(btnLogout);

            Panel mainBox = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 245, 248)
            };
            this.Controls.Add(mainBox);
            mainBox.BringToFront();

            if (CurrentUser.Role == "Client")
            {
                BuildClientInterface(mainBox);
            }
            else if (CurrentUser.Role == "Technician")
            {
                BuildTechnicianInterface(mainBox);
            }
            else // Admin
            {
                BuildAdminInterface(mainBox);
            }

            LoadScootersFromDb();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из учетной записи?", "Смена пользователя", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                rideTimer?.Stop();
                IsLoggedOut = true;
                this.Close();
            }
        }

        // =========================================================================
        // ИНТЕРФЕЙС КЛИЕНТА (URENT)
        // =========================================================================
        private void BuildClientInterface(Panel container)
        {
            mapPanel = Create2GisMapCanvas();
            mapPanel.Dock = DockStyle.Fill;
            container.Controls.Add(mapPanel);

            pnlClientSheet = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 240,
                BackColor = Color.FromArgb(20, 21, 24),
                Visible = false
            };
            container.Controls.Add(pnlClientSheet);
            pnlClientSheet.BringToFront();

            Panel handleBar = new Panel()
            {
                Size = new Size(40, 4),
                BackColor = Color.FromArgb(70, 72, 80),
                Location = new Point((pnlClientSheet.Width / 2) - 20, 6),
                Anchor = AnchorStyles.Top
            };
            pnlClientSheet.Controls.Add(handleBar);

            lblClientQr = new Label()
            {
                Text = "SC-1001",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 14),
                AutoSize = true
            };
            pnlClientSheet.Controls.Add(lblClientQr);

            lblClientBat = new Label()
            {
                Text = "85%",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 200, 83),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(54, 24),
                Location = new Point(20, 52)
            };
            pnlClientSheet.Controls.Add(lblClientBat);

            lblClientTime = new Label()
            {
                Text = "~3 ч хода",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(170, 175, 185),
                Location = new Point(82, 55),
                AutoSize = true
            };
            pnlClientSheet.Controls.Add(lblClientTime);

            lblLiveTimer = new Label()
            {
                Text = "",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(254, 224, 0),
                Location = new Point(200, 54),
                AutoSize = true,
                Visible = false
            };
            pnlClientSheet.Controls.Add(lblLiveTimer);

            pbClientScooter = new PictureBox()
            {
                Location = new Point(480, 15),
                Size = new Size(130, 95),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = scooterImg,
                BackColor = Color.Transparent
            };
            pnlClientSheet.Controls.Add(pbClientScooter);

            pnlTariffs = new Panel() { Location = new Point(20, 84), Size = new Size(500, 64) };
            pnlClientSheet.Controls.Add(pnlTariffs);

            Panel card1 = new Panel() { Location = new Point(0, 0), Size = new Size(130, 60), BackColor = Color.FromArgb(34, 36, 42) };
            card1.Controls.Add(new Label() { Text = "По минутам\nКуда угодно", ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 7.5F), Location = new Point(6, 4), AutoSize = true });
            card1.Controls.Add(new Label() { Text = "9,99 ₽/мин", ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(6, 36), AutoSize = true });
            pnlTariffs.Controls.Add(card1);

            Panel card2 = new Panel() { Location = new Point(140, 0), Size = new Size(130, 60), BackColor = Color.FromArgb(28, 30, 34) };
            card2.Controls.Add(new Label() { Text = "30 мин\nна 1 день", ForeColor = Color.Gray, Font = new Font("Segoe UI", 7.5F), Location = new Point(6, 4), AutoSize = true });
            card2.Controls.Add(new Label() { Text = "179 ₽", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(6, 36), AutoSize = true });
            pnlTariffs.Controls.Add(card2);

            Button btnPayMethod = new Button()
            {
                Text = "Я",
                Size = new Size(48, 48),
                Location = new Point(20, 160),
                BackColor = Color.FromArgb(235, 60, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnPayMethod.FlatAppearance.BorderSize = 0;
            pnlClientSheet.Controls.Add(btnPayMethod);

            btnClientAction = new Button()
            {
                Text = "Забронировать (50 ₽ + 9,99 ₽/мин)",
                Size = new Size(400, 48),
                Location = new Point(78, 160),
                BackColor = Color.FromArgb(254, 224, 0),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClientAction.FlatAppearance.BorderSize = 0;
            btnClientAction.Click += BtnClientAction_Click;
            pnlClientSheet.Controls.Add(btnClientAction);

            CheckActiveClientRent();
        }

        // =========================================================================
        // ИНТЕРФЕЙС ТЕХНИКА
        // =========================================================================
        private void BuildTechnicianInterface(Panel container)
        {
            Panel rightBox = new Panel() { Dock = DockStyle.Right, Width = 350, BackColor = Color.White };
            container.Controls.Add(rightBox);

            Label lblHead = new Label()
            {
                Text = "Задачи обслуживания",
                Dock = DockStyle.Top,
                Height = 36,
                ForeColor = Color.FromArgb(30, 30, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(10, 8, 0, 0)
            };
            rightBox.Controls.Add(lblHead);

            dataGridView1 = new DataGridView()
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            rightBox.Controls.Add(dataGridView1);
            dataGridView1.BringToFront();
            LoadTechDbTasks();

            pnlRemoteControl = CreateRemoteControlWidget();
            pnlRemoteControl.Dock = DockStyle.Right;
            container.Controls.Add(pnlRemoteControl);

            Panel leftBox = new Panel() { Dock = DockStyle.Fill };
            container.Controls.Add(leftBox);

            Panel searchBar = new Panel() { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };
            leftBox.Controls.Add(searchBar);

            Label lblS = new Label() { Text = "Поиск по номеру / QR:", Location = new Point(12, 13), AutoSize = true };
            txtSearchScooter = new TextBox() { Location = new Point(155, 10), Width = 130, Font = new Font("Segoe UI", 9.5F) };
            Button btnS = new Button()
            {
                Text = "Открыть пульт",
                Location = new Point(295, 9),
                Width = 120,
                Height = 26,
                BackColor = Color.FromArgb(45, 50, 63),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnS.Click += (s, e) => FindScooterManual(txtSearchScooter.Text.Trim());
            searchBar.Controls.AddRange(new Control[] { lblS, txtSearchScooter, btnS });

            mapPanel = Create2GisMapCanvas();
            mapPanel.Dock = DockStyle.Fill;
            leftBox.Controls.Add(mapPanel);
            mapPanel.BringToFront();
        }

        // =========================================================================
        // ИНТЕРФЕЙС АДМИНИСТРАТОРА
        // =========================================================================
        private void BuildAdminInterface(Panel container)
        {
            Panel rightBox = new Panel() { Dock = DockStyle.Right, Width = 560, BackColor = Color.White };
            container.Controls.Add(rightBox);

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel() { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(240, 242, 245) };
            rightBox.Controls.Add(pnlButtons);

            string[] tables = { "Scooter", "Users", "Parking", "Rent", "Task", "Review" };
            foreach (var t in tables)
            {
                Button b = new Button() { Text = t, Width = 75, Height = 28 };
                string tbl = t;
                b.Click += (s, e) => LoadAdminTable(tbl);
                pnlButtons.Controls.Add(b);
            }

            dataGridView1 = new DataGridView() { Dock = DockStyle.Fill, BackgroundColor = Color.White };
            rightBox.Controls.Add(dataGridView1);
            dataGridView1.BringToFront();
            LoadAdminTable("Scooter");

            mapPanel = Create2GisMapCanvas();
            mapPanel.Dock = DockStyle.Fill;
            container.Controls.Add(mapPanel);
        }

        // =========================================================================
        // ПУЛЬТ УПРАВЛЕНИЯ
        // =========================================================================
        private Panel CreateRemoteControlWidget()
        {
            Panel p = new Panel()
            {
                Width = 320,
                BackColor = Color.FromArgb(244, 245, 248),
                Padding = new Padding(12),
                AutoScroll = true
            };

            Label lblTitle = new Label()
            {
                Text = "← Пульт управления",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(12, 10),
                Size = new Size(290, 28)
            };
            p.Controls.Add(lblTitle);

            Panel cardModel = new Panel()
            {
                Location = new Point(12, 45),
                Size = new Size(140, 110),
                BackColor = Color.FromArgb(43, 47, 58)
            };

            lblGear = new Label()
            {
                Text = "⚙",
                ForeColor = Color.FromArgb(255, 179, 138),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                Location = new Point(8, 8),
                AutoSize = true,
                Visible = false
            };

            PictureBox pbTechScooter = new PictureBox()
            {
                Location = new Point(90, 8),
                Size = new Size(42, 42),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = scooterImg,
                BackColor = Color.Transparent
            };

            lblTechQr = new Label() { Text = "---", ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(10, 48), AutoSize = true };
            Label lblModel = new Label() { Text = "Ninebot SL 90", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 8F), Location = new Point(10, 78), AutoSize = true };
            cardModel.Controls.AddRange(new Control[] { lblGear, pbTechScooter, lblTechQr, lblModel });
            p.Controls.Add(cardModel);

            Panel cardBattery = new Panel() { Location = new Point(160, 45), Size = new Size(140, 110), BackColor = Color.White };
            cardBattery.Controls.Add(new Label() { Text = "⚡", ForeColor = Color.Gray, Font = new Font("Segoe UI", 11F), Location = new Point(8, 8), AutoSize = true });
            cardBattery.Controls.Add(new Label() { Text = "Батарея", ForeColor = Color.Black, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Location = new Point(8, 48), AutoSize = true });
            lblTechBatVal = new Label() { Text = "0%", ForeColor = Color.Black, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(95, 48), AutoSize = true };
            cardBattery.Controls.Add(lblTechBatVal);

            Panel barBg = new Panel() { Location = new Point(10, 75), Size = new Size(120, 14), BackColor = Color.FromArgb(235, 238, 242) };
            pnlBatteryBar = new Panel() { Location = new Point(0, 0), Size = new Size(30, 14), BackColor = Color.FromArgb(245, 183, 37) };
            barBg.Controls.Add(pnlBatteryBar);
            cardBattery.Controls.Add(barBg);
            p.Controls.Add(cardBattery);

            Panel cardNet = new Panel() { Location = new Point(12, 165), Size = new Size(140, 70), BackColor = Color.FromArgb(236, 237, 252) };
            cardNet.Controls.Add(new Label() { Text = "📶  В сети\n\n09:31:18 (online)", Font = new Font("Segoe UI", 8F), Location = new Point(8, 8), Size = new Size(124, 55) });
            p.Controls.Add(cardNet);

            Panel cardGps = new Panel() { Location = new Point(12, 245), Size = new Size(140, 70), BackColor = Color.FromArgb(236, 237, 252) };
            cardGps.Controls.Add(new Label() { Text = "📍  GPS\n\nСпутники: 16", Font = new Font("Segoe UI", 8F), Location = new Point(8, 8), Size = new Size(124, 55) });
            p.Controls.Add(cardGps);

            Panel cardLock = new Panel() { Location = new Point(160, 165), Size = new Size(140, 150), BackColor = Color.White };
            lblLockStatus = new Label() { Text = "🔒 Замок\n\nЗакрыт", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Location = new Point(10, 12), Size = new Size(120, 45) };
            btnToggleLock = new Button() { Text = "Открыть", Location = new Point(10, 95), Size = new Size(120, 32), BackColor = Color.FromArgb(235, 238, 242), FlatStyle = FlatStyle.Flat };
            btnToggleLock.Click += (s, e) =>
            {
                bool closed = lblLockStatus.Text.Contains("Закрыт");
                lblLockStatus.Text = closed ? "🔓 Замок\n\nОткрыт" : "🔒 Замок\n\nЗакрыт";
                btnToggleLock.Text = closed ? "Закрыть" : "Открыть";
            };
            cardLock.Controls.AddRange(new Control[] { lblLockStatus, btnToggleLock });
            p.Controls.Add(cardLock);

            Panel cardTracker = new Panel() { Location = new Point(12, 325), Size = new Size(288, 140), BackColor = Color.White };
            cardTracker.Controls.Add(new Label() { Text = "🎯 Трекер\ndriver.scooter.omni.api", Font = new Font("Segoe UI", 8F), Location = new Point(10, 10), Size = new Size(260, 30) });
            lblTechGpsCoords = new Label() { Text = "56.857177, 53.301067", ForeColor = Color.Indigo, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(10, 45), Size = new Size(260, 20) };
            cardTracker.Controls.Add(lblTechGpsCoords);

            btnSwapBattery = new Button()
            {
                Text = "Заменить АКБ",
                Location = new Point(10, 75),
                Size = new Size(268, 42),
                BackColor = Color.FromArgb(108, 92, 231),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSwapBattery.Click += (s, e) => ExecuteBatterySwap();
            cardTracker.Controls.Add(btnSwapBattery);
            p.Controls.Add(cardTracker);

            return p;
        }

        // =========================================================================
        // КАРТА 2GIS
        // =========================================================================
        private Panel Create2GisMapCanvas()
        {
            Panel p = new Panel() { BackColor = Color.FromArgb(244, 246, 249) };
            p.Paint += MapCanvas_Paint;
            p.MouseClick += MapCanvas_MouseClick;
            return p;
        }

        private void MapCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush grass = new SolidBrush(Color.FromArgb(232, 244, 232)))
            {
                g.FillRectangle(grass, 30, 210, 180, 120);
                g.FillRectangle(grass, 240, 210, 150, 150);
                g.FillRectangle(grass, 420, 220, 140, 160);
            }

            Rectangle[] buildings = {
                new Rectangle(30, 20, 150, 100),
                new Rectangle(210, 20, 130, 90),
                new Rectangle(370, 20, 160, 90),
                new Rectangle(250, 230, 110, 80),
                new Rectangle(70, 260, 90, 70),
                new Rectangle(260, 360, 110, 60),
                new Rectangle(30, 370, 120, 80),
                new Rectangle(430, 240, 120, 140)
            };

            using (SolidBrush bBrush = new SolidBrush(Color.FromArgb(236, 239, 244)))
            using (Pen bPen = new Pen(Color.FromArgb(216, 220, 228), 1.2F))
            {
                foreach (var b in buildings)
                {
                    g.FillRectangle(bBrush, b);
                    g.DrawRectangle(bPen, b);
                }
            }

            using (Pen pathPen = new Pen(Color.FromArgb(135, 195, 145), 1.5F))
            {
                pathPen.DashStyle = DashStyle.Dash;
                g.DrawLine(pathPen, 30, 145, 550, 145);
                g.DrawLine(pathPen, 180, 145, 180, 420);
                g.DrawLine(pathPen, 380, 145, 380, 420);
            }

            using (Pen roadBorder = new Pen(Color.FromArgb(205, 210, 218), 26))
            using (Pen roadCenter = new Pen(Color.White, 22))
            {
                Point[] kalash = { new Point(0, 175), new Point(340, 175), new Point(460, 195), new Point(mapPanel.Width, 205) };
                g.DrawLines(roadBorder, kalash);
                g.DrawLines(roadCenter, kalash);

                Point[] samson = { new Point(410, 195), new Point(410, mapPanel.Height) };
                g.DrawLines(roadBorder, samson);
                g.DrawLines(roadCenter, samson);
            }

            using (Font streetFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (Brush streetBrush = new SolidBrush(Color.FromArgb(100, 105, 115)))
            {
                g.DrawString("просп. Калашникова", streetFont, streetBrush, 50, 168);
                g.DrawString("ул. Самсонова", streetFont, streetBrush, 418, 280);
            }

            using (Font poiFont = new Font("Segoe UI", 8F))
            {
                g.FillEllipse(new SolidBrush(Color.FromArgb(215, 235, 255)), 280, 245, 18, 18);
                g.DrawString("🎓 ИТЭУП", poiFont, Brushes.RoyalBlue, 260, 268);

                g.FillEllipse(new SolidBrush(Color.FromArgb(220, 245, 225)), 100, 275, 18, 18);
                g.DrawString("⛽ Башнефть", poiFont, Brushes.ForestGreen, 78, 298);

                g.FillEllipse(new SolidBrush(Color.FromArgb(215, 235, 255)), 295, 375, 18, 18);
                g.DrawString("🌐 Почемучка", poiFont, Brushes.RoyalBlue, 270, 396);
            }

            Point[] parkings = {
                new Point(65, 235),
                new Point(200, 155),
                new Point(370, 175),
                new Point(230, 345),
                new Point(450, 390)
            };
            foreach (var p in parkings)
            {
                g.FillEllipse(Brushes.White, p.X - 14, p.Y - 14, 28, 28);
                g.DrawEllipse(new Pen(Color.FromArgb(200, 205, 215), 1.5F), p.X - 14, p.Y - 14, 28, 28);
                g.DrawString("P", new Font("Segoe UI", 10F, FontStyle.Bold), Brushes.Black, p.X - 7, p.Y - 8);
            }

            foreach (var sc in mapScooters)
            {
                bool isSelected = (sc.Id == selectedScooterId);

                if (isSelected)
                {
                    g.FillEllipse(new SolidBrush(Color.FromArgb(136, 84, 255)), sc.Location.X - 17, sc.Location.Y - 17, 34, 34);

                    g.FillEllipse(Brushes.White, sc.Location.X - 16, sc.Location.Y - 40, 32, 26);
                    Point[] tri = {
                        new Point(sc.Location.X - 6, sc.Location.Y - 16),
                        new Point(sc.Location.X + 6, sc.Location.Y - 16),
                        new Point(sc.Location.X, sc.Location.Y - 8)
                    };
                    g.FillPolygon(Brushes.White, tri);

                    if (scooterImg != null)
                        g.DrawImage(scooterImg, sc.Location.X - 10, sc.Location.Y - 37, 20, 20);
                }

                g.FillEllipse(Brushes.White, sc.Location.X - 13, sc.Location.Y - 13, 26, 26);
                g.DrawEllipse(new Pen(Color.FromArgb(40, 40, 45), 1.6F), sc.Location.X - 13, sc.Location.Y - 13, 26, 26);
                g.FillEllipse(Brushes.Black, sc.Location.X - 2, sc.Location.Y + 15, 4, 4);

                if (scooterImg != null)
                    g.DrawImage(scooterImg, sc.Location.X - 9, sc.Location.Y - 9, 18, 18);
                else
                    g.DrawString("🛴", new Font("Segoe UI", 9F), Brushes.Black, sc.Location.X - 8, sc.Location.Y - 8);

                if (sc.Battery <= 20)
                    g.FillEllipse(Brushes.Crimson, sc.Location.X + 5, sc.Location.Y - 13, 8, 8);
            }

            int zX = mapPanel.Width - 50;
            g.FillRectangle(Brushes.White, zX, 180, 36, 72);
            g.DrawRectangle(new Pen(Color.FromArgb(220, 224, 230)), zX, 180, 36, 72);
            g.DrawLine(new Pen(Color.FromArgb(220, 224, 230)), zX, 216, zX + 36, 216);
            g.DrawString("+", new Font("Segoe UI", 14F), Brushes.Gray, zX + 9, 186);
            g.DrawString("−", new Font("Segoe UI", 14F), Brushes.Gray, zX + 11, 222);

            if (CurrentUser.Role == "Client" && !tripStartTime.HasValue)
            {
                int qrBtnW = 140;
                int qrBtnX = (mapPanel.Width / 2) - (qrBtnW / 2);
                int qrBtnY = mapPanel.Height - 65;
                using (GraphicsPath path = GetRoundedPath(new Rectangle(qrBtnX, qrBtnY, qrBtnW, 40), 20))
                {
                    g.FillPath(new SolidBrush(Color.FromArgb(116, 59, 251)), path);
                    g.DrawString("⚏ QR-код", new Font("Segoe UI", 10F, FontStyle.Bold), Brushes.White, qrBtnX + 28, qrBtnY + 11);
                }
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2F;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void MapCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var sc in mapScooters)
            {
                int dist = (int)Math.Sqrt(Math.Pow(sc.Location.X - e.X, 2) + Math.Pow(sc.Location.Y - e.Y, 2));
                if (dist <= 18)
                {
                    SelectScooter(sc);
                    break;
                }
            }
        }

        private void SelectScooter(ScooterPoint sc)
        {
            selectedScooterId = sc.Id;
            selectedScooterQr = sc.Qr;
            selectedBattery = sc.Battery;
            selectedStatus = sc.Status;
            selectedCoords = "56.857177, 53.301067";

            mapPanel.Invalidate();

            if (CurrentUser.Role == "Client")
            {
                if (!tripStartTime.HasValue)
                {
                    pnlClientSheet.Visible = true;
                    lblClientQr.Text = sc.Qr;
                    lblClientBat.Text = $"{sc.Battery}%";
                    lblClientBat.BackColor = sc.Battery > 20 ? Color.FromArgb(0, 200, 83) : Color.FromArgb(235, 87, 87);
                    lblClientTime.Text = $"~{Math.Max(1, sc.Battery / 25)} ч хода";
                    lblLiveTimer.Visible = false;
                    pnlTariffs.Visible = true;

                    btnClientAction.Text = "Забронировать (50 ₽ + 9,99 ₽/мин)";
                    btnClientAction.BackColor = Color.FromArgb(254, 224, 0);
                    btnClientAction.ForeColor = Color.Black;
                }
            }
            else if (CurrentUser.Role == "Technician")
            {
                UpdateTechRemoteControl(sc.Id, sc.Qr, sc.Battery, sc.Status, selectedCoords);
            }
        }

        // =========================================================================
        // ПУЛЬТ ТЕХНИКА
        // =========================================================================
        private void UpdateTechRemoteControl(int id, string qr, int bat, string status, string coords)
        {
            lblTechQr.Text = qr;
            lblTechBatVal.Text = $"{bat}%";
            lblTechGpsCoords.Text = coords;

            int barW = (int)((bat / 100.0) * 120);
            pnlBatteryBar.Width = Math.Max(5, Math.Min(120, barW));
            pnlBatteryBar.BackColor = bat <= 20 ? Color.FromArgb(235, 87, 87) : Color.FromArgb(245, 183, 37);

            lblGear.Visible = (bat <= 20 || status == "NeedsService");

            if (bat <= 20 || status == "NeedsService")
            {
                btnSwapBattery.Enabled = true;
                btnSwapBattery.Text = "Заменить АКБ";
                btnSwapBattery.BackColor = Color.FromArgb(108, 92, 231);
            }
            else
            {
                btnSwapBattery.Enabled = false;
                btnSwapBattery.Text = "Замена не требуется (>20%)";
                btnSwapBattery.BackColor = Color.LightGray;
            }
        }

        private void ExecuteBatterySwap()
        {
            if (selectedScooterId == 0) return;

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand("UPDATE Scooter SET batteryLevel = 100, status = 'Available', errorCode = 0 WHERE id = ?", conn))
                    {
                        cmd.Parameters.AddWithValue("?", selectedScooterId);
                        cmd.ExecuteNonQuery();
                    }

                    using (OleDbCommand tCmd = new OleDbCommand("UPDATE Task SET status = 'Done' WHERE scooter_id = ? AND status <> 'Done'", conn))
                    {
                        tCmd.Parameters.AddWithValue("?", selectedScooterId);
                        tCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"АКБ самоката {selectedScooterQr} успешно заменена на 100%!");
                UpdateTechRemoteControl(selectedScooterId, selectedScooterQr, 100, "Available", selectedCoords);
                LoadScootersFromDb();
                LoadTechDbTasks();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void FindScooterManual(string search)
        {
            if (string.IsNullOrEmpty(search)) return;

            foreach (var sc in mapScooters)
            {
                if (sc.Qr.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    SelectScooter(sc);
                    MessageBox.Show($"Самокат {sc.Qr} найден и открыт в пульте!");
                    return;
                }
            }
            MessageBox.Show("Самокат не найден!");
        }

        // =========================================================================
        // РАСПРЕДЕЛЕНИЕ САМОКАТОВ ПО ПАРКОВКАМ
        // =========================================================================
        private void LoadScootersFromDb()
        {
            mapScooters.Clear();

            Point[] parkingHubs = {
                new Point(65, 235),
                new Point(200, 155),
                new Point(370, 175),
                new Point(230, 345),
                new Point(450, 390)
            };

            int[] hubCounters = new int[parkingHubs.Length];

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand("SELECT id, qrCode, batteryLevel, status FROM Scooter", conn))
                    using (OleDbDataReader r = cmd.ExecuteReader())
                    {
                        int index = 0;
                        while (r.Read())
                        {
                            int id = Convert.ToInt32(r["id"]);
                            string qr = r["qrCode"].ToString();
                            int bat = Convert.ToInt32(r["batteryLevel"]);
                            string status = r["status"].ToString();

                            int hubIdx = index % parkingHubs.Length;
                            int slot = hubCounters[hubIdx]++;

                            int offsetX = (slot % 3) * 28 - 28;
                            int offsetY = (slot / 3) * 30 - 15;

                            Point pt = new Point(parkingHubs[hubIdx].X + offsetX, parkingHubs[hubIdx].Y + offsetY);

                            mapScooters.Add(new ScooterPoint()
                            {
                                Id = id,
                                Qr = qr,
                                Battery = bat,
                                Status = status,
                                Location = pt
                            });

                            index++;
                        }
                    }
                }
                if (mapPanel != null) mapPanel.Invalidate();
            }
            catch { }
        }

        // =========================================================================
        // ЛОГИКА АРЕНДЫ (ТАЙМЕР И ТОЧНЫЙ РАСЧЕТ)
        // =========================================================================
        private void CheckActiveClientRent()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    string q = "SELECT TOP 1 Rent.id, Rent.scooter_id, Scooter.qrCode, Scooter.batteryLevel, Rent.startTime " +
                               "FROM Rent INNER JOIN Scooter ON Rent.scooter_id = Scooter.id " +
                               "WHERE Rent.user_id = ? AND Rent.status = 'Active' ORDER BY Rent.id DESC";

                    using (OleDbCommand cmd = new OleDbCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("?", CurrentUser.Id);
                        using (OleDbDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                currentActiveRentId = Convert.ToInt32(r["id"]);
                                currentActiveScooterId = Convert.ToInt32(r["scooter_id"]);
                                DateTime dbStart = Convert.ToDateTime(r["startTime"]);

                                if ((DateTime.Now - dbStart).TotalHours > 4)
                                {
                                    tripStartTime = DateTime.Now;
                                }
                                else
                                {
                                    tripStartTime = dbStart;
                                }

                                pnlClientSheet.Visible = true;
                                lblClientQr.Text = r["qrCode"].ToString();
                                lblClientBat.Text = $"{r["batteryLevel"]}%";
                                pnlTariffs.Visible = false;
                                lblLiveTimer.Visible = true;

                                btnClientAction.Text = "🛑 Завершить поездку";
                                btnClientAction.BackColor = Color.FromArgb(235, 60, 60);
                                btnClientAction.ForeColor = Color.White;

                                rideTimer.Start();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void BtnClientAction_Click(object sender, EventArgs e)
        {
            if (btnClientAction.Text.Contains("Завершить"))
            {
                rideTimer.Stop();

                DateTime start = tripStartTime.HasValue ? tripStartTime.Value : DateTime.Now.AddMinutes(-2);
                TimeSpan elapsed = DateTime.Now - start;

                int minutes = (int)Math.Ceiling(elapsed.TotalMinutes);
                if (minutes < 1) minutes = 1;
                if (minutes > 180) minutes = 2;

                int basePrice = 50;
                int minutePrice = (int)(minutes * 9.99);
                int totalCost = basePrice + minutePrice;

                try
                {
                    using (OleDbConnection conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();

                        string closeSql = "UPDATE Rent SET endTime = Now(), totalCost = ?, status = 'Completed', parkingPhoto = 'done.jpg' WHERE id = ?";
                        using (OleDbCommand cmd = new OleDbCommand(closeSql, conn))
                        {
                            cmd.Parameters.AddWithValue("?", totalCost);
                            cmd.Parameters.AddWithValue("?", currentActiveRentId);
                            cmd.ExecuteNonQuery();
                        }

                        string freeSql = "UPDATE Scooter SET status = 'Available', batteryLevel = IIF(batteryLevel > 8, batteryLevel - 8, 5) WHERE id = ?";
                        using (OleDbCommand freeCmd = new OleDbCommand(freeSql, conn))
                        {
                            freeCmd.Parameters.AddWithValue("?", currentActiveScooterId);
                            freeCmd.ExecuteNonQuery();
                        }
                    }

                    string receipt = $"🛴 Чек поездки — Urent 42\n" +
                                     $"─────────────────────────────\n" +
                                     $"Самокат: {lblClientQr.Text}\n" +
                                     $"Длительность: {minutes} мин. ({elapsed:mm\\:ss})\n\n" +
                                     $"• Старт аренды: 50 ₽\n" +
                                     $"• Тариф (9,99 ₽/мин): {minutePrice} ₽\n" +
                                     $"─────────────────────────────\n" +
                                     $"ИТОГО К ОПЛАТЕ: {totalCost} ₽\n\n" +
                                     $"Списано с карты: •••• 4242";

                    MessageBox.Show(receipt, "Поездка завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    tripStartTime = null;
                    currentActiveRentId = 0;
                    currentActiveScooterId = 0;

                    pnlClientSheet.Visible = false;
                    lblLiveTimer.Visible = false;
                    pnlTariffs.Visible = true;
                    btnClientAction.Text = "Забронировать (50 ₽ + 9,99 ₽/мин)";
                    btnClientAction.BackColor = Color.FromArgb(254, 224, 0);
                    btnClientAction.ForeColor = Color.Black;

                    LoadScootersFromDb();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
            else
            {
                if (selectedScooterId == 0)
                {
                    MessageBox.Show("Выберите самокат на карте!");
                    return;
                }

                if (selectedBattery <= 20)
                {
                    MessageBox.Show("Этот самокат разряжен и ожидает замены АКБ!");
                    return;
                }

                try
                {
                    using (OleDbConnection conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();

                        using (OleDbCommand cleanCmd = new OleDbCommand("UPDATE Rent SET status = 'Completed', totalCost = 150 WHERE user_id = ? AND status = 'Active'", conn))
                        {
                            cleanCmd.Parameters.AddWithValue("?", CurrentUser.Id);
                            cleanCmd.ExecuteNonQuery();
                        }

                        string rentSql = "INSERT INTO Rent (user_id, scooter_id, parking_id, startTime, totalCost, status) VALUES (?, ?, 1, Now(), 0, 'Active')";
                        using (OleDbCommand rentCmd = new OleDbCommand(rentSql, conn))
                        {
                            rentCmd.Parameters.AddWithValue("?", CurrentUser.Id);
                            rentCmd.Parameters.AddWithValue("?", selectedScooterId);
                            rentCmd.ExecuteNonQuery();
                        }

                        using (OleDbCommand idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                        {
                            currentActiveRentId = Convert.ToInt32(idCmd.ExecuteScalar());
                        }

                        using (OleDbCommand upd = new OleDbCommand("UPDATE Scooter SET status = 'InUse' WHERE id = ?", conn))
                        {
                            upd.Parameters.AddWithValue("?", selectedScooterId);
                            upd.ExecuteNonQuery();
                        }

                        currentActiveScooterId = selectedScooterId;
                        tripStartTime = DateTime.Now;

                        MessageBox.Show($"Самокат {selectedScooterQr} успешно разблокирован!\nПриятной поездки!", "Старт", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        pnlTariffs.Visible = false;
                        lblLiveTimer.Visible = true;
                        lblLiveTimer.Text = "⏱ Поездка: 00:00  •  К оплате: 50 ₽";

                        btnClientAction.Text = "🛑 Завершить поездку";
                        btnClientAction.BackColor = Color.FromArgb(235, 60, 60);
                        btnClientAction.ForeColor = Color.White;

                        rideTimer.Start();
                        LoadScootersFromDb();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        // =========================================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ БД
        // =========================================================================
        private void LoadTechDbTasks()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    string q = "SELECT Task.id, Task.taskType, Task.status, Scooter.qrCode, Task.scooter_id FROM Task INNER JOIN Scooter ON Task.scooter_id = Scooter.id WHERE Task.status <> 'Done'";
                    OleDbDataAdapter da = new OleDbDataAdapter(q, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch { }
        }

        private void LoadAdminTable(string table)
        {
            try
            {
                adapter = new OleDbDataAdapter($"SELECT * FROM [{table}]", connectionString);
                dtDb = new DataTable();
                adapter.Fill(dtDb);
                dataGridView1.DataSource = dtDb;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}