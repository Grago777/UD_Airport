using Npgsql;
using System;
using System.Windows.Forms;

namespace UD_WForms.Models.Database
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            try
            {
                // Сначала создаем базу данных, если она не существует
                bool databaseCreated = CreateDatabaseIfNotExists();

                // Если база данных была создана, даем время на её инициализацию
                if (databaseCreated)
                {
                    System.Threading.Thread.Sleep(1000); // Небольшая задержка для создания БД
                }

                // Затем создаем таблицы
                CreateTables();

                //MessageBox.Show("База данных успешно инициализирована!", "Успех",
                //MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool CreateDatabaseIfNotExists()
        {
            // Строка подключения к системной базе данных postgres
            string masterConnectionString = ConnectionFactory.GetConnectionString()
                .Replace("Database=aviadb", "Database=postgres");

            using (var connection = new NpgsqlConnection(masterConnectionString))
            {
                connection.Open();

                // Проверяем существование базы данных
                string checkDbQuery = "SELECT 1 FROM pg_database WHERE datname = 'aviadb'";
                using (var checkCmd = new NpgsqlCommand(checkDbQuery, connection))
                {
                    var result = checkCmd.ExecuteScalar();

                    // Если база данных не существует, создаем её
                    if (result == null)
                    {
                        string createDbQuery = "CREATE DATABASE aviadb ENCODING 'UTF8'";
                        using (var createCmd = new NpgsqlCommand(createDbQuery, connection))
                        {
                            createCmd.ExecuteNonQuery();
                            //MessageBox.Show("База данных 'aviadb' создана успешно!", "Информация",
                                //MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true; // База была создана
                        }
                    }
                }
            }
            return false; // База уже существовала
        }

        private static void CreateTables()
        {
            using (var connection = ConnectionFactory.CreateConnection())
            {
                try
                {
                    connection.Open();
                    //MessageBox.Show("Подключение к базе данных aviadb установлено", "Информация",
                        //MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Создание таблицы Аэропорт
                    string createAirportTable = @"
                        CREATE TABLE IF NOT EXISTS Airport (
                            AirportId SERIAL PRIMARY KEY,
                            Name VARCHAR(100) NOT NULL,
                            IATACode VARCHAR(3) UNIQUE NOT NULL,
                            Country VARCHAR(50) NOT NULL,
                            City VARCHAR(50) NOT NULL
                        );";

                    // Создание таблицы Пассажир
                    string createPassengerTable = @"
                        CREATE TABLE IF NOT EXISTS Passenger (
                            PassengerId SERIAL PRIMARY KEY,
                            FullName VARCHAR(100) NOT NULL,
                            PhoneNumber VARCHAR(20),
                            Email VARCHAR(100),
                            PassportData VARCHAR(50) UNIQUE NOT NULL
                        );";

                    // Создание таблицы Рейс
                    string createFlightTable = @"
                        CREATE TABLE IF NOT EXISTS Flight (
                            FlightNumber VARCHAR(10) PRIMARY KEY,
                            FlightType VARCHAR(20) NOT NULL,
                            Aircraft VARCHAR(50) NOT NULL,
                            DepartureDate TIMESTAMP NOT NULL,
                            ArrivalDate TIMESTAMP NOT NULL,
                            FlightTime INTERVAL NOT NULL,
                            Status VARCHAR(20) NOT NULL,
                            DepartureAirportId INTEGER REFERENCES Airport(AirportId),
                            ArrivalAirportId INTEGER REFERENCES Airport(AirportId),
                            EconomySeats INTEGER NOT NULL,
                            BusinessSeats INTEGER NOT NULL,
                            Airline VARCHAR(50) NOT NULL
                        );";
                    string checkFlightTable = @"
                        DO $$
                        BEGIN 
                            IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'flight') THEN
                                CREATE TABLE Flight (
                                    FlightNumber VARCHAR(10) PRIMARY KEY,
                                    FlightType VARCHAR(20) NOT NULL,
                                    Aircraft VARCHAR(50) NOT NULL,
                                    DepartureDate TIMESTAMP NOT NULL,
                                    ArrivalDate TIMESTAMP NOT NULL,
                                    FlightTime INTERVAL NOT NULL,
                                    Status VARCHAR(20) NOT NULL,
                                    DepartureAirportId INTEGER REFERENCES Airport(AirportId),
                                    ArrivalAirportId INTEGER REFERENCES Airport(AirportId),
                                    EconomySeats INTEGER NOT NULL,
                                    BusinessSeats INTEGER NOT NULL,
                                    Airline VARCHAR(50) NOT NULL
                                 );
                            END IF;
                        END $$;";
                     ExecuteNonQuery(connection, checkFlightTable);

                    // Создание таблицы Билет
                    string createTicketTable = @"
                        CREATE TABLE IF NOT EXISTS Ticket (
                            RecordNumber SERIAL PRIMARY KEY,
                            TicketNumber VARCHAR(20) UNIQUE NOT NULL,
                            FlightNumber VARCHAR(10) REFERENCES Flight(FlightNumber),
                            PassengerId INTEGER REFERENCES Passenger(PassengerId),
                            Class VARCHAR(20) NOT NULL,
                            Status VARCHAR(20) NOT NULL,
                            Luggage DECIMAL(5,2) DEFAULT 0,
                            Price DECIMAL(10,2) NOT NULL
                        );";

                    // Выполняем создание таблиц с проверкой
                    ExecuteNonQuery(connection, createAirportTable, "Таблица Airport");
                    ExecuteNonQuery(connection, createPassengerTable, "Таблица Passenger");
                    ExecuteNonQuery(connection, createFlightTable, "Таблица Flight");
                    ExecuteNonQuery(connection, createTicketTable, "Таблица Ticket");

                    // Добавляем тестовые данные, если таблицы пустые
                    InsertTestData(connection);

                    //MessageBox.Show("Все таблицы успешно созданы!", "Успех",
                        //MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании таблиц: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }
        }

        private static void ExecuteNonQuery(NpgsqlConnection connection, string query, string tableName = "")
        {
            try
            {
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        //Console.WriteLine($"{tableName} создана/проверена успешно");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании таблицы {tableName}: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private static void InsertTestData(NpgsqlConnection connection)
        {
            try
            {
                // Проверяем, есть ли данные в аэропортах
                string checkAirports = "SELECT COUNT(*) FROM Airport";
                using (var cmd = new NpgsqlCommand(checkAirports, connection))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        MessageBox.Show("Добавление тестовых данных в аэропорты...", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Добавляем тестовые аэропорты
                        string insertAirports = @"
                            INSERT INTO Airport (Name, IATACode, Country, City) VALUES
                            ('Шереметьево', 'SVO', 'Россия', 'Москва'),
                            ('Пулково', 'LED', 'Россия', 'Санкт-Петербург'),
                            ('Домодедово', 'DME', 'Россия', 'Москва'),
                            ('Сочи', 'AER', 'Россия', 'Сочи');";

                        ExecuteNonQuery(connection, insertAirports);
                    }
                }

                // Проверяем, есть ли данные в пассажирах
                string checkPassengers = "SELECT COUNT(*) FROM Passenger";
                using (var cmd = new NpgsqlCommand(checkPassengers, connection))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        // Добавляем тестовых пассажиров
                        string insertPassengers = @"
                            INSERT INTO Passenger (FullName, PhoneNumber, Email, PassportData) VALUES
                            ('Иванов Иван Иванович', '+79161234567', 'ivanov@mail.ru', '1234 567890'),
                            ('Петров Петр Петрович', '+79167654321', 'petrov@mail.ru', '0987 654321'),
                            ('Сидорова Анна Сергеевна', '+79169998877', 'sidorova@mail.ru', '1122 334455');";

                        ExecuteNonQuery(connection, insertPassengers);
                    }
                }

                // Проверяем, есть ли данные в рейсах
                string checkFlights = "SELECT COUNT(*) FROM Flight";
                using (var cmd = new NpgsqlCommand(checkFlights, connection))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        // Получаем ID аэропортов
                        int svoId = GetAirportIdByCode(connection, "SVO");
                        int ledId = GetAirportIdByCode(connection, "LED");
                        int aerId = GetAirportIdByCode(connection, "AER");

                        if (svoId > 0 && ledId > 0 && aerId > 0)
                        {
                            // Добавляем тестовые рейсы
                            string insertFlights = $@"
                                INSERT INTO Flight (FlightNumber, FlightType, Aircraft, DepartureDate, ArrivalDate, FlightTime, Status, DepartureAirportId, ArrivalAirportId, EconomySeats, BusinessSeats, Airline) VALUES
                                ('SU100', 'Регулярный', 'Boeing 737', '2024-01-20 10:00:00', '2024-01-20 12:00:00', '2 hours', 'По расписанию', {svoId}, {ledId}, 150, 20, 'Аэрофлот'),
                                ('SU200', 'Регулярный', 'Airbus A320', '2024-01-20 14:00:00', '2024-01-20 18:00:00', '4 hours', 'По расписанию', {svoId}, {aerId}, 140, 16, 'Аэрофлот'),
                                ('SU300', 'Регулярный', 'Boeing 777', '2024-01-20 16:00:00', '2024-01-20 17:30:00', '1 hour 30 minutes', 'По расписанию', {ledId}, {svoId}, 200, 30, 'Аэрофлот');";

                            ExecuteNonQuery(connection, insertFlights);
                        }
                    }
                }

                // Проверяем, есть ли данные в билетах
                string checkTickets = "SELECT COUNT(*) FROM Ticket";
                using (var cmd = new NpgsqlCommand(checkTickets, connection))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        // Добавляем тестовые билеты
                        string insertTickets = @"
                            INSERT INTO Ticket (TicketNumber, FlightNumber, PassengerId, Class, Status, Luggage, Price) VALUES
                            ('TK240120001', 'SU100', 1, 'Бизнес', 'Активен', 20.00, 25000.00),
                            ('TK240120002', 'SU100', 2, 'Эконом', 'Активен', 10.00, 8000.00),
                            ('TK240120003', 'SU200', 3, 'Эконом', 'Активен', 15.00, 12000.00),
                            ('TK240120004', 'SU300', 1, 'Бизнес', 'Использован', 25.00, 30000.00),
                            ('TK240120005', 'SU400', 2, 'Эконом', 'Возвращен', 5.00, 7000.00);";

                        ExecuteNonQuery(connection, insertTickets);
                    }
                }

                //MessageBox.Show("Тестовые данные успешно добавлены!", "Успех",
                    //MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении тестовых данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Не прерываем выполнение из-за ошибки тестовых данных
            }
        }

        private static int GetAirportIdByCode(NpgsqlConnection connection, string iataCode)
        {
            try
            {
                string query = "SELECT AirportId FROM Airport WHERE IATACode = @IATACode";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@IATACode", iataCode);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static bool TestDatabaseConnection()
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}