using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;

namespace UD_WForms.Forms
{
    public partial class SellTicketForm : Form
    {
        private ITicketService _ticketService;
        private IPassengerService _passengerService;
        private IFlightService _flightService;
        private List<Passenger> _allPassengers;
        private List<Flight> _allFlights;
        private Flight _selectedFlight;
        private decimal _basePrice;

        // Элементы формы
        private ComboBox cmbPassenger;
        private ComboBox cmbFlight;
        private ComboBox cmbClass;
        private NumericUpDown numLuggage;
        private Label lblFlightInfo;
        private Label lblSeatsInfo;
        private Label lblPrice;
        private Button btnCalculate;
        private Button btnSell;
        private Button btnCancel;
        private Button btnRefreshFlights;

        public SellTicketForm()
        {
            InitializeComponent();
            _ticketService = ServiceLocator.GetService<ITicketService>();
            _passengerService = ServiceLocator.GetService<IPassengerService>();
            _flightService = ServiceLocator.GetService<IFlightService>();

            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Продажа нового билета";
            this.Size = new System.Drawing.Size(550, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;

            // Основной контейнер
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);
            mainPanel.AutoScroll = true;

            int top = 10;
            int leftLabel = 10;
            int leftControl = 150;
            int controlWidth = 280;
            int spacing = 35;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = "ПРОДАЖА БИЛЕТА",
                Left = 10,
                Top = top,
                Width = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            top += 50;

            // Пассажир
            var lblPassenger = new Label() { Text = "Пассажир:*", Left = leftLabel, Top = top, Width = 130 };
            cmbPassenger = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "DisplayText"
            };
            top += spacing;

            // Рейс
            var lblFlight = new Label() { Text = "Рейс:*", Left = leftLabel, Top = top, Width = 130 };

            // Панель для рейса с кнопкой обновления
            Panel flightPanel = new Panel();
            flightPanel.Left = leftControl;
            flightPanel.Top = top - 3;
            flightPanel.Width = controlWidth + 40;
            flightPanel.Height = 25;

            cmbFlight = new ComboBox()
            {
                Left = 0,
                Top = 0,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "DisplayText"
            };

            btnRefreshFlights = new Button()
            {
                Text = "🔄",
                Left = controlWidth + 5,
                Top = 0,
                Width = 30,
                Height = 23,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8F)
            };
            btnRefreshFlights.Click += BtnRefreshFlights_Click;

            flightPanel.Controls.Add(cmbFlight);
            flightPanel.Controls.Add(btnRefreshFlights);

            cmbFlight.SelectedIndexChanged += CmbFlight_SelectedIndexChanged;
            top += spacing;

            // Информация о рейсе
            lblFlightInfo = new Label()
            {
                Text = "Выберите рейс для отображения информации",
                Left = leftControl,
                Top = top,
                Width = controlWidth + 40,
                Height = 50,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic),
                ForeColor = System.Drawing.Color.DarkGray
            };
            top += 60;

            // Класс
            var lblClass = new Label() { Text = "Класс:*", Left = leftLabel, Top = top, Width = 130 };
            cmbClass = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbClass.Items.AddRange(new string[] { "Эконом", "Бизнес" });
            cmbClass.SelectedIndex = 0;
            cmbClass.SelectedIndexChanged += CmbClass_SelectedIndexChanged;
            top += spacing;

            // Багаж
            var lblLuggage = new Label() { Text = "Багаж (кг):", Left = leftLabel, Top = top, Width = 130 };
            numLuggage = new NumericUpDown()
            {
                Left = leftControl,
                Top = top,
                Width = 100,
                Minimum = 0,
                Maximum = 50,
                Value = 0,
                DecimalPlaces = 1,
                Increment = 0.5M
            };
            numLuggage.ValueChanged += NumLuggage_ValueChanged;
            top += spacing;

            // Информация о местах
            lblSeatsInfo = new Label()
            {
                Text = "Доступно мест: -",
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular)
            };
            top += spacing;

            // Кнопка расчета
            btnCalculate = new Button()
            {
                Text = "🔄 Рассчитать стоимость",
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                BackColor = System.Drawing.Color.LightBlue
            };
            btnCalculate.Click += BtnCalculate_Click;
            top += 40;

            // Цена
            var lblPriceTitle = new Label() { Text = "Итоговая цена:", Left = leftLabel, Top = top, Width = 130 };
            lblPrice = new Label()
            {
                Text = "0 ₽",
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkGreen
            };
            top += 50;

            // Кнопки
            btnSell = new Button()
            {
                Text = "💰 Продать билет",
                Left = leftControl,
                Top = top,
                Width = 120,
                BackColor = System.Drawing.Color.LightGreen,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
            };
            btnSell.Click += BtnSell_Click;

            btnCancel = new Button()
            {
                Text = "Отмена",
                Left = leftControl + 130,
                Top = top,
                Width = 80
            };
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            mainPanel.Controls.AddRange(new Control[] {
                lblTitle,
                lblPassenger, cmbPassenger,
                lblFlight, flightPanel,
                lblFlightInfo,
                lblClass, cmbClass,
                lblLuggage, numLuggage,
                lblSeatsInfo,
                btnCalculate,
                lblPriceTitle, lblPrice,
                btnSell, btnCancel
            });

            this.Controls.Add(mainPanel);
            this.ResumeLayout(false);
        }

        private void LoadData()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // Загружаем пассажиров
                _allPassengers = _passengerService.GetAllPassengers();
                cmbPassenger.Items.Clear();

                if (_allPassengers.Count == 0)
                {
                    MessageBox.Show("В системе нет пассажиров. Сначала добавьте пассажира.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    foreach (var passenger in _allPassengers)
                    {
                        cmbPassenger.Items.Add(new PassengerComboBoxItem(passenger));
                    }
                    cmbPassenger.SelectedIndex = 0;
                }

                // Загружаем рейсы
                LoadFlights();

                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFlights()
        {
            try
            {
                cmbFlight.Items.Clear();
                _allFlights = _flightService.GetAllFlights();

                // Отладочная информация
                Console.WriteLine($"Всего рейсов в системе: {_allFlights.Count}");

                // Фильтруем рейсы
                var availableFlights = _allFlights
                    .Where(f => f.Status == "По расписанию" || f.Status == "Задержан")
                    .Where(f => f.DepartureDate > DateTime.Now)
                    .Where(f => f.EconomySeats > 0 || f.BusinessSeats > 0)
                    .OrderBy(f => f.DepartureDate)
                    .ToList();

                Console.WriteLine($"Доступных рейсов для продажи: {availableFlights.Count}");

                if (availableFlights.Count == 0)
                {
                    cmbFlight.Items.Add("Нет доступных рейсов");
                    cmbFlight.Enabled = false;
                    lblFlightInfo.Text = "Нет доступных рейсов для продажи билетов.\nВсе рейсы либо уже вылетели, либо нет свободных мест.";
                    lblFlightInfo.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    foreach (var flight in availableFlights)
                    {
                        cmbFlight.Items.Add(new FlightComboBoxItem(flight));
                    }
                    cmbFlight.SelectedIndex = 0;
                    cmbFlight.Enabled = true;
                }

                // Обновляем статистику
                UpdateFlightStatistics(availableFlights);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки рейсов: {ex.Message}");
                cmbFlight.Items.Add("Ошибка загрузки рейсов");
                cmbFlight.Enabled = false;
            }
        }

        private void UpdateFlightStatistics(List<Flight> flights)
        {
            int totalFlights = flights.Count;
            int withEconomySeats = flights.Count(f => f.EconomySeats > 0);
            int withBusinessSeats = flights.Count(f => f.BusinessSeats > 0);

            Console.WriteLine($"Статистика: Всего {totalFlights} рейсов, " +
                             $"с эконом местами: {withEconomySeats}, " +
                             $"с бизнес местами: {withBusinessSeats}");
        }

        private void BtnRefreshFlights_Click(object sender, EventArgs e)
        {
            LoadFlights();
            MessageBox.Show("Список рейсов обновлен", "Обновление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CmbFlight_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFlight.SelectedItem is FlightComboBoxItem flightItem)
            {
                _selectedFlight = flightItem.Flight;
                UpdateFlightInfo();
                UpdateSeatsInfo();
                CalculatePrice();
            }
            else if (cmbFlight.SelectedItem?.ToString() == "Нет доступных рейсов" ||
                     cmbFlight.SelectedItem?.ToString() == "Ошибка загрузки рейсов")
            {
                _selectedFlight = null;
                lblFlightInfo.Text = cmbFlight.SelectedItem?.ToString();
                lblFlightInfo.ForeColor = System.Drawing.Color.Red;
                lblSeatsInfo.Text = "Доступно мест: -";
                lblPrice.Text = "0 ₽";
            }
        }

        private void CmbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSeatsInfo();
            CalculatePrice();
        }

        private void NumLuggage_ValueChanged(object sender, EventArgs e)
        {
            CalculatePrice();
        }

        private void UpdateFlightInfo()
        {
            if (_selectedFlight == null) return;

            string flightInfo = $"✈️ Рейс: {_selectedFlight.FlightNumber}\n" +
                              $"🏢 Авиакомпания: {_selectedFlight.Airline}\n" +
                              $"🛫 Вылет: {_selectedFlight.DepartureDate:dd.MM.yyyy HH:mm}\n" +
                              $"🛬 Прибытие: {_selectedFlight.ArrivalDate:dd.MM.yyyy HH:mm}\n" +
                              $"✈️ Самолет: {_selectedFlight.Aircraft}";

            lblFlightInfo.Text = flightInfo;
            lblFlightInfo.ForeColor = System.Drawing.Color.DarkBlue;
        }

        private void UpdateSeatsInfo()
        {
            if (_selectedFlight == null) return;

            string seatsInfo = "";
            if (cmbClass.SelectedItem?.ToString() == "Эконом")
            {
                seatsInfo = $"✅ Доступно мест эконом: {_selectedFlight.EconomySeats}";
                _basePrice = 5000; // Базовая цена эконом
            }
            else if (cmbClass.SelectedItem?.ToString() == "Бизнес")
            {
                seatsInfo = $"✅ Доступно мест бизнес: {_selectedFlight.BusinessSeats}";
                _basePrice = 15000; // Базовая цена бизнес
            }

            lblSeatsInfo.Text = seatsInfo;
            lblSeatsInfo.ForeColor = System.Drawing.Color.DarkGreen;
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            CalculatePrice();
        }

        private void CalculatePrice()
        {
            if (_selectedFlight == null)
            {
                lblPrice.Text = "0 ₽";
                return;
            }

            decimal price = _basePrice;

            // Наценка за багаж (100 ₽ за кг сверх 10 кг бесплатно)
            decimal freeLuggage = 10;
            decimal luggage = (decimal)numLuggage.Value;
            if (luggage > freeLuggage)
            {
                price += (luggage - freeLuggage) * 100;
            }

            // Наценка за бизнес класс
            if (cmbClass.SelectedItem?.ToString() == "Бизнес")
            {
                price *= 1.5M; // +50% за бизнес
            }

            // Скидка за раннее бронирование (если до вылета более 7 дней)
            TimeSpan timeToDeparture = _selectedFlight.DepartureDate - DateTime.Now;
            if (timeToDeparture.TotalDays > 7)
            {
                price *= 0.9M; // -10% скидка
            }

            lblPrice.Text = $"{price:N2} ₽";
        }

        private void BtnSell_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var passengerItem = cmbPassenger.SelectedItem as PassengerComboBoxItem;
                var flightItem = cmbFlight.SelectedItem as FlightComboBoxItem;

                if (passengerItem == null || flightItem == null)
                {
                    MessageBox.Show("Выберите пассажира и рейс", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Проверяем доступность мест
                if (!CheckSeatAvailability())
                    return;

                // Запрашиваем подтверждение
                var confirmResult = MessageBox.Show(
                    $"Подтвердите продажу билета:\n\n" +
                    $"Пассажир: {passengerItem.Passenger.FullName}\n" +
                    $"Рейс: {flightItem.Flight.FlightNumber}\n" +
                    $"Класс: {cmbClass.SelectedItem}\n" +
                    $"Багаж: {numLuggage.Value} кг\n" +
                    $"Цена: {lblPrice.Text}\n\n" +
                    $"Продолжить продажу?",
                    "Подтверждение продажи",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                    return;

                // Создаем билет
                var ticket = new Ticket
                {
                    TicketNumber = GenerateTicketNumber(),
                    FlightNumber = flightItem.Flight.FlightNumber,
                    PassengerId = passengerItem.Passenger.PassengerId,
                    Class = cmbClass.SelectedItem?.ToString() ?? "Эконом",
                    Status = "Активен",
                    Luggage = (decimal)numLuggage.Value,
                    Price = decimal.Parse(lblPrice.Text.Replace(" ₽", "").Replace(" ", ""))
                };

                // Сохраняем билет
                if (_ticketService.CreateTicket(ticket))
                {
                    // Обновляем количество мест
                    UpdateFlightSeats();

                    // Показываем чек
                    MessageBox.Show(
                        $"✅ БИЛЕТ ПРОДАН УСПЕШНО!\n\n" +
                        $"📋 Номер билета: {ticket.TicketNumber}\n" +
                        $"👤 Пассажир: {passengerItem.Passenger.FullName}\n" +
                        $"✈️ Рейс: {ticket.FlightNumber}\n" +
                        $"🎫 Класс: {ticket.Class}\n" +
                        $"🧳 Багаж: {ticket.Luggage} кг\n" +
                        $"💰 Стоимость: {ticket.Price:N2} ₽\n\n" +
                        $"Билет сохранен в системе.",
                        "Успешная продажа",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить билет в базу данных", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при продаже билета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (cmbPassenger.SelectedItem == null || cmbPassenger.Items.Count == 0)
            {
                MessageBox.Show("В системе нет пассажиров. Сначала добавьте пассажира.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbFlight.SelectedItem == null || !cmbFlight.Enabled)
            {
                MessageBox.Show("Выберите доступный рейс", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbFlight.Focus();
                return false;
            }

            if (cmbClass.SelectedItem == null)
            {
                MessageBox.Show("Выберите класс", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbClass.Focus();
                return false;
            }

            return true;
        }

        private bool CheckSeatAvailability()
        {
            if (_selectedFlight == null) return false;

            string selectedClass = cmbClass.SelectedItem?.ToString();
            int availableSeats = 0;
            string seatType = "";

            if (selectedClass == "Эконом")
            {
                availableSeats = _selectedFlight.EconomySeats;
                seatType = "эконом";
            }
            else if (selectedClass == "Бизнес")
            {
                availableSeats = _selectedFlight.BusinessSeats;
                seatType = "бизнес";
            }

            if (availableSeats <= 0)
            {
                MessageBox.Show($"❌ Нет свободных мест {seatType} класса на выбранный рейс\n\n" +
                               $"Пожалуйста, выберите другой рейс или класс.",
                    "Нет мест", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void UpdateFlightSeats()
        {
            if (_selectedFlight == null) return;

            string selectedClass = cmbClass.SelectedItem?.ToString();

            if (selectedClass == "Эконом")
            {
                _selectedFlight.EconomySeats--;
            }
            else if (selectedClass == "Бизнес")
            {
                _selectedFlight.BusinessSeats--;
            }

            // Обновляем рейс в базе данных
            _flightService.UpdateFlight(_selectedFlight);
        }

        private string GenerateTicketNumber()
        {
            Random rnd = new Random();
            return $"TK{DateTime.Now:yyMMddHHmm}{rnd.Next(10, 99)}";
        }

        // Вспомогательные классы для ComboBox
        private class PassengerComboBoxItem
        {
            public Passenger Passenger { get; set; }
            public string DisplayText => $"{Passenger.FullName} (Паспорт: {Passenger.PassportData})";

            public PassengerComboBoxItem(Passenger passenger)
            {
                Passenger = passenger;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        private class FlightComboBoxItem
        {
            public Flight Flight { get; set; }
            public string DisplayText => $"{Flight.FlightNumber} - {Flight.Airline} - {Flight.DepartureDate:dd.MM.yy HH:mm}";

            public FlightComboBoxItem(Flight flight)
            {
                Flight = flight;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}