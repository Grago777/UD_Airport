using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models.Database;

namespace UD_WForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Инициализация базы данных
            InitializeDatabase();

            // Регистрация сервисов
            InitializeServices();

            // Запуск приложения
            Application.Run(new MainForm());
        }

        static void InitializeDatabase()
        {
            if (DatabaseInitializer.TestDatabaseConnection())
            {
                DatabaseInitializer.InitializeDatabase();
            }
        }

        static void InitializeServices()
        {
            try
            {
                // Регистрируем сервисы
                ServiceLocator.Register<IPassengerService>(new PassengerService());
                ServiceLocator.Register<ITicketService>(new TicketService());
                ServiceLocator.Register<IFlightService>(new FlightService());
                ServiceLocator.Register<IAirportService>(new AirportService());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации сервисов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}