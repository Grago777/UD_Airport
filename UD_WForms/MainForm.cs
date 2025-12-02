using System;
using System.Windows.Forms;
using UD_WForms.Models.Database;
using UD_WForms.Forms;
using System.Diagnostics;
using UD_WForms.Services;

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
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Text = "Авиакасса - Система управления билетами";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true;

            // Menu Strip
            MenuStrip mainMenu = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            ToolStripMenuItem ticketsMenu = new ToolStripMenuItem("Билеты");
            ToolStripMenuItem passengersMenu = new ToolStripMenuItem("Пассажиры");
            ToolStripMenuItem flightsMenu = new ToolStripMenuItem("Рейсы");
            ToolStripMenuItem airportsMenu = new ToolStripMenuItem("Аэропорты");
            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("Отчеты");
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");

            // Элементы меню Файл
            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Настройки БД");
            settingsItem.Click += (s, e) => ShowConnectionSettings();

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, e) => Application.Exit();

            fileMenu.DropDownItems.Add(settingsItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitItem);

            // Элементы меню Билеты
            ToolStripMenuItem manageTicketsItem = new ToolStripMenuItem("Управление билетами");
            manageTicketsItem.Click += (s, e) => ShowTicketsForm();

            ToolStripMenuItem searchTicketsItem = new ToolStripMenuItem("Поиск билетов");
            searchTicketsItem.Click += (s, e) => ShowSearchForm("tickets");

            ticketsMenu.DropDownItems.Add(manageTicketsItem);
            ticketsMenu.DropDownItems.Add(searchTicketsItem);
            


            // Элементы меню Пассажиры
            ToolStripMenuItem managePassengersItem = new ToolStripMenuItem("Управление пассажирами");
            managePassengersItem.Click += (s, e) => ShowPassengersForm();

            passengersMenu.DropDownItems.Add(managePassengersItem);

            // Элементы меню Рейсы
            ToolStripMenuItem manageFlightsItem = new ToolStripMenuItem("Управление рейсами");
            manageFlightsItem.Click += (s, e) => ShowFlightsForm();

            flightsMenu.DropDownItems.Add(manageFlightsItem);

            // Элементы меню Аэропорты
            ToolStripMenuItem manageAirportsItem = new ToolStripMenuItem("Управление аэропортами");
            manageAirportsItem.Click += (s, e) => ShowAirportsForm();

            airportsMenu.DropDownItems.Add(manageAirportsItem);

            mainMenu.Items.AddRange(new ToolStripItem[] {
                fileMenu, ticketsMenu, passengersMenu, flightsMenu, airportsMenu, reportsMenu, helpMenu
            });

            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // Welcome Panel
            Panel welcomePanel = new Panel();
            welcomePanel.Dock = DockStyle.Fill;
            welcomePanel.BackColor = System.Drawing.Color.White;

            Label welcomeLabel = new Label();
            welcomeLabel.Text = "Добро пожаловать в систему авиакассы!\n\n" +
                               "Для работы с системой выберите соответствующий раздел в меню.\n\n" +
                               "Доступные модули:\n" +
                               "• Билеты - управление продажей билетов\n" +
                               "• Пассажиры - база данных пассажиров\n" +
                               "• Рейсы - расписание и управление рейсами\n" +
                               "• Аэропорты - справочник аэропортов";
            welcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular);
            welcomeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            welcomeLabel.Dock = DockStyle.Fill;
            welcomeLabel.Padding = new Padding(20);

            welcomePanel.Controls.Add(welcomeLabel);
            this.Controls.Add(welcomePanel);

            // Status Strip
            StatusStrip statusStrip = new StatusStrip();
            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "Готов к работе";
            statusStrip.Items.Add(statusLabel);

            this.Controls.Add(statusStrip);

            this.ResumeLayout(false);
            this.PerformLayout();
            this.SuspendLayout();

            // MainForm
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Text = "Авиакасса - Система управления билетами";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true; // Включаем поддержку MDI
        }

        private void ShowConnectionSettings()
        {
            var settingsForm = new ConnectionSettingsForm();
            settingsForm.ShowDialog();
        }

        private void ShowTicketsForm()
        {
            var ticketsForm = new TicketsForm();
            ticketsForm.Show();
        }

        private void ShowPassengersForm()
        {
            var passengersForm = new PassengersForm();
            passengersForm.Show();
        }

        private void ShowFlightsForm()
        {
            try
            {
                // Проверяем сервисы
                if (!ServiceLocator.IsRegistered<IFlightService>() || !ServiceLocator.IsRegistered<IAirportService>())
                {
                    MessageBox.Show("Не все необходимые сервисы зарегистрированы", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var flightsForm = new FlightsForm();
                //flightsForm.MdiParent = this;
                //flightsForm.WindowState = FormWindowState.Maximized;
                flightsForm.Show();

                // Проверяем, загрузились ли данные
                if (flightsForm.IsHandleCreated)
                {
                    Console.WriteLine("Форма рейсов успешно создана");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы рейсов: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAirportsForm()
        {
            AirportsForm airportsForm = new AirportsForm();
            //airportsForm.MdiParent = this;
            airportsForm.Show();
        }

        private void ShowSearchForm(string searchType)
        {
            MessageBox.Show($"Форма поиска {searchType} будет реализована в следующем этапе", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void ShowAddFlightForm()
        {
            try
            {
                var flightService = ServiceLocator.GetService<IFlightService>();
                var airportService = ServiceLocator.GetService<IAirportService>();
                using (var form = new FlightForm(null, flightService, airportService))
                {
                   if (form.ShowDialog() == DialogResult.OK)
                    {
                        RefreshAllData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void RefreshAllData()
        {
            foreach (Form childForm in this.MdiChildren)
            {
                // Просто активируем форму, если нужно обновить - формы сами обновляются при активации
                childForm.Activate();
            }

            MessageBox.Show("Данные обновлены", "Обновление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ShowDebugFlightForm()
        {
            var debugForm = new DebugFlightForm();
            debugForm.ShowDialog();
        }
        
        
    }
}