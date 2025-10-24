// MainForm.cs
using System;
using System.Data;
using System.Windows.Forms;

namespace UD_WForms
{
    public partial class MainForm : Form
    {
        private DatabaseHelper db;

        public MainForm()
        {
            InitializeComponent();

            // Настройка подключения - ЗАМЕНИТЕ НА СВОИ ДАННЫЕ
            db = new DatabaseHelper("localhost", "aviation_db", "postgres", "Frida");

            LoadFlights();
        }

        private void LoadFlights()
        {
            string query = @"
                SELECT 
                    f.flight_id as ""ID"",
                    f.flight_number as ""Номер рейса"",
                    f.airline as ""Авиакомпания"",
                    dep.name as ""Аэропорт вылета"",
                    arr.name as ""Аэропорт прилета"",
                    TO_CHAR(f.departure_date, 'DD.MM.YYYY HH24:MI') as ""Вылет"",
                    TO_CHAR(f.arrival_date, 'DD.MM.YYYY HH24:MI') as ""Прилет"",
                    f.flight_duration as ""Время в пути"",
                    f.status as ""Статус"",
                    (f.seats_economy + f.seats_business) as ""Всего мест""
                FROM flights f
                JOIN airports dep ON f.departure_airport_id = dep.airport_id
                JOIN airports arr ON f.arrival_airport_id = arr.airport_id
                ORDER BY f.departure_date";

            DataTable flights = db.ExecuteQuery(query);
            dataGridViewFlights.DataSource = flights;

            if (dataGridViewFlights.Columns.Count > 0)
            {
                dataGridViewFlights.Columns["ID"].Visible = false;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string departure = txtDepartureCity.Text.Trim();
            string arrival = txtArrivalCity.Text.Trim();
            string date = dateTimePickerDeparture.Value.ToString("yyyy-MM-dd");

            string query = $@"
                SELECT 
                    f.flight_id as ""ID"",
                    f.flight_number as ""Номер рейса"",
                    f.airline as ""Авиакомпания"",
                    dep.name as ""Аэропорт вылета"",
                    arr.name as ""Аэропорт прилета"",
                    TO_CHAR(f.departure_date, 'DD.MM.YYYY HH24:MI') as ""Вылет"",
                    TO_CHAR(f.arrival_date, 'DD.MM.YYYY HH24:MI') as ""Прилет"",
                    f.flight_duration as ""Время в пути"",
                    f.status as ""Статус"",
                    (f.seats_economy + f.seats_business) as ""Всего мест""
                FROM flights f
                JOIN airports dep ON f.departure_airport_id = dep.airport_id
                JOIN airports arr ON f.arrival_airport_id = arr.airport_id
                WHERE dep.city ILIKE '%{departure}%' 
                AND arr.city ILIKE '%{arrival}%' 
                AND DATE(f.departure_date) = '{date}'
                ORDER BY f.departure_date";

            DataTable flights = db.ExecuteQuery(query);
            dataGridViewFlights.DataSource = flights;

            if (dataGridViewFlights.Columns.Count > 0)
            {
                dataGridViewFlights.Columns["ID"].Visible = false;
            }
        }

        private void btnBookTicket_Click(object sender, EventArgs e)
        {
            if (dataGridViewFlights.CurrentRow != null && dataGridViewFlights.CurrentRow.Cells["ID"].Value != null)
            {
                int flightId = Convert.ToInt32(dataGridViewFlights.CurrentRow.Cells["ID"].Value);
                // Здесь будет вызов формы бронирования
                MessageBox.Show($"Бронирование рейса ID: {flightId}", "Бронирование",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Выберите рейс для бронирования", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnShowTickets_Click(object sender, EventArgs e)
        {
            // Здесь будет вызов формы просмотра билетов
            MessageBox.Show("Форма просмотра билетов", "Билеты",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadFlights();
        }
    }
}