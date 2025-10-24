// DatabaseCreator.cs
using System;
using System.Windows.Forms;
using Npgsql;

namespace UD_WForms
{
    public class DatabaseCreator
    {
        private string connectionString;

        public DatabaseCreator(string host, string username, string password)
        {
            connectionString = $"Host={host};Username={username};Password={password};Timeout=300";
        }

        public bool CreateDatabaseIfNotExists()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем существование базы данных
                    string checkDbQuery = "SELECT 1 FROM pg_database WHERE datname = 'aviation_db'";
                    using (var checkCmd = new NpgsqlCommand(checkDbQuery, connection))
                    {
                        var result = checkCmd.ExecuteScalar();
                        if (result == null)
                        {
                            // Создаем базу данных
                            string createDbQuery = "CREATE DATABASE aviation_db";
                            using (var createCmd = new NpgsqlCommand(createDbQuery, connection))
                            {
                                createCmd.ExecuteNonQuery();
                                MessageBox.Show("База данных aviation_db создана успешно", "Информация",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }

                    // Подключаемся к созданной базе данных
                    string newConnectionString = connectionString + ";Database=aviation_db";
                    using (var dbConnection = new NpgsqlConnection(newConnectionString))
                    {
                        dbConnection.Open();
                        CreateTables(dbConnection);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания базы данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void CreateTables(NpgsqlConnection connection)
        {
            // Таблица аэропортов
            string createAirportsTable = @"
                CREATE TABLE IF NOT EXISTS airports (
                    airport_id SERIAL PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    iata_code VARCHAR(3) UNIQUE NOT NULL,
                    country VARCHAR(50) NOT NULL,
                    city VARCHAR(50) NOT NULL
                )";

            // Таблица пассажиров
            string createPassengersTable = @"
                CREATE TABLE IF NOT EXISTS passengers (
                    passenger_id SERIAL PRIMARY KEY,
                    first_name VARCHAR(50) NOT NULL,
                    last_name VARCHAR(50) NOT NULL,
                    middle_name VARCHAR(50),
                    phone VARCHAR(20),
                    email VARCHAR(100),
                    passport_number VARCHAR(20) UNIQUE NOT NULL,
                    passport_series VARCHAR(10),
                    passport_issued_by VARCHAR(200),
                    passport_issue_date DATE
                )";

            // Таблица рейсов
            string createFlightsTable = @"
                CREATE TABLE IF NOT EXISTS flights (
                    flight_id SERIAL PRIMARY KEY,
                    flight_number VARCHAR(10) NOT NULL,
                    flight_type VARCHAR(20) CHECK (flight_type IN ('domestic', 'international')),
                    aircraft VARCHAR(50) NOT NULL,
                    departure_date TIMESTAMP NOT NULL,
                    arrival_date TIMESTAMP NOT NULL,
                    flight_duration INTERVAL,
                    status VARCHAR(20) DEFAULT 'scheduled',
                    departure_airport_id INTEGER REFERENCES airports(airport_id),
                    arrival_airport_id INTEGER REFERENCES airports(airport_id),
                    seats_economy INTEGER DEFAULT 0,
                    seats_business INTEGER DEFAULT 0,
                    airline VARCHAR(50) NOT NULL
                )";

            // Таблица билетов
            string createTicketsTable = @"
                CREATE TABLE IF NOT EXISTS tickets (
                    ticket_id SERIAL PRIMARY KEY,
                    ticket_number VARCHAR(20) UNIQUE NOT NULL,
                    flight_id INTEGER REFERENCES flights(flight_id),
                    passenger_id INTEGER REFERENCES passengers(passenger_id),
                    class VARCHAR(20) CHECK (class IN ('economy', 'business')),
                    status VARCHAR(20) DEFAULT 'booked',
                    has_luggage BOOLEAN DEFAULT false,
                    price DECIMAL(10,2) NOT NULL,
                    seat_number VARCHAR(10),
                    booking_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";

            ExecuteCommand(connection, createAirportsTable);
            ExecuteCommand(connection, createPassengersTable);
            ExecuteCommand(connection, createFlightsTable);
            ExecuteCommand(connection, createTicketsTable);

            // Вставляем тестовые данные
            InsertTestData(connection);

            MessageBox.Show("Таблицы созданы успешно", "Информация",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExecuteCommand(NpgsqlConnection connection, string commandText)
        {
            using (var command = new NpgsqlCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void InsertTestData(NpgsqlConnection connection)
        {
            // Проверяем, есть ли уже данные
            string checkAirports = "SELECT COUNT(*) FROM airports";
            using (var cmd = new NpgsqlCommand(checkAirports, connection))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0) return; // Данные уже есть
            }

            // Вставляем тестовые аэропорты
            string insertAirports = @"
                INSERT INTO airports (name, iata_code, country, city) VALUES
                ('Шереметьево', 'SVO', 'Россия', 'Москва'),
                ('Пулково', 'LED', 'Россия', 'Санкт-Петербург'),
                ('Сочи', 'AER', 'Россия', 'Сочи'),
                ('Домодедово', 'DME', 'Россия', 'Москва')
                ON CONFLICT (iata_code) DO NOTHING";

            // Вставляем тестовые рейсы
            string insertFlights = @"
                INSERT INTO flights (flight_number, flight_type, aircraft, departure_date, arrival_date, 
                                   flight_duration, status, departure_airport_id, arrival_airport_id, 
                                   seats_economy, seats_business, airline) VALUES
                ('SU1001', 'domestic', 'Boeing 737', '2024-01-20 08:00:00', '2024-01-20 09:30:00', 
                 '1 hour 30 minutes', 'scheduled', 1, 2, 150, 20, 'Аэрофлот'),
                ('SU1002', 'domestic', 'Airbus A320', '2024-01-20 10:00:00', '2024-01-20 13:00:00', 
                 '3 hours', 'scheduled', 1, 3, 140, 16, 'Аэрофлот'),
                ('SU1003', 'domestic', 'Boeing 737', '2024-01-20 11:00:00', '2024-01-20 12:30:00', 
                 '1 hour 30 minutes', 'scheduled', 2, 1, 150, 20, 'Аэрофлот')
                ON CONFLICT DO NOTHING";

            // Вставляем тестовых пассажиров
            string insertPassengers = @"
                INSERT INTO passengers (first_name, last_name, middle_name, phone, email, passport_number) VALUES
                ('Иван', 'Иванов', 'Иванович', '+79161234567', 'ivanov@mail.ru', '1234567890'),
                ('Петр', 'Петров', 'Петрович', '+79169876543', 'petrov@mail.ru', '0987654321')
                ON CONFLICT (passport_number) DO NOTHING";

            ExecuteCommand(connection, insertAirports);
            ExecuteCommand(connection, insertFlights);
            ExecuteCommand(connection, insertPassengers);
        }
    }
}
