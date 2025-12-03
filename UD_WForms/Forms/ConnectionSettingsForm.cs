using System;
using System.Windows.Forms;
using UD_WForms.Models.Database;

namespace UD_WForms.Forms
{
    public partial class ConnectionSettingsForm : Form
    {
        // Свойство для получения имени БД
        public string DatabaseName { get; private set; }

        // Поля формы
        private TextBox _txtHost;
        private TextBox _txtPort;
        private TextBox _txtDatabase;
        private TextBox _txtUsername;
        private TextBox _txtPassword;

        // Сохраняем оригинальную строку подключения для восстановления в случае ошибки
        private string _originalConnectionString;

        public ConnectionSettingsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
            _originalConnectionString = ConnectionFactory.GetConnectionString();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Настройки подключения к БД";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;

            int leftLabel = 20;
            int leftControl = 180;
            int controlWidth = 250;
            int spacing = 40;
            int top = 20;

            // Хост
            var lblHost = new Label()
            {
                Text = "Хост:",
                Left = leftLabel,
                Top = top,
                Width = 150,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F)
            };
            _txtHost = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = "localhost"
            };
            top += spacing;

            // Порт
            var lblPort = new Label()
            {
                Text = "Порт:",
                Left = leftLabel,
                Top = top,
                Width = 150,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F)
            };
            _txtPort = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = "5432"
            };
            top += spacing;

            // База данных
            var lblDatabase = new Label()
            {
                Text = "База данных:",
                Left = leftLabel,
                Top = top,
                Width = 150,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F)
            };
            _txtDatabase = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = "aviadb1"
            };
            top += spacing;

            // Пользователь
            var lblUsername = new Label()
            {
                Text = "Пользователь:",
                Left = leftLabel,
                Top = top,
                Width = 150,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F)
            };
            _txtUsername = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = "postgres"
            };
            top += spacing;

            // Пароль
            var lblPassword = new Label()
            {
                Text = "Пароль:",
                Left = leftLabel,
                Top = top,
                Width = 150,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F)
            };
            _txtPassword = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                UseSystemPasswordChar = true
            };
            top += 50;

            // Кнопка проверки подключения к системной БД
            var btnTestPostgres = new Button()
            {
                Text = "Проверить PostgreSQL",
                Left = leftLabel,
                Top = top,
                Width = 140,
                BackColor = System.Drawing.Color.LightBlue
            };

            // Кнопка проверки подключения к конкретной БД
            var btnTestDatabase = new Button()
            {
                Text = "Проверить БД",
                Left = leftLabel + 150,
                Top = top,
                Width = 100,
                BackColor = System.Drawing.Color.LightGreen
            };

            // Кнопка создания БД
            var btnCreateDatabase = new Button()
            {
                Text = "Создать БД",
                Left = leftLabel + 260,
                Top = top,
                Width = 100,
                BackColor = System.Drawing.Color.LightYellow
            };
            top += 40;

            // Кнопка просмотра существующих БД
            var btnListDatabases = new Button()
            {
                Text = "Список БД",
                Left = leftLabel,
                Top = top,
                Width = 100,
                BackColor = System.Drawing.Color.LightGray
            };

            // Кнопка выбора БД
            var btnSelectDatabase = new Button()
            {
                Text = "Выбрать БД",
                Left = leftLabel + 110,
                Top = top,
                Width = 100,
                BackColor = System.Drawing.Color.LightCyan
            };
            top += 40;

            // Основные кнопки
            var btnSave = new Button()
            {
                Text = "💾 Сохранить",
                Left = leftControl + 80,
                Top = top,
                Width = 120,
                BackColor = System.Drawing.Color.LightGreen
            };
            var btnCancel = new Button()
            {
                Text = "Отмена",
                Left = leftControl,
                Top = top,
                Width = 80
            };

            // Обработчики событий

            // Проверка подключения к PostgreSQL
            btnTestPostgres.Click += (s, e) =>
            {
                try
                {
                    // Временно устанавливаем строку подключения для теста
                    string tempConnectionString = $"Host={_txtHost.Text};Port={_txtPort.Text};" +
                                                 $"Database=postgres;Username={_txtUsername.Text};" +
                                                 $"Password={_txtPassword.Text}";

                    using (var connection = new Npgsql.NpgsqlConnection(tempConnectionString))
                    {
                        connection.Open();
                        MessageBox.Show("Подключение к PostgreSQL успешно!", "Тест подключения",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения к PostgreSQL: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Проверка подключения к конкретной БД
            btnTestDatabase.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtDatabase.Text))
                {
                    MessageBox.Show("Введите имя базы данных", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtDatabase.Focus();
                    return;
                }

                try
                {
                    // Временно устанавливаем строку подключения для теста
                    string tempConnectionString = $"Host={_txtHost.Text};Port={_txtPort.Text};" +
                                                 $"Database={_txtDatabase.Text};Username={_txtUsername.Text};" +
                                                 $"Password={_txtPassword.Text}";

                    using (var connection = new Npgsql.NpgsqlConnection(tempConnectionString))
                    {
                        connection.Open();
                        MessageBox.Show($"Подключение к базе данных '{_txtDatabase.Text}' успешно!", "Тест подключения",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения к базе данных '{_txtDatabase.Text}': {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Создание новой БД
            btnCreateDatabase.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtDatabase.Text))
                {
                    MessageBox.Show("Введите имя базы данных", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtDatabase.Focus();
                    return;
                }

                try
                {
                    // Сначала проверяем подключение к PostgreSQL
                    string postgresConnectionString = $"Host={_txtHost.Text};Port={_txtPort.Text};" +
                                                     $"Database=postgres;Username={_txtUsername.Text};" +
                                                     $"Password={_txtPassword.Text}";

                    using (var postgresConnection = new Npgsql.NpgsqlConnection(postgresConnectionString))
                    {
                        postgresConnection.Open();

                        // Проверяем существование базы данных
                        string checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{_txtDatabase.Text}'";
                        using (var checkCmd = new Npgsql.NpgsqlCommand(checkDbQuery, postgresConnection))
                        {
                            var result = checkCmd.ExecuteScalar();

                            if (result == null)
                            {
                                // Создаем базу данных
                                string createDbQuery = $"CREATE DATABASE \"{_txtDatabase.Text}\" ENCODING 'UTF8'";
                                using (var createCmd = new Npgsql.NpgsqlCommand(createDbQuery, postgresConnection))
                                {
                                    createCmd.ExecuteNonQuery();

                                    // Даем время на создание БД
                                    System.Threading.Thread.Sleep(2000);

                                    // Инициализируем новую БД
                                    string newDbConnectionString = $"Host={_txtHost.Text};Port={_txtPort.Text};" +
                                                                  $"Database={_txtDatabase.Text};Username={_txtUsername.Text};" +
                                                                  $"Password={_txtPassword.Text}";

                                    // Устанавливаем строку подключения для инициализации
                                    ConnectionFactory.SetConnectionString(newDbConnectionString);

                                    try
                                    {
                                        DatabaseInitializer.InitializeDatabase(_txtDatabase.Text);

                                        MessageBox.Show($"База данных '{_txtDatabase.Text}' успешно создана и инициализирована!", "Успех",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    catch (Exception initEx)
                                    {
                                        MessageBox.Show($"Ошибка при инициализации БД: {initEx.Message}", "Ошибка",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        // Восстанавливаем оригинальную строку подключения
                                        ConnectionFactory.SetConnectionString(_originalConnectionString);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"База данных '{_txtDatabase.Text}' уже существует.", "Информация",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании базы данных: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Просмотр списка существующих БД
            btnListDatabases.Click += (s, e) =>
            {
                try
                {
                    string postgresConnectionString = $"Host={_txtHost.Text};Port={_txtPort.Text};" +
                                                     $"Database=postgres;Username={_txtUsername.Text};" +
                                                     $"Password={_txtPassword.Text}";

                    using (var postgresConnection = new Npgsql.NpgsqlConnection(postgresConnectionString))
                    {
                        postgresConnection.Open();

                        string query = @"
                            SELECT datname, 
                                   pg_size_pretty(pg_database_size(datname)) as size,
                                   pg_encoding_to_char(encoding) as encoding
                            FROM pg_database 
                            WHERE datistemplate = false 
                            ORDER BY datname";

                        using (var cmd = new Npgsql.NpgsqlCommand(query, postgresConnection))
                        using (var reader = cmd.ExecuteReader())
                        {
                            string databases = "Существующие базы данных:\n\n";
                            int count = 0;

                            while (reader.Read())
                            {
                                databases += $"• {reader.GetString(0)} ({reader.GetString(1)}) - {reader.GetString(2)}\n";
                                count++;
                            }

                            if (count == 0)
                                databases = "Базы данных не найдены";

                            MessageBox.Show(databases, "Список баз данных",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении списка БД: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Выбор БД из списка
            btnSelectDatabase.Click += (s, e) =>
            {
                try
                {
                    string postgresConnectionString = $"Host={_txtHost.Text};Port={_txtPort.Text};" +
                                                     $"Database=postgres;Username={_txtUsername.Text};" +
                                                     $"Password={_txtPassword.Text}";

                    using (var postgresConnection = new Npgsql.NpgsqlConnection(postgresConnectionString))
                    {
                        postgresConnection.Open();

                        string query = "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname";

                        using (var cmd = new Npgsql.NpgsqlCommand(query, postgresConnection))
                        using (var reader = cmd.ExecuteReader())
                        {
                            var databases = new System.Collections.Generic.List<string>();

                            while (reader.Read())
                            {
                                databases.Add(reader.GetString(0));
                            }

                            if (databases.Count == 0)
                            {
                                MessageBox.Show("Базы данных не найдены", "Информация",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            using (var selectForm = new Form())
                            {
                                selectForm.Text = "Выбор базы данных";
                                selectForm.Size = new System.Drawing.Size(300, 400);
                                selectForm.StartPosition = FormStartPosition.CenterParent;

                                var listBox = new ListBox();
                                listBox.Dock = DockStyle.Fill;
                                listBox.Items.AddRange(databases.ToArray());

                                var btnSelect = new Button()
                                {
                                    Text = "Выбрать",
                                    Dock = DockStyle.Bottom,
                                    Height = 30
                                };

                                btnSelect.Click += (sender, args) =>
                                {
                                    if (listBox.SelectedItem != null)
                                    {
                                        _txtDatabase.Text = listBox.SelectedItem.ToString();
                                        selectForm.DialogResult = DialogResult.OK;
                                        selectForm.Close();
                                    }
                                };

                                selectForm.Controls.Add(listBox);
                                selectForm.Controls.Add(btnSelect);

                                selectForm.ShowDialog();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выборе БД: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Сохранение настроек
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtDatabase.Text))
                {
                    MessageBox.Show("Введите имя базы данных", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtDatabase.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtHost.Text))
                {
                    MessageBox.Show("Введите хост", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtHost.Focus();
                    return;
                }

                try
                {
                    // Создаем новую строку подключения
                    string newConnectionString = $"Host={_txtHost.Text};Port={_txtPort.Text};" +
                                                $"Database={_txtDatabase.Text};Username={_txtUsername.Text};" +
                                                $"Password={_txtPassword.Text}";

                    // Проверяем подключение
                    using (var testConnection = new Npgsql.NpgsqlConnection(newConnectionString))
                    {
                        testConnection.Open();
                    }

                    // Сохраняем настройки в ConnectionFactory
                    ConnectionFactory.SetConnectionString(newConnectionString);

                    // Сохраняем имя БД в свойство
                    DatabaseName = _txtDatabase.Text;

                    MessageBox.Show($"Настройки сохранены успешно!\nБаза данных: {DatabaseName}", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}\n\nПроверьте параметры подключения.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Отмена
            btnCancel.Click += (s, e) =>
            {
                // Восстанавливаем оригинальную строку подключения
                ConnectionFactory.SetConnectionString(_originalConnectionString);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new Control[] {
                lblHost, _txtHost,
                lblPort, _txtPort,
                lblDatabase, _txtDatabase,
                lblUsername, _txtUsername,
                lblPassword, _txtPassword,
                btnTestPostgres, btnTestDatabase, btnCreateDatabase,
                btnListDatabases, btnSelectDatabase,
                btnSave, btnCancel
            });

            this.ResumeLayout(false);
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Получаем текущую строку подключения
                string currentConnectionString = ConnectionFactory.GetConnectionString();

                if (!string.IsNullOrEmpty(currentConnectionString))
                {
                    // Парсим строку подключения
                    var parameters = currentConnectionString.Split(';');

                    foreach (var param in parameters)
                    {
                        if (param.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
                            _txtHost.Text = param.Substring(5);
                        else if (param.StartsWith("Port=", StringComparison.OrdinalIgnoreCase))
                            _txtPort.Text = param.Substring(5);
                        else if (param.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                            _txtDatabase.Text = param.Substring(9);
                        else if (param.StartsWith("Username=", StringComparison.OrdinalIgnoreCase))
                            _txtUsername.Text = param.Substring(9);
                        else if (param.StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                            _txtPassword.Text = param.Substring(9);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
                // Используем значения по умолчанию
            }
        }
    }
}