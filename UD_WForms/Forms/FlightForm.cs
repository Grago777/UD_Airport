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
        private ComboBox cmbDepartureAirport;
        private ComboBox cmbArrivalAirport;
        private ComboBox cmbStatus;
        private ComboBox cmbFlightType;
        private DateTimePicker dtpDeparture;
        private DateTimePicker dtpArrival;
        private NumericUpDown numEconomySeats;
        private NumericUpDown numBusinessSeats;

        public FlightForm(string flightNumber, IFlightService flightService, IAirportService airportService)
        {
            InitializeComponent();
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

            LoadAirports();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = _isEditMode ? "Редактирование рейса" : "Добавление рейса";
            this.Size = new System.Drawing.Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int leftLabel = 10;
            int leftControl = 150;
            int top = 20;
            int spacing = 40;

            // Номер рейса
            var lblFlightNumber = new Label() { Text = "Номер рейса:*", Left = leftLabel, Top = top, Width = 130 };
            var txtFlightNumber = new TextBox() { Left = leftControl, Top = top, Width = 200, Enabled = !_isEditMode };
            txtFlightNumber.Text = _flight.FlightNumber;

            top += spacing;

            // Тип рейса
            var lblFlightType = new Label() { Text = "Тип рейса:*", Left = leftLabel, Top = top, Width = 130 };
            cmbFlightType = new ComboBox() { Left = leftControl, Top = top, Width = 200 };
            cmbFlightType.Items.AddRange(new string[] { "Регулярный", "Чартерный", "Грузовой" });
            cmbFlightType.SelectedItem = _flight.FlightType;

            top += spacing;

            // Самолет
            var lblAircraft = new Label() { Text = "Самолет:*", Left = leftLabel, Top = top, Width = 130 };
            var txtAircraft = new TextBox() { Left = leftControl, Top = top, Width = 200 };
            txtAircraft.Text = _isEditMode ? _flight.Aircraft : "Boeing 737";

            top += spacing;

            // Авиакомпания
            var lblAirline = new Label() { Text = "Авиакомпания:*", Left = leftLabel, Top = top, Width = 130 };
            var txtAirline = new TextBox() { Left = leftControl, Top = top, Width = 200 };
            txtAirline.Text = _isEditMode ? _flight.Airline : "Аэрофлот";

            top += spacing;

            // Аэропорт вылета
            var lblDepartureAirport = new Label() { Text = "Аэропорт вылета:*", Left = leftLabel, Top = top, Width = 130 };
            cmbDepartureAirport = new ComboBox() { Left = leftControl, Top = top, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

            top += spacing;

            // Аэропорт прибытия
            var lblArrivalAirport = new Label() { Text = "Аэропорт прибытия:*", Left = leftLabel, Top = top, Width = 130 };
            cmbArrivalAirport = new ComboBox() { Left = leftControl, Top = top, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

            top += spacing;

            // Дата и время вылета
            var lblDeparture = new Label() { Text = "Вылет:*", Left = leftLabel, Top = top, Width = 130 };
            dtpDeparture = new DateTimePicker() { Left = leftControl, Top = top, Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };
            dtpDeparture.Value = _flight.DepartureDate;

            top += spacing;

            // Дата и время прибытия
            var lblArrival = new Label() { Text = "Прибытие:*", Left = leftLabel, Top = top, Width = 130 };
            dtpArrival = new DateTimePicker() { Left = leftControl, Top = top, Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };
            dtpArrival.Value = _flight.ArrivalDate;

            top += spacing;

            // Время полета
            var lblFlightTime = new Label() { Text = "Время полета:", Left = leftLabel, Top = top, Width = 130 };
            var lblCalculatedTime = new Label() { Text = "00:00", Left = leftControl, Top = top, Width = 100 };
            lblCalculatedTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);

            // Обновляем время при изменении дат
            dtpDeparture.ValueChanged += (s, e) => UpdateFlightTime(lblCalculatedTime);
            dtpArrival.ValueChanged += (s, e) => UpdateFlightTime(lblCalculatedTime);

            top += spacing;

            // Места эконом класса
            var lblEconomySeats = new Label() { Text = "Места эконом:*", Left = leftLabel, Top = top, Width = 130 };
            numEconomySeats = new NumericUpDown() { Left = leftControl, Top = top, Width = 100, Minimum = 0, Maximum = 1000, Value = _flight.EconomySeats };

            top += spacing;

            // Места бизнес класса
            var lblBusinessSeats = new Label() { Text = "Места бизнес:*", Left = leftLabel, Top = top, Width = 130 };
            numBusinessSeats = new NumericUpDown() { Left = leftControl, Top = top, Width = 100, Minimum = 0, Maximum = 200, Value = _flight.BusinessSeats };

            top += spacing;

            // Статус
            var lblStatus = new Label() { Text = "Статус:*", Left = leftLabel, Top = top, Width = 130 };
            cmbStatus = new ComboBox() { Left = leftControl, Top = top, Width = 200 };
            cmbStatus.Items.AddRange(new string[] { "По расписанию", "Задержан", "Отменен", "Вылетел", "Прибыл" });
            cmbStatus.SelectedItem = _flight.Status;

            top += 50;

            // Кнопки
            var btnSave = new Button() { Text = "Сохранить", Left = leftControl, Top = top, Width = 80 };
            var btnCancel = new Button() { Text = "Отмена", Left = leftControl + 90, Top = top, Width = 80 };

            btnSave.Click += (s, e) =>
            {
                if (ValidateForm(txtFlightNumber, txtAircraft, txtAirline))
                {
                    SaveFlight(txtFlightNumber.Text, txtAircraft.Text, txtAirline.Text);
                }
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new Control[] {
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

            // Инициализируем расчет времени
            UpdateFlightTime(lblCalculatedTime);

            this.ResumeLayout(false);
        }

        private void LoadAirports()
        {
            try
            {
                var airports = _airportService.GetAllAirports();
                foreach (var airport in airports)
                {
                    string displayText = $"{airport.IATACode} - {airport.Name} ({airport.City})";
                    cmbDepartureAirport.Items.Add(new AirportComboBoxItem(displayText, airport.AirportId));
                    cmbArrivalAirport.Items.Add(new AirportComboBoxItem(displayText, airport.AirportId));
                }

                if (_isEditMode && cmbDepartureAirport.Items.Count > 0)
                {
                    SelectAirportInComboBox(cmbDepartureAirport, _flight.DepartureAirportId);
                    SelectAirportInComboBox(cmbArrivalAirport, _flight.ArrivalAirportId);
                }
                else if (cmbDepartureAirport.Items.Count > 0)
                {
                    cmbDepartureAirport.SelectedIndex = 0;
                    if (cmbArrivalAirport.Items.Count > 1)
                        cmbArrivalAirport.SelectedIndex = 1;
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
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is AirportComboBoxItem item && item.AirportId == airportId)
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void UpdateFlightTime(Label label)
        {
            TimeSpan flightTime = dtpArrival.Value - dtpDeparture.Value;
            if (flightTime.TotalMinutes > 0)
            {
                label.Text = $"{flightTime.Hours:00}:{flightTime.Minutes:00}";
                _flight.FlightTime = flightTime;
            }
            else
            {
                label.Text = "00:00";
                label.ForeColor = System.Drawing.Color.Red;
            }
        }

        private bool ValidateForm(TextBox txtFlightNumber, TextBox txtAircraft, TextBox txtAirline)
        {
            if (string.IsNullOrEmpty(txtFlightNumber.Text))
            {
                MessageBox.Show("Введите номер рейса", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(txtAircraft.Text))
            {
                MessageBox.Show("Введите тип самолета", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(txtAirline.Text))
            {
                MessageBox.Show("Введите авиакомпанию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbDepartureAirport.SelectedItem == null || cmbArrivalAirport.SelectedItem == null)
            {
                MessageBox.Show("Выберите аэропорты вылета и прибытия", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbDepartureAirport.SelectedIndex == cmbArrivalAirport.SelectedIndex)
            {
                MessageBox.Show("Аэропорты вылета и прибытия не могут совпадать", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (dtpArrival.Value <= dtpDeparture.Value)
            {
                MessageBox.Show("Время прибытия должно быть позже времени вылета", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void SaveFlight(string flightNumber, string aircraft, string airline)
        {
            try
            {
                _flight.FlightNumber = flightNumber;
                _flight.Aircraft = aircraft;
                _flight.Airline = airline;
                _flight.FlightType = cmbFlightType.SelectedItem?.ToString();
                _flight.DepartureDate = dtpDeparture.Value;
                _flight.ArrivalDate = dtpArrival.Value;
                _flight.Status = cmbStatus.SelectedItem?.ToString();
                _flight.EconomySeats = (int)numEconomySeats.Value;
                _flight.BusinessSeats = (int)numBusinessSeats.Value;

                if (cmbDepartureAirport.SelectedItem is AirportComboBoxItem departureItem)
                    _flight.DepartureAirportId = departureItem.AirportId;

                if (cmbArrivalAirport.SelectedItem is AirportComboBoxItem arrivalItem)
                    _flight.ArrivalAirportId = arrivalItem.AirportId;

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
            return "SU" + DateTime.Now.ToString("MMddHHmm");
        }

        // Вспомогательный класс для ComboBox
        private class AirportComboBoxItem
        {
            public string DisplayText { get; set; }
            public int AirportId { get; set; }

            public AirportComboBoxItem(string displayText, int airportId)
            {
                DisplayText = displayText;
                AirportId = airportId;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}