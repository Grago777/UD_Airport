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
        private Panel _contentPanel;
        private TableLayoutPanel _mainContainer;
        private StatusStrip _statusStrip;
        private MenuStrip _mainMenu;
        private Label _welcomeLabel;
        private string _databaseName = "aviadb";

        public MainForm()
        {
            InitializeComponent();

            // Проверяем подключение перед инициализацией БД
            if (DatabaseInitializer.TestDatabaseConnection(_databaseName))
            {
                InitializeDatabase(_databaseName);
            }
            else
            {
                // Показываем форму настройки подключения
                var settingsForm = new ConnectionSettingsForm();
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Получаем новое имя БД из настроек (если оно там есть)
                    if (!string.IsNullOrEmpty(settingsForm.DatabaseName))
                    {
                        _databaseName = settingsForm.DatabaseName;
                    }

                    if (DatabaseInitializer.TestDatabaseConnection(_databaseName))
                    {
                        InitializeDatabase(_databaseName);
                    }
                    else
                    {
                        MessageBox.Show($"Не удалось подключиться к базе данных '{_databaseName}'. Приложение будет закрыто.", "Ошибка",
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

        private void InitializeDatabase(string databaseName)
        {
            try
            {
                DatabaseInitializer.InitializeDatabase(databaseName);
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

            // Основные свойства формы
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Text = $"Авиакасса - Система управления билетами [{_databaseName}]";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.AutoScaleMode = AutoScaleMode.Font;

            // Главный контейнер с TableLayoutPanel для гибкого управления
            _mainContainer = new TableLayoutPanel();
            _mainContainer.Dock = DockStyle.Fill;
            _mainContainer.RowCount = 3;
            _mainContainer.ColumnCount = 1;
            _mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // Меню
            _mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Контент
            _mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // Статус бар
            _mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Меню (автоматический размер)
            _mainMenu = CreateMainMenu();
            _mainMenu.Dock = DockStyle.Fill;

            // Контентная панель (растягивается на все доступное пространство)
            _contentPanel = new Panel();
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.BackColor = System.Drawing.Color.White;
            _contentPanel.AutoScroll = true;
            _contentPanel.Padding = new Padding(40);

            // Приветственный текст
            _welcomeLabel = new Label();
            _welcomeLabel.Text = $"Добро пожаловать в систему авиакассы!\n\n" +
                               $"Текущая база данных: {_databaseName}\n\n" +
                               "Для работы с системой выберите соответствующий раздел в меню.\n\n" +
                               "Доступные модули:\n" +
                               "• Билеты - управление продажей билетов\n" +
                               "• Пассажиры - база данных пассажиров\n" +
                               "• Рейсы - расписание и управление рейсами\n" +
                               "• Аэропорты - справочник аэропортов";
            _welcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular);
            _welcomeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _welcomeLabel.Dock = DockStyle.Fill;
            _welcomeLabel.AutoSize = true;  // Автоматический размер текста
            _welcomeLabel.Padding = new Padding(20);

            // Создаем панель для центрирования текста
            Panel centerPanel = new Panel();
            centerPanel.Dock = DockStyle.Fill;
            centerPanel.BackColor = System.Drawing.Color.White;

            // Используем TableLayoutPanel для центрирования
            TableLayoutPanel tableCenter = new TableLayoutPanel();
            tableCenter.Dock = DockStyle.Fill;
            tableCenter.RowCount = 1;
            tableCenter.ColumnCount = 1;
            tableCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Панель для текста
            Panel textContainer = new Panel();
            textContainer.Anchor = AnchorStyles.None;
            textContainer.AutoSize = true;
            textContainer.Controls.Add(_welcomeLabel);

            // Добавляем текст в центрирующую таблицу
            tableCenter.Controls.Add(textContainer, 0, 0);
            tableCenter.SetCellPosition(textContainer, new TableLayoutPanelCellPosition(0, 0));

            centerPanel.Controls.Add(tableCenter);
            _contentPanel.Controls.Add(centerPanel);

            // Статус бар (автоматический размер)
            _statusStrip = new StatusStrip();
            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = $"Готов к работе (БД: {_databaseName})";
            statusLabel.Spring = true;  // Растягивается по ширине
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _statusStrip.Items.Add(statusLabel);
            _statusStrip.Dock = DockStyle.Fill;

            // Добавляем элементы в главный контейнер
            _mainContainer.Controls.Add(_mainMenu, 0, 0);
            _mainContainer.Controls.Add(_contentPanel, 0, 1);
            _mainContainer.Controls.Add(_statusStrip, 0, 2);

            // Добавляем главный контейнер на форму
            this.Controls.Add(_mainContainer);

            // Устанавливаем MainMenuStrip для корректной работы меню
            this.MainMenuStrip = _mainMenu;

            this.ResumeLayout(true);
            this.PerformLayout();
        }

        private MenuStrip CreateMainMenu()
        {
            MenuStrip mainMenu = new MenuStrip();

            // Меню Файл
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Настройки БД");
            settingsItem.Click += (s, e) => ShowConnectionSettings();
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, e) => Application.Exit();
            fileMenu.DropDownItems.Add(settingsItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitItem);

            // Меню Билеты
            ToolStripMenuItem ticketsMenu = new ToolStripMenuItem("Билеты");
            ToolStripMenuItem manageTicketsItem = new ToolStripMenuItem("Управление билетами");
            manageTicketsItem.Click += (s, e) => ShowTicketsForm();
            ticketsMenu.DropDownItems.Add(manageTicketsItem);

            // Меню Пассажиры
            ToolStripMenuItem passengersMenu = new ToolStripMenuItem("Пассажиры");
            ToolStripMenuItem managePassengersItem = new ToolStripMenuItem("Управление пассажирами");
            managePassengersItem.Click += (s, e) => ShowPassengersForm();
            passengersMenu.DropDownItems.Add(managePassengersItem);

            // Меню Рейсы
            ToolStripMenuItem flightsMenu = new ToolStripMenuItem("Рейсы");
            ToolStripMenuItem manageFlightsItem = new ToolStripMenuItem("Управление рейсами");
            manageFlightsItem.Click += (s, e) => ShowFlightsForm();
            flightsMenu.DropDownItems.Add(manageFlightsItem);

            // Меню Аэропорты
            ToolStripMenuItem airportsMenu = new ToolStripMenuItem("Аэропорты");
            ToolStripMenuItem manageAirportsItem = new ToolStripMenuItem("Управление аэропортами");
            manageAirportsItem.Click += (s, e) => ShowAirportsForm();
            airportsMenu.DropDownItems.Add(manageAirportsItem);

            // Добавляем все меню в главное меню
            mainMenu.Items.AddRange(new ToolStripItem[] {
                fileMenu, ticketsMenu, passengersMenu, flightsMenu, airportsMenu
            });

            return mainMenu;
        }

        // Метод для смены контента на форме
        public void SetContent(Control content)
        {
            _contentPanel.Controls.Clear();
            content.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(content);
        }

        // Метод для возврата к приветственному экрану
        public void ShowWelcomeScreen()
        {
            SetContent(_contentPanel); // Восстанавливаем оригинальный контент
        }

        // Метод обновления статус-бара
        public void UpdateStatus(string message)
        {
            if (_statusStrip.Items.Count > 0)
            {
                _statusStrip.Items[0].Text = message;
            }
        }

        private void ShowConnectionSettings()
        {
            var settingsForm = new ConnectionSettingsForm();
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                // Обновляем имя БД, если оно изменилось
                if (!string.IsNullOrEmpty(settingsForm.DatabaseName) && settingsForm.DatabaseName != _databaseName)
                {
                    _databaseName = settingsForm.DatabaseName;

                    // Обновляем заголовок формы
                    this.Text = $"Авиакасса - Система управления билетами [{_databaseName}]";

                    // Обновляем статус бар
                    UpdateStatus($"Готов к работе (БД: {_databaseName})");

                    // Обновляем приветственный текст
                    _welcomeLabel.Text = _welcomeLabel.Text.Replace($"Текущая база данных: {_databaseName}",
                        $"Текущая база данных: {_databaseName}");

                    // Переинициализируем сервисы с новой БД
                    InitializeServices();
                }
            }
        }
        private void InitializeServices()
        {
            try
            {
                // Перерегистрируем сервисы с новой БД
                ServiceLocator.Register<IPassengerService>(new PassengerService());
                ServiceLocator.Register<ITicketService>(new TicketService());
                ServiceLocator.Register<IFlightService>(new FlightService());
                ServiceLocator.Register<IAirportService>(new AirportService());

                MessageBox.Show($"Сервисы переинициализированы для базы данных '{_databaseName}'", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации сервисов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                if (!ServiceLocator.IsRegistered<IFlightService>() || !ServiceLocator.IsRegistered<IAirportService>())
                {
                    MessageBox.Show("Не все необходимые сервисы зарегистрированы", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var flightsForm = new FlightsForm();
                flightsForm.WindowState = FormWindowState.Maximized;
                flightsForm.Show();

                if (flightsForm.IsHandleCreated)
                {
                    Console.WriteLine("Форма рейсов успешно создана");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы рейсов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAirportsForm()
        {
            AirportsForm airportsForm = new AirportsForm();
            airportsForm.Show();
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