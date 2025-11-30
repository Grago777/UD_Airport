using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;

namespace UD_WForms.Forms
{
    public partial class DebugFlightForm : Form
    {
        private IFlightService _flightService;
        private IAirportService _airportService;

        public DebugFlightForm()
        {
            InitializeComponent();
            _flightService = ServiceLocator.GetService<IFlightService>();
            _airportService = ServiceLocator.GetService<IAirportService>();

            TestServices();
        }

        private void InitializeComponent()
        {
            this.Text = "Отладка - Создание рейса";
            this.Size = new System.Drawing.Size(400, 300);

            var btnTest = new Button() { Text = "Тест создания рейса", Location = new System.Drawing.Point(50, 50), Size = new System.Drawing.Size(150, 30) };
            btnTest.Click += BtnTest_Click;

            var btnCheckFlights = new Button() { Text = "Проверить рейсы", Location = new System.Drawing.Point(50, 100), Size = new System.Drawing.Size(150, 30) };
            btnCheckFlights.Click += BtnCheckFlights_Click;

            var btnClose = new Button() { Text = "Закрыть", Location = new System.Drawing.Point(50, 150), Size = new System.Drawing.Size(150, 30) };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { btnTest, btnCheckFlights, btnClose });
        }

        private void TestServices()
        {
            try
            {
                var flights = _flightService.GetAllFlights();
                var airports = _airportService.GetAllAirports();

                MessageBox.Show($"Сервисы работают. Рейсов: {flights.Count}, Аэропортов: {airports.Count}", "Тест сервисов");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сервисов: {ex.Message}", "Ошибка");
            }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            try
            {
                var testFlight = new Flight
                {
                    FlightNumber = "TEST001",
                    FlightType = "Регулярный",
                    Aircraft = "Boeing 737",
                    DepartureDate = DateTime.Now.AddHours(1),
                    ArrivalDate = DateTime.Now.AddHours(3),
                    FlightTime = TimeSpan.FromHours(2),
                    Status = "По расписанию",
                    DepartureAirportId = 1, // Должен существовать в базе
                    ArrivalAirportId = 2,   // Должен существовать в базе
                    EconomySeats = 150,
                    BusinessSeats = 20,
                    Airline = "Test Airlines"
                };

                bool result = _flightService.CreateFlight(testFlight);
                MessageBox.Show(result ? "Тестовый рейс создан!" : "Не удалось создать тестовый рейс", "Результат");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка создания");
            }
        }

        private void BtnCheckFlights_Click(object sender, EventArgs e)
        {
            try
            {
                var flights = _flightService.GetAllFlights();
                string message = $"Всего рейсов: {flights.Count}\n\n";

                foreach (var flight in flights)
                {
                    message += $"{flight.FlightNumber} - {flight.Airline} - {flight.Aircraft}\n";
                }

                MessageBox.Show(message, "Список рейсов");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка проверки");
            }
        }
    }
}