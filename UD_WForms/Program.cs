using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models.Database;

namespace UD_WForms
{
    internal static class Program
    {
        // Определяем имя базы данных по умолчанию
        private const string DEFAULT_DATABASE_NAME = "aviadb";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Проверяем наличие параметра командной строки
            string databaseName = GetDatabaseNameFromArgs() ?? DEFAULT_DATABASE_NAME;

            // Инициализация базы данных с указанным именем
            InitializeDatabase(databaseName);

            // Регистрация сервисов
            InitializeServices();

            // Запуск приложения с передачей имени БД
            Application.Run(new MainForm());
        }

        static string GetDatabaseNameFromArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                // Проверяем аргументы командной строки
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].StartsWith("--database=", StringComparison.OrdinalIgnoreCase) ||
                        args[i].StartsWith("-db=", StringComparison.OrdinalIgnoreCase))
                    {
                        return args[i].Split('=')[1];
                    }
                    else if (i == 1 && !args[i].StartsWith("-") && !args[i].StartsWith("/"))
                    {
                        // Если первый аргумент не флаг, считаем его именем БД
                        return args[i];
                    }
                }
            }
            return null;
        }

        static void InitializeDatabase(string databaseName = DEFAULT_DATABASE_NAME)
        {
            try
            {
                Console.WriteLine($"Попытка инициализации базы данных: {databaseName}");

                // Сначала проверяем подключение к системной БД
                if (DatabaseInitializer.TestDatabaseConnection("postgres"))
                {
                    Console.WriteLine("Подключение к PostgreSQL установлено");

                    // Проверяем/создаем конкретную БД
                    if (DatabaseInitializer.TestDatabaseConnection(databaseName))
                    {
                        Console.WriteLine($"База данных '{databaseName}' доступна");
                        DatabaseInitializer.InitializeDatabase(databaseName);
                    }
                    else
                    {
                        Console.WriteLine($"Создаем базу данных '{databaseName}'...");
                        DatabaseInitializer.InitializeDatabase(databaseName);
                    }
                }
                else
                {
                    Console.WriteLine("Ошибка подключения к PostgreSQL");
                    MessageBox.Show($"Не удалось подключиться к PostgreSQL. Пожалуйста, проверьте:\n" +
                                  "1. Запущен ли сервер PostgreSQL\n" +
                                  "2. Правильность параметров подключения\n" +
                                  "3. Наличие прав у пользователя для создания БД",
                                  "Ошибка подключения",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации базы данных '{databaseName}': {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void InitializeServices()
        {
            try
            {
                Console.WriteLine("Инициализация сервисов...");

                // Регистрируем сервисы
                ServiceLocator.Register<IPassengerService>(new PassengerService());
                ServiceLocator.Register<ITicketService>(new TicketService());
                ServiceLocator.Register<IFlightService>(new FlightService());
                ServiceLocator.Register<IAirportService>(new AirportService());

                Console.WriteLine("Сервисы успешно инициализированы");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации сервисов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}