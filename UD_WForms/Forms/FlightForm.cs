using System;
using System.Windows.Forms;
using System.Collections.Generic;
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
                Text = _isEditMode ? "Редактирование рейса" : "Добавление нового рейса",
                Left = 10,
                Top = top,
                Width = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.Navy
            };
            top += 40;

            // Номер рейса - с проверкой на null
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

            // Загрузка аэропортов
            LoadAirports(cmbDepartureAirport, cmbArrivalAirport);

            // Дата и время вылета
            var lblDeparture = new Label() { Text = "Вылет:*", Left = leftLabel, Top = top, Width = 130 };
            var dtpDeparture = new DateTimePicker()
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
            var dtpArrival = new DateTimePicker()
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
            var lblCalculatedTime = new Label()
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

            // Обновление времени полета
            void UpdateFlightTime()
            {
                TimeSpan flightTime = dtpArrival.Value - dtpDeparture.Value;
                if (flightTime.TotalMinutes > 0)
                {
                    lblCalculatedTime.Text = flightTime.ToString(@"hh\:mm");
                    lblCalculatedTime.ForeColor = System.Drawing.Color.DarkGreen;
                }
                else
                {
                    lblCalculatedTime.Text = "00:00";
                    lblCalculatedTime.ForeColor = System.Drawing.Color.Red;
                }
            }

            dtpDeparture.ValueChanged += (s, e) => UpdateFlightTime();
            dtpArrival.ValueChanged += (s, e) => UpdateFlightTime();

            // Инициализируем время полета
            UpdateFlightTime();

            // Сохранение
            btnSave.Click += (s, e) =>
            {
                if (ValidateForm(txtFlightNumber, txtAircraft, txtAirline, cmbDepartureAirport, cmbArrivalAirport))
                {
                    SaveFlight(
                        txtFlightNumber.Text,
                        cmbFlightType.SelectedItem?.ToString(),
                        txtAircraft.Text,
                        txtAirline.Text,
                        GetSelectedAirportId(cmbDepartureAirport),
                        GetSelectedAirportId(cmbArrivalAirport),
                        dtpDeparture.Value,
                        dtpArrival.Value,
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
                lblDeparture, dtpDeparture,
                lblArrival, dtpArrival,
                lblFlightTime, lblCalculatedTime,
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
                var airports = _airportService.GetAllAirports();
                foreach (var airport in airports)
                {
                    cmbDeparture.Items.Add($"{airport.IATACode} - {airport.Name} ({airport.City})");
                    cmbArrival.Items.Add($"{airport.IATACode} - {airport.Name} ({airport.City})");
                }

                if (_isEditMode && _flight != null)
                {
                    SelectAirportInComboBox(cmbDeparture, _flight.DepartureAirportId);
                    SelectAirportInComboBox(cmbArrival, _flight.ArrivalAirportId);
                }
                else if (cmbDeparture.Items.Count > 0)
                {
                    cmbDeparture.SelectedIndex = 0;
                    if (cmbArrival.Items.Count > 1)
                        cmbArrival.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки аэропортов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectAirportInComboBox(ComboBox comboBox, int airportId)
        {
            var airports = _airportService.GetAllAirports();
            var airport = airports.FirstOrDefault(a => a.AirportId == airportId);
            if (airport != null)
            {
                string displayText = $"{airport.IATACode} - {airport.Name} ({airport.City})";
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i].ToString() == displayText)
                    {
                        comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private int GetSelectedAirportId(ComboBox comboBox)
        {
            if (comboBox.SelectedItem == null) return 0;

            string selectedText = comboBox.SelectedItem.ToString();
            string iataCode = selectedText.Split('-')[0].Trim();

            var airport = _airportService.GetAirportByIATACode(iataCode);
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

            if (cmbDeparture.SelectedItem == null || cmbArrival.SelectedItem == null)
            {
                MessageBox.Show("Выберите аэропорты вылета и прибытия", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbDeparture.SelectedIndex == cmbArrival.SelectedIndex)
            {
                MessageBox.Show("Аэропорты вылета и прибытия не могут совпадать", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                _flight.FlightType = flightType;
                _flight.Aircraft = aircraft;
                _flight.Airline = airline;
                _flight.DepartureDate = departureDate;
                _flight.ArrivalDate = arrivalDate;
                _flight.FlightTime = arrivalDate - departureDate;
                _flight.Status = status;
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
                    MessageBox.Show("Данные рейса успешно сохранены!", "Успех",
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
                _flight = _flightService.GetFlightByNumber(flightNumber);
                if (_flight == null)
                {
                    MessageBox.Show("Рейс не найден", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
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