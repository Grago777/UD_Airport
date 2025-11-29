using System;
using System.Windows.Forms;
using UD_WForms.Models.Database;
using UD_WForms.Forms;

namespace UD_WForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // Проверяем подключение перед инициализацией БД
            if (DatabaseInitializer.TestDatabaseConnection())
            {
                InitializeDatabase();
            }
            else
            {
                // Показываем форму настройки подключения
                var settingsForm = new ConnectionSettingsForm();
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    if (DatabaseInitializer.TestDatabaseConnection())
                    {
                        InitializeDatabase();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось подключиться к базе данных. Приложение будет закрыто.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                DatabaseInitializer.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}\nПриложение будет работать в ограниченном режиме.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // MainForm
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Авиакасса - Главная";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Menu Strip
            MenuStrip mainMenu = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            ToolStripMenuItem ticketsMenu = new ToolStripMenuItem("Билеты");
            ToolStripMenuItem passengersMenu = new ToolStripMenuItem("Пассажиры");
            ToolStripMenuItem flightsMenu = new ToolStripMenuItem("Рейсы");
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");

            // Элементы меню
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, e) => Application.Exit();

            ToolStripMenuItem ticketsItem = new ToolStripMenuItem("Управление билетами");
            ticketsItem.Click += (s, e) => ShowTicketsForm();

            ToolStripMenuItem passengersItem = new ToolStripMenuItem("Управление пассажирами");
            passengersItem.Click += (s, e) => ShowPassengersForm();

            ToolStripMenuItem flightsItem = new ToolStripMenuItem("Управление рейсами");
            flightsItem.Click += (s, e) => ShowFlightsForm();

            // Добавление в меню
            fileMenu.DropDownItems.Add(exitItem);
            ticketsMenu.DropDownItems.Add(ticketsItem);
            passengersMenu.DropDownItems.Add(passengersItem);
            flightsMenu.DropDownItems.Add(flightsItem);

            mainMenu.Items.AddRange(new ToolStripItem[] { fileMenu, ticketsMenu, passengersMenu, flightsMenu, helpMenu });
            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // Welcome Label
            Label welcomeLabel = new Label();
            welcomeLabel.Text = "Добро пожаловать в систему авиакассы!\n\nВыберите раздел в меню для работы с данными.";
            welcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular);
            welcomeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            welcomeLabel.Dock = DockStyle.Fill;
            this.Controls.Add(welcomeLabel);

            this.ResumeLayout(false);
            this.PerformLayout();

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Настройки БД");
            settingsItem.Click += (s, e) =>
            {
                var settingsForm = new ConnectionSettingsForm();
                settingsForm.ShowDialog();
            };
            fileMenu.DropDownItems.Add(settingsItem);
        }

        private void ShowTicketsForm()
        {
            var ticketsForm = new TicketsForm();
            ticketsForm.ShowDialog();
        }

        private void ShowPassengersForm()
        {
            MessageBox.Show("Форма управления пассажирами будет реализована позже", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowFlightsForm()
        {
            MessageBox.Show("Форма управления рейсами будет реализована позже", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}