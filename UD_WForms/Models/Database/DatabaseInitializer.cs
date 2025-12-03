using Npgsql;
using System;
using System.Windows.Forms;

namespace UD_WForms.Models.Database
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase(string databaseName = "aviadb2")
        {
            try
            {
                // Сначала создаем базу данных, если она не существует
                bool databaseCreated = CreateDatabaseIfNotExists(databaseName);

                if (databaseCreated)
                {
                    // Даем время на создание БД
                    System.Threading.Thread.Sleep(2000);
                }

                // Затем создаем таблицы
                CreateTables(databaseName);

                MessageBox.Show($"База данных '{databaseName}' успешно инициализирована!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool CreateDatabaseIfNotExists(string databaseName)
        {
            try
            {
                // Подключаемся к системной БД postgres
                using (var connection = new NpgsqlConnection(ConnectionFactory.GetConnectionString("postgres")))
                {
                    connection.Open();

                    // Проверяем существование базы данных
                    string checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
                    using (var checkCmd = new NpgsqlCommand(checkDbQuery, connection))
                    {
                        var result = checkCmd.ExecuteScalar();

                        // Если база данных не существует, создаем её
                        if (result == null)
                        {
                            // Сначала закрываем все существующие подключения к этой БД (если есть)
                            string terminateConnections = $@"
                                SELECT pg_terminate_backend(pg_stat_activity.pid)
                                FROM pg_stat_activity
                                WHERE pg_stat_activity.datname = '{databaseName}'
                                AND pid <> pg_backend_pid();";
                            
                            try
                            {
                                using (var terminateCmd = new NpgsqlCommand(terminateConnections, connection))
                                {
                                    terminateCmd.ExecuteNonQuery();
                                }
                            }
                            catch { /* Игнорируем ошибки при закрытии соединений */ }

                            // Создаем базу данных
                            string createDbQuery = $"CREATE DATABASE \"{databaseName}\" ENCODING 'UTF8'";
                            using (var createCmd = new NpgsqlCommand(createDbQuery, connection))
                            {
                                createCmd.ExecuteNonQuery();
                                MessageBox.Show($"База данных '{databaseName}' создана успешно!", "Информация",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return true; // База была создана
                            }
                        }
                        else
                        {
                            Console.WriteLine($"База данных '{databaseName}' уже существует");
                            return false; // База уже существовала
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private static bool CreateDatabaseIfNotExists()
        {
            // Строка подключения к системной базе данных postgres
            string masterConnectionString = ConnectionFactory.GetConnectionString()
                .Replace($"Database=aviadb", "Database=postgres");

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
                            MessageBox.Show("База данных 'aviadb' создана успешно!", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true; // База была создана
                        }
                    }
                }
            }
            return false; // База уже существовала
        }

        private static void CreateTables(string databaseName)
        {
            using (var connection = ConnectionFactory.CreateConnection(databaseName))
            {
                try
                {
                    connection.Open();

                    // Создание таблицы Аэропорт
                    string createAirportTable = @"
                        DO $$
                        BEGIN
                            IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'airport') THEN
                                CREATE TABLE Airport (
                                    AirportId SERIAL PRIMARY KEY,
                                    Name VARCHAR(100) NOT NULL,
                                    IATACode VARCHAR(3) UNIQUE NOT NULL,
                                    Country VARCHAR(50) NOT NULL,
                                    City VARCHAR(50) NOT NULL
                                );
                            END IF;
                        END $$;";

                    // Создание таблицы Пассажир
                    string createPassengerTable = @"
                        DO $$
                        BEGIN
                            IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'passenger') THEN
                                CREATE TABLE Passenger (
                                    PassengerId SERIAL PRIMARY KEY,
                                    FullName VARCHAR(100) NOT NULL,
                                    PhoneNumber VARCHAR(20),
                                    Email VARCHAR(100),
                                    PassportData VARCHAR(50) UNIQUE NOT NULL
                                );
                            END IF;
                        END $$;";

                    // Создание таблицы Рейс
                    string createFlightTable = @"
                        DO $$
                        BEGIN 
                            IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'flight') THEN
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

                    // Создание таблицы Билет
                    string createTicketTable = @"
                        DO $$
                        BEGIN
                            IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'ticket') THEN
                                CREATE TABLE Ticket (
                                    RecordNumber SERIAL PRIMARY KEY,
                                    TicketNumber VARCHAR(20) UNIQUE NOT NULL,
                                    FlightNumber VARCHAR(10) REFERENCES Flight(FlightNumber),
                                    PassengerId INTEGER REFERENCES Passenger(PassengerId),
                                    Class VARCHAR(20) NOT NULL,
                                    Status VARCHAR(20) NOT NULL,
                                    Luggage DECIMAL(5,2) DEFAULT 0,
                                    Price DECIMAL(10,2) NOT NULL
                                );
                            END IF;
                        END $$;";

                    // Выполняем создание таблиц
                    ExecuteNonQuery(connection, createAirportTable);
                    ExecuteNonQuery(connection, createPassengerTable);
                    ExecuteNonQuery(connection, createFlightTable);
                    ExecuteNonQuery(connection, createTicketTable);

                    // Добавляем тестовые данные
                    InsertTestData(connection);

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
                        Console.WriteLine($"{tableName} создана/проверена успешно");
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
                                ('SU101', 'Регулярный', 'Boeing 737', '{DateTime.Now.AddDays(1):yyyy-MM-dd} 10:00:00', '{DateTime.Now.AddDays(1):yyyy-MM-dd} 12:00:00', '02:00:00', 'По расписанию', {svoId}, {ledId}, 150, 20, 'Аэрофлот'),
                                ('SU102', 'Регулярный', 'Airbus A320', '{DateTime.Now.AddDays(2):yyyy-MM-dd} 14:00:00', '{DateTime.Now.AddDays(2):yyyy-MM-dd} 18:00:00', '04:00:00', 'По расписанию', {svoId}, {aerId}, 140, 16, 'Аэрофлот'),
                                ('SU103', 'Регулярный', 'Boeing 777', '{DateTime.Now.AddDays(3):yyyy-MM-dd} 16:00:00', '{DateTime.Now.AddDays(3):yyyy-MM-dd} 17:30:00', '01:30:00', 'По расписанию', {ledId}, {svoId}, 200, 30, 'Аэрофлот'),
                                ('SU105', 'Регулярный', 'Airbus A330', '{DateTime.Now.AddDays(4):yyyy-MM-dd} 20:00:00', '{DateTime.Now.AddDays(5):yyyy-MM-dd} 22:00:00', '02:00:00', 'По расписанию', {aerId}, {svoId}, 160, 24, 'Аэрофлот');";

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
                        // Сначала проверяем, что рейсы существуют
                        string checkFlightNumbers = @"
                    SELECT COUNT(*) FROM Flight 
                    WHERE FlightNumber IN ('SU100', 'SU200', 'SU300', 'SU400')";

                        using (var checkFlightCmd = new NpgsqlCommand(checkFlightNumbers, connection))
                        {
                            long flightCount = (long)checkFlightCmd.ExecuteScalar();

                            if (flightCount < 4)
                            {
                                MessageBox.Show($"Найдено только {flightCount} из 4 требуемых рейсов. Тестовые билеты не будут добавлены.",
                                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        // Добавляем тестовые билеты с корректными номерами рейсов
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

        public static bool TestDatabaseConnection(string databaseName)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection(databaseName))
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