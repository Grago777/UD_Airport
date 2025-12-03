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
        private ToolStripStatusLabel _statusLabel;

        // Добавляем поле для хранения имени БД
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
                    // Получаем новое имя БД из настроек
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
                // Обновляем статус после успешной инициализации
                UpdateStatus($"База данных '{databaseName}' инициализирована");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных '{databaseName}': {ex.Message}\nПриложение будет работать в ограниченном режиме.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateStatus($"Ошибка инициализации БД '{databaseName}'");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Основные свойства формы
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Text = $"Авиакасса - Система управления билетами (БД: {_databaseName})";
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

            // Меню (автоматический размер) с отображением БД
            _mainMenu = CreateMainMenu();
            _mainMenu.Dock = DockStyle.Fill;

            // Контентная панель (растягивается на все доступное пространство)
            _contentPanel = new Panel();
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.BackColor = System.Drawing.Color.White;
            _contentPanel.AutoScroll = true;
            _contentPanel.Padding = new Padding(40);

            // Приветственный текст с отображением БД
            _welcomeLabel = new Label();
            _welcomeLabel.Text = $"Добро пожаловать в систему авиакассы!\n\n" +
                               $"🛢️ Текущая база данных: <b>{_databaseName}</b>\n\n" +
                               "Для работы с системой выберите соответствующий раздел в меню.\n\n" +
                               "Доступные модули:\n" +
                               "• 📋 Билеты - управление продажей билетов\n" +
                               "• 👥 Пассажиры - база данных пассажиров\n" +
                               "• ✈️ Рейсы - расписание и управление рейсами\n" +
                               "• 🏢 Аэропорты - справочник аэропортов";
            _welcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular);
            _welcomeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _welcomeLabel.Dock = DockStyle.Fill;
            _welcomeLabel.AutoSize = true;
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

            // Статус бар с отображением информации о БД
            _statusStrip = new StatusStrip();

            // Левый статус - информация о БД
            ToolStripStatusLabel dbLabel = new ToolStripStatusLabel();
            dbLabel.Text = $"🛢️ БД: {_databaseName}";
            dbLabel.BorderSides = ToolStripStatusLabelBorderSides.Right;
            dbLabel.BorderStyle = Border3DStyle.Etched;
            dbLabel.AutoSize = false;
            dbLabel.Width = 150;

            // Основной статус
            _statusLabel = new ToolStripStatusLabel();
            _statusLabel.Text = "Готов к работе";
            _statusLabel.Spring = true;  // Растягивается по ширине
            _statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Правая часть - информация о пользователе или время
            ToolStripStatusLabel timeLabel = new ToolStripStatusLabel();
            timeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            timeLabel.AutoSize = false;
            timeLabel.Width = 120;

            _statusStrip.Items.AddRange(new ToolStripItem[] {
                dbLabel, _statusLabel, timeLabel
            });
            _statusStrip.Dock = DockStyle.Fill;

            // Добавляем элементы в главный контейнер
            _mainContainer.Controls.Add(_mainMenu, 0, 0);
            _mainContainer.Controls.Add(_contentPanel, 0, 1);
            _mainContainer.Controls.Add(_statusStrip, 0, 2);

            // Добавляем главный контейнер на форму
            this.Controls.Add(_mainContainer);

            // Устанавливаем MainMenuStrip для корректной работы меню
            this.MainMenuStrip = _mainMenu;

            //// Таймер для обновления времени в статус баре
            //Timer timer = new Timer();
            //timer.Interval = 60000; // 1 минута
            //timer.Tick += (s, e) => {
            //    timeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            //};
            //timer.Start();

            this.ResumeLayout(true);
            this.PerformLayout();
        }

        private MenuStrip CreateMainMenu()
        {
            MenuStrip mainMenu = new MenuStrip();

            // Меню Файл с отображением БД в подменю
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");

            // Пункт меню с текущей БД
            ToolStripMenuItem currentDbItem = new ToolStripMenuItem($"Текущая БД: {_databaseName}");
            currentDbItem.Enabled = false;
            currentDbItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic);

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Настройки подключения БД");
            settingsItem.Click += (s, e) => ShowConnectionSettings();

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, e) => Application.Exit();

            fileMenu.DropDownItems.Add(currentDbItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
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
            if (_statusLabel != null)
            {
                _statusLabel.Text = message;
            }
        }

        // Обновление информации о БД в интерфейсе
        private void UpdateDatabaseInfoInUI()
        {
            // Обновляем заголовок формы
            this.Text = $"Авиакасса - Система управления билетами (БД: {_databaseName})";

            // Обновляем приветственный текст
            _welcomeLabel.Text = $"Добро пожаловать в систему авиакассы!\n\n" +
                               $"🛢️ Текущая база данных: <b>{_databaseName}</b>\n\n" +
                               "Для работы с системой выберите соответствующий раздел в меню.\n\n" +
                               "Доступные модули:\n" +
                               "• 📋 Билеты - управление продажей билетов\n" +
                               "• 👥 Пассажиры - база данных пассажиров\n" +
                               "• ✈️ Рейсы - расписание и управление рейсами\n" +
                               "• 🏢 Аэропорты - справочник аэропортов";

            // Обновляем статус бар
            ToolStripStatusLabel dbLabel = _statusStrip.Items[0] as ToolStripStatusLabel;
            if (dbLabel != null)
            {
                dbLabel.Text = $"🛢️ БД: {_databaseName}";
            }

            // Обновляем меню
            UpdateMenuDatabaseInfo();
        }

        // Обновление информации о БД в меню
        private void UpdateMenuDatabaseInfo()
        {
            if (_mainMenu.Items.Count > 0 && _mainMenu.Items[0] is ToolStripMenuItem fileMenu)
            {
                // Обновляем первый пункт меню (текущая БД)
                if (fileMenu.DropDownItems.Count > 0)
                {
                    fileMenu.DropDownItems[0].Text = $"Текущая БД: {_databaseName}";
                }
            }
        }

        private void ShowConnectionSettings()
        {
            var settingsForm = new ConnectionSettingsForm();
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                // Получаем новое имя БД
                string newDatabaseName = settingsForm.DatabaseName;
                if (!string.IsNullOrEmpty(newDatabaseName) && newDatabaseName != _databaseName)
                {
                    _databaseName = newDatabaseName;

                    // Обновляем интерфейс
                    UpdateDatabaseInfoInUI();

                    // Переинициализируем сервисы с новой БД
                    InitializeServices();

                    MessageBox.Show($"Переключение на базу данных '{_databaseName}' выполнено успешно!", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (newDatabaseName == _databaseName)
                {
                    // Если БД не изменилась, просто обновляем статус
                    UpdateStatus($"Подключено к БД: {_databaseName}");
                }
            }
        }

        private void InitializeServices()
        {
            try
            {
                UpdateStatus($"Инициализация сервисов для БД: {_databaseName}...");

                // Перерегистрируем сервисы с новой БД
                ServiceLocator.Register<IPassengerService>(new PassengerService());
                ServiceLocator.Register<ITicketService>(new TicketService());
                ServiceLocator.Register<IFlightService>(new FlightService());
                ServiceLocator.Register<IAirportService>(new AirportService());

                UpdateStatus($"Сервисы инициализированы для БД: {_databaseName}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка инициализации сервисов");
                MessageBox.Show($"Ошибка инициализации сервисов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowTicketsForm()
        {
            UpdateStatus($"Открытие формы билетов (БД: {_databaseName})...");
            var ticketsForm = new TicketsForm();
            ticketsForm.Text = $"Управление билетами - {_databaseName}";
            ticketsForm.Show();
            UpdateStatus($"Форма билетов открыта");
        }

        private void ShowPassengersForm()
        {
            UpdateStatus($"Открытие формы пассажиров (БД: {_databaseName})...");
            var passengersForm = new PassengersForm();
            passengersForm.Text = $"Управление пассажирами - {_databaseName}";
            passengersForm.Show();
            UpdateStatus($"Форма пассажиров открыта");
        }

        private void ShowFlightsForm()
        {
            try
            {
                UpdateStatus($"Открытие формы рейсов (БД: {_databaseName})...");

                if (!ServiceLocator.IsRegistered<IFlightService>() || !ServiceLocator.IsRegistered<IAirportService>())
                {
                    MessageBox.Show("Не все необходимые сервисы зарегистрированы", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var flightsForm = new FlightsForm();
                flightsForm.Text = $"Управление рейсами - {_databaseName}";
                flightsForm.WindowState = FormWindowState.Maximized;
                flightsForm.Show();

                if (flightsForm.IsHandleCreated)
                {
                    UpdateStatus($"Форма рейсов открыта");
                    Console.WriteLine($"Форма рейсов успешно создана для БД: {_databaseName}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка открытия формы рейсов");
                MessageBox.Show($"Ошибка открытия формы рейсов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAirportsForm()
        {
            UpdateStatus($"Открытие формы аэропортов (БД: {_databaseName})...");
            AirportsForm airportsForm = new AirportsForm();
            airportsForm.Text = $"Управление аэропортами - {_databaseName}";
            airportsForm.Show();
            UpdateStatus($"Форма аэропортов открыта");
        }

        private void ShowAddFlightForm()
        {
            try
            {
                UpdateStatus($"Создание нового рейса (БД: {_databaseName})...");

                var flightService = ServiceLocator.GetService<IFlightService>();
                var airportService = ServiceLocator.GetService<IAirportService>();
                using (var form = new FlightForm(null, flightService, airportService))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        RefreshAllData();
                    }
                }

                UpdateStatus($"Новый рейс создан");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка создания рейса");
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

            UpdateStatus($"Данные обновлены в БД: {_databaseName}");
            MessageBox.Show($"Данные обновлены в базе данных '{_databaseName}'", "Обновление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowDebugFlightForm()
        {
            var debugForm = new DebugFlightForm();
            debugForm.ShowDialog();
        }
    }
}