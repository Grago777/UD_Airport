using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;

namespace UD_WForms.Forms
{
    public partial class FlightForm : Form
    {
        private IFlightService _flightService;
        private IAirportService _airportService;
        private Flight _flight;
        private bool _isEditMode;
        private List<Airport> _allAirports = new List<Airport>();

        // Объявляем контролы как поля класса, чтобы они были доступны во всех методах
        private DateTimePicker _dtpDeparture;
        private DateTimePicker _dtpArrival;
        private Label _lblCalculatedTime;

        public FlightForm(string flightNumber, IFlightService flightService, IAirportService airportService)
        {
            _flightService = flightService;
            _airportService = airportService;
            _isEditMode = !string.IsNullOrEmpty(flightNumber);

            if (_isEditMode)
            {
                LoadFlight(flightNumber);
            }
            else
            {
                _flight = new Flight
                {
                    FlightNumber = GenerateFlightNumber(),
                    DepartureDate = DateTime.Now.AddHours(1),
                    ArrivalDate = DateTime.Now.AddHours(3),
                    FlightTime = TimeSpan.FromHours(2),
                    Status = "По расписанию",
                    FlightType = "Регулярный",
                    EconomySeats = 150,
                    BusinessSeats = 20,
                    Airline = "Аэрофлот"
                };
            }

            InitializeForm();
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            this.Text = _isEditMode ? $"Редактирование рейса {_flight?.FlightNumber}" : "Добавление нового рейса";
            this.Size = new System.Drawing.Size(550, 600);
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
            int controlWidth = 250;
            int spacing = 35;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = _isEditMode ? "РЕДАКТИРОВАНИЕ РЕЙСА" : "НОВЫЙ РЕЙС",
                Left = 20,
                Top = top,
                Width = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            top += 50;

            // Номер рейса
            var lblFlightNumber = new Label() { Text = "Номер рейса:*", Left = leftLabel, Top = top, Width = 130 };
            var txtFlightNumber = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _flight?.FlightNumber ?? GenerateFlightNumber(),
                Enabled = !_isEditMode,
                BackColor = _isEditMode ? System.Drawing.Color.LightGray : System.Drawing.Color.White
            };
            top += spacing;

            // Тип рейса
            var lblFlightType = new Label() { Text = "Тип рейса:*", Left = leftLabel, Top = top, Width = 130 };
            var cmbFlightType = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFlightType.Items.AddRange(new string[] { "Регулярный", "Чартерный", "Грузовой" });
            cmbFlightType.SelectedItem = _flight?.FlightType ?? "Регулярный";
            top += spacing;

            // Самолет
            var lblAircraft = new Label() { Text = "Самолет:*", Left = leftLabel, Top = top, Width = 130 };
            var txtAircraft = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _flight?.Aircraft ?? "Boeing 737"
            };
            top += spacing;

            // Авиакомпания
            var lblAirline = new Label() { Text = "Авиакомпания:*", Left = leftLabel, Top = top, Width = 130 };
            var txtAirline = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _flight?.Airline ?? "Аэрофлот"
            };
            top += spacing;

            // Аэропорт вылета
            var lblDepartureAirport = new Label() { Text = "Аэропорт вылета:*", Left = leftLabel, Top = top, Width = 130 };
            var cmbDepartureAirport = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            top += spacing;

            // Аэропорт прибытия
            var lblArrivalAirport = new Label() { Text = "Аэропорт прибытия:*", Left = leftLabel, Top = top, Width = 130 };
            var cmbArrivalAirport = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            top += spacing;

            // Дата и время вылета
            var lblDeparture = new Label() { Text = "Вылет:*", Left = leftLabel, Top = top, Width = 130 };
            _dtpDeparture = new DateTimePicker()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm",
                Value = _flight?.DepartureDate ?? DateTime.Now.AddHours(1),
                ShowUpDown = true
            };
            top += spacing;

            // Дата и время прибытия
            var lblArrival = new Label() { Text = "Прибытие:*", Left = leftLabel, Top = top, Width = 130 };
            _dtpArrival = new DateTimePicker()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm",
                Value = _flight?.ArrivalDate ?? DateTime.Now.AddHours(3),
                ShowUpDown = true
            };
            top += spacing;

            // Время полета
            var lblFlightTime = new Label() { Text = "Время полета:", Left = leftLabel, Top = top, Width = 130 };
            _lblCalculatedTime = new Label()
            {
                Text = _flight?.FlightTime.ToString(@"hh\:mm") ?? "02:00",
                Left = leftControl,
                Top = top,
                Width = 100,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue
            };
            top += spacing;

            // Места эконом класса
            var lblEconomySeats = new Label() { Text = "Места эконом:*", Left = leftLabel, Top = top, Width = 130 };
            var numEconomySeats = new NumericUpDown()
            {
                Left = leftControl,
                Top = top,
                Width = 100,
                Minimum = 0,
                Maximum = 1000,
                Value = _flight?.EconomySeats ?? 150
            };
            top += spacing;

            // Места бизнес класса
            var lblBusinessSeats = new Label() { Text = "Места бизнес:*", Left = leftLabel, Top = top, Width = 130 };
            var numBusinessSeats = new NumericUpDown()
            {
                Left = leftControl,
                Top = top,
                Width = 100,
                Minimum = 0,
                Maximum = 200,
                Value = _flight?.BusinessSeats ?? 20
            };
            top += spacing;

            // Статус
            var lblStatus = new Label() { Text = "Статус:*", Left = leftLabel, Top = top, Width = 130 };
            var cmbStatus = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new string[] { "По расписанию", "Задержан", "Отменен", "Вылетел", "Прибыл" });
            cmbStatus.SelectedItem = _flight?.Status ?? "По расписанию";
            top += 50;

            // Кнопки
            var btnSave = new Button()
            {
                Text = "💾 Сохранить",
                Left = leftControl,
                Top = top,
                Width = 100,
                BackColor = System.Drawing.Color.LightGreen
            };
            var btnCancel = new Button()
            {
                Text = "Отмена",
                Left = leftControl + 110,
                Top = top,
                Width = 80
            };

            // Загрузка аэропортов
            LoadAirports(cmbDepartureAirport, cmbArrivalAirport);

            // Обновление времени полета
            void UpdateFlightTime()
            {
                TimeSpan flightTime = _dtpArrival.Value - _dtpDeparture.Value;
                if (flightTime.TotalMinutes > 0)
                {
                    _lblCalculatedTime.Text = flightTime.ToString(@"hh\:mm");
                    _lblCalculatedTime.ForeColor = System.Drawing.Color.DarkGreen;
                }
                else
                {
                    _lblCalculatedTime.Text = "00:00";
                    _lblCalculatedTime.ForeColor = System.Drawing.Color.Red;
                }
            }

            _dtpDeparture.ValueChanged += (s, e) => UpdateFlightTime();
            _dtpArrival.ValueChanged += (s, e) => UpdateFlightTime();

            // Инициализируем время полета
            UpdateFlightTime();

            // Сохранение
            btnSave.Click += (s, e) =>
            {
                if (ValidateForm(txtFlightNumber, txtAircraft, txtAirline, cmbDepartureAirport, cmbArrivalAirport))
                {
                    // Получаем ID выбранных аэропортов
                    int departureAirportId = GetSelectedAirportId(cmbDepartureAirport);
                    int arrivalAirportId = GetSelectedAirportId(cmbArrivalAirport);

                    if (departureAirportId == 0 || arrivalAirportId == 0)
                    {
                        MessageBox.Show("Не удалось определить выбранные аэропорты", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    SaveFlight(
                        txtFlightNumber.Text,
                        cmbFlightType.SelectedItem?.ToString(),
                        txtAircraft.Text,
                        txtAirline.Text,
                        departureAirportId,
                        arrivalAirportId,
                        _dtpDeparture.Value,
                        _dtpArrival.Value,
                        cmbStatus.SelectedItem?.ToString(),
                        (int)numEconomySeats.Value,
                        (int)numBusinessSeats.Value
                    );
                }
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            mainPanel.Controls.AddRange(new Control[] {
                lblTitle,
                lblFlightNumber, txtFlightNumber,
                lblFlightType, cmbFlightType,
                lblAircraft, txtAircraft,
                lblAirline, txtAirline,
                lblDepartureAirport, cmbDepartureAirport,
                lblArrivalAirport, cmbArrivalAirport,
                lblDeparture, _dtpDeparture,
                lblArrival, _dtpArrival,
                lblFlightTime, _lblCalculatedTime,
                lblEconomySeats, numEconomySeats,
                lblBusinessSeats, numBusinessSeats,
                lblStatus, cmbStatus,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
            this.ResumeLayout(false);
        }

        private void LoadAirports(ComboBox cmbDeparture, ComboBox cmbArrival)
        {
            try
            {
                _allAirports = _airportService.GetAllAirports();

                // Очищаем и заполняем ComboBox
                cmbDeparture.Items.Clear();
                cmbArrival.Items.Clear();

                cmbDeparture.Items.Add("-- Выберите аэропорт --");
                cmbArrival.Items.Add("-- Выберите аэропорт --");

                foreach (var airport in _allAirports)
                {
                    string displayText = $"{airport.IATACode} - {airport.Name} ({airport.City}, {airport.Country})";
                    cmbDeparture.Items.Add(displayText);
                    cmbArrival.Items.Add(displayText);
                }

                // Устанавливаем выбранные значения для редактирования
                if (_isEditMode && _flight != null)
                {
                    SetSelectedAirport(cmbDeparture, _flight.DepartureAirportId);
                    SetSelectedAirport(cmbArrival, _flight.ArrivalAirportId);
                }
                else
                {
                    cmbDeparture.SelectedIndex = 0;
                    cmbArrival.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки аэропортов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetSelectedAirport(ComboBox comboBox, int airportId)
        {
            var airport = _allAirports.FirstOrDefault(a => a.AirportId == airportId);
            if (airport != null)
            {
                string searchText = $"{airport.IATACode} - {airport.Name} ({airport.City}, {airport.Country})";

                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i].ToString() == searchText)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }
                }
            }

            // Если не нашли, выбираем первый элемент
            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private int GetSelectedAirportId(ComboBox comboBox)
        {
            if (comboBox.SelectedIndex <= 0 || comboBox.SelectedItem == null)
                return 0;

            string selectedText = comboBox.SelectedItem.ToString();

            // Извлекаем IATA код (первые 3 символа перед пробелом)
            string iataCode = selectedText.Substring(0, 3).Trim();

            // Находим аэропорт по IATA коду
            var airport = _allAirports.FirstOrDefault(a => a.IATACode == iataCode);
            return airport?.AirportId ?? 0;
        }

        private bool ValidateForm(TextBox txtFlightNumber, TextBox txtAircraft, TextBox txtAirline,
                                ComboBox cmbDeparture, ComboBox cmbArrival)
        {
            if (string.IsNullOrWhiteSpace(txtFlightNumber.Text))
            {
                MessageBox.Show("Введите номер рейса", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFlightNumber.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAircraft.Text))
            {
                MessageBox.Show("Введите тип самолета", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAircraft.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAirline.Text))
            {
                MessageBox.Show("Введите авиакомпанию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAirline.Focus();
                return false;
            }

            if (cmbDeparture.SelectedIndex == 0 || cmbArrival.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите аэропорты вылета и прибытия", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbDeparture.SelectedIndex == cmbArrival.SelectedIndex)
            {
                MessageBox.Show("Аэропорты вылета и прибытия не могут совпадать", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверяем, что дата прибытия позже даты вылета
            if (_dtpDeparture.Value >= _dtpArrival.Value)
            {
                MessageBox.Show("Дата прибытия должна быть позже даты вылета", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dtpArrival.Focus();
                return false;
            }

            return true;
        }

        private void SaveFlight(string flightNumber, string flightType, string aircraft, string airline,
                              int departureAirportId, int arrivalAirportId,
                              DateTime departureDate, DateTime arrivalDate, string status,
                              int economySeats, int businessSeats)
        {
            try
            {
                // Создаем или обновляем объект Flight
                if (_flight == null)
                {
                    _flight = new Flight();
                }

                _flight.FlightNumber = flightNumber;
                _flight.FlightType = flightType ?? "Регулярный";
                _flight.Aircraft = aircraft;
                _flight.Airline = airline;
                _flight.DepartureDate = departureDate;
                _flight.ArrivalDate = arrivalDate;
                _flight.FlightTime = arrivalDate - departureDate;
                _flight.Status = status ?? "По расписанию";
                _flight.EconomySeats = economySeats;
                _flight.BusinessSeats = businessSeats;
                _flight.DepartureAirportId = departureAirportId;
                _flight.ArrivalAirportId = arrivalAirportId;

                bool success;
                if (_isEditMode)
                {
                    success = _flightService.UpdateFlight(_flight);
                }
                else
                {
                    success = _flightService.CreateFlight(_flight);
                }

                if (success)
                {
                    string message = _isEditMode ?
                        "Данные рейса успешно обновлены!" :
                        "Новый рейс успешно добавлен!";

                    MessageBox.Show(message, "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить данные рейса", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFlight(string flightNumber)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _flight = _flightService.GetFlightByNumber(flightNumber);

                if (_flight == null)
                {
                    MessageBox.Show("Рейс не найден", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }

                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private string GenerateFlightNumber()
        {
            Random rnd = new Random();
            string[] airlines = { "SU", "S7", "U6", "FV", "DP" };
            string airline = airlines[rnd.Next(airlines.Length)];
            return $"{airline}{rnd.Next(1000, 9999)}";
        }
    }
}