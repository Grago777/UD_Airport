using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;

namespace UD_WForms.Forms
{
    public partial class TicketEditForm : Form
    {
        private ITicketService _ticketService;
        private IPassengerService _passengerService;
        private IFlightService _flightService;
        private Ticket _ticket;
        private bool _isEditMode;

        public TicketEditForm(int recordNumber, ITicketService ticketService,
                            IPassengerService passengerService, IFlightService flightService)
        {
            _ticketService = ticketService;
            _passengerService = passengerService;
            _flightService = flightService;
            _isEditMode = recordNumber > 0;

            if (_isEditMode)
            {
                LoadTicket(recordNumber);
            }
            else
            {
                _ticket = new Ticket();
            }

            InitializeForm();
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            this.Text = _isEditMode ? $"Редактирование билета #{_ticket.RecordNumber}" : "Редактирование билета";
            this.Size = new System.Drawing.Size(500, 500);
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

            int top = 20;
            int leftLabel = 20;
            int leftControl = 150;
            int controlWidth = 250;
            int spacing = 35;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = _isEditMode ? "РЕДАКТИРОВАНИЕ БИЛЕТА" : "ИНФОРМАЦИЯ О БИЛЕТЕ",
                Left = 20,
                Top = top,
                Width = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            top += 50;

            // Номер билета
            var lblTicketNumber = new Label() { Text = "Номер билета:*", Left = leftLabel, Top = top, Width = 120 };
            var txtTicketNumber = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _ticket.TicketNumber ?? GenerateTicketNumber(),
                MaxLength = 20,
                ReadOnly = _isEditMode // Нельзя менять номер существующего билета
            };
            top += spacing;

            // Рейс
            var lblFlight = new Label() { Text = "Рейс:*", Left = leftLabel, Top = top, Width = 120 };
            var cmbFlight = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Загружаем рейсы
            var flights = _flightService.GetAllFlights();
            cmbFlight.Items.Add("-- Выберите рейс --");
            foreach (var flight in flights)
            {
                cmbFlight.Items.Add($"{flight.FlightNumber} - {flight.Airline}");
            }
            if (_isEditMode)
            {
                cmbFlight.SelectedIndex = flights.FindIndex(f => f.FlightNumber == _ticket.FlightNumber) + 1;
            }
            else
            {
                cmbFlight.SelectedIndex = 0;
            }
            top += spacing;

            // Пассажир
            var lblPassenger = new Label() { Text = "Пассажир:*", Left = leftLabel, Top = top, Width = 120 };
            var cmbPassenger = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Загружаем пассажиров
            var passengers = _passengerService.GetAllPassengers();
            cmbPassenger.Items.Add("-- Выберите пассажира --");
            foreach (var passenger in passengers)
            {
                cmbPassenger.Items.Add($"{passenger.PassengerId}: {passenger.FullName}");
            }
            if (_isEditMode)
            {
                cmbPassenger.SelectedIndex = passengers.FindIndex(p => p.PassengerId == _ticket.PassengerId) + 1;
            }
            else
            {
                cmbPassenger.SelectedIndex = 0;
            }
            top += spacing;

            // Класс
            var lblClass = new Label() { Text = "Класс:*", Left = leftLabel, Top = top, Width = 120 };
            var cmbClass = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbClass.Items.AddRange(new string[] { "Эконом", "Бизнес", "Первый" });
            cmbClass.SelectedItem = _ticket.Class ?? "Эконом";
            top += spacing;

            // Статус
            var lblStatus = new Label() { Text = "Статус:*", Left = leftLabel, Top = top, Width = 120 };
            var cmbStatus = new ComboBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new string[] { "Активен", "Использован", "Возвращен", "Отменен" });
            cmbStatus.SelectedItem = _ticket.Status ?? "Активен";
            top += spacing;

            // Багаж
            var lblLuggage = new Label() { Text = "Багаж (кг):", Left = leftLabel, Top = top, Width = 120 };
            var numLuggage = new NumericUpDown()
            {
                Left = leftControl,
                Top = top,
                Width = 100,
                Minimum = 0,
                Maximum = 50,
                Value = _ticket.Luggage,
                DecimalPlaces = 1,
                Increment = 0.5M
            };
            top += spacing;

            // Цена
            var lblPrice = new Label() { Text = "Цена (₽):*", Left = leftLabel, Top = top, Width = 120 };
            var numPrice = new NumericUpDown()
            {
                Left = leftControl,
                Top = top,
                Width = 120,
                Minimum = 0,
                Maximum = 1000000,
                Value = _ticket.Price,
                DecimalPlaces = 2
            };
            top += 50;

            // Кнопки
            var btnSave = new Button()
            {
                Text = _isEditMode ? "💾 Сохранить изменения" : "💾 Сохранить",
                Left = leftControl,
                Top = top,
                Width = 140,
                BackColor = System.Drawing.Color.LightGreen,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
            };
            btnSave.Click += (s, e) =>
            {
                if (ValidateForm(txtTicketNumber, cmbFlight, cmbPassenger, cmbClass, cmbStatus, numPrice))
                {
                    SaveTicket(
                        txtTicketNumber.Text,
                        cmbFlight.SelectedItem?.ToString()?.Split('-')[0]?.Trim(),
                        cmbPassenger.SelectedItem?.ToString()?.Split(':')[0]?.Trim(),
                        cmbClass.SelectedItem?.ToString(),
                        cmbStatus.SelectedItem?.ToString(),
                        (decimal)numLuggage.Value,
                        numPrice.Value
                    );
                }
            };

            var btnCancel = new Button()
            {
                Text = "Отмена",
                Left = leftControl + 150,
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
                lblTicketNumber, txtTicketNumber,
                lblFlight, cmbFlight,
                lblPassenger, cmbPassenger,
                lblClass, cmbClass,
                lblStatus, cmbStatus,
                lblLuggage, numLuggage,
                lblPrice, numPrice,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
            this.ResumeLayout(false);
        }

        private void LoadTicket(int recordNumber)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _ticket = _ticketService.GetTicketById(recordNumber);

                if (_ticket == null)
                {
                    MessageBox.Show("Билет не найден", "Ошибка",
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

        private bool ValidateForm(TextBox txtTicketNumber, ComboBox cmbFlight, ComboBox cmbPassenger,
                                ComboBox cmbClass, ComboBox cmbStatus, NumericUpDown numPrice)
        {
            // Проверка номера билета
            if (string.IsNullOrWhiteSpace(txtTicketNumber.Text))
            {
                MessageBox.Show("Введите номер билета", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTicketNumber.Focus();
                return false;
            }

            // Проверка рейса
            if (cmbFlight.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите рейс", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbFlight.Focus();
                return false;
            }

            // Проверка пассажира
            if (cmbPassenger.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите пассажира", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbPassenger.Focus();
                return false;
            }

            // Проверка класса
            if (cmbClass.SelectedItem == null)
            {
                MessageBox.Show("Выберите класс", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbClass.Focus();
                return false;
            }

            // Проверка статуса
            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbStatus.Focus();
                return false;
            }

            // Проверка цены
            if (numPrice.Value <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPrice.Focus();
                return false;
            }

            return true;
        }

        private void SaveTicket(string ticketNumber, string flightNumber, string passengerIdStr,
                              string ticketClass, string status, decimal luggage, decimal price)
        {
            try
            {
                // Парсим ID пассажира
                if (!int.TryParse(passengerIdStr, out int passengerId))
                {
                    MessageBox.Show("Неверный формат ID пассажира", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Обновляем данные билета
                _ticket.TicketNumber = ticketNumber.Trim();
                _ticket.FlightNumber = flightNumber;
                _ticket.PassengerId = passengerId;
                _ticket.Class = ticketClass;
                _ticket.Status = status;
                _ticket.Luggage = luggage;
                _ticket.Price = price;

                bool success;

                if (_isEditMode)
                {
                    success = _ticketService.UpdateTicket(_ticket);
                }
                else
                {
                    success = _ticketService.CreateTicket(_ticket);
                }

                if (success)
                {
                    string message = _isEditMode ?
                        "Данные билета успешно обновлены!" :
                        "Новый билет успешно создан!";

                    MessageBox.Show(message, "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить данные", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateTicketNumber()
        {
            Random rnd = new Random();
            return $"TK{DateTime.Now:yyMMddHHmm}{rnd.Next(10, 99)}";
        }
    }
}