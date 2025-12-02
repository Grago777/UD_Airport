using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;
using System.Text.RegularExpressions;

namespace UD_WForms.Forms
{
    public partial class PassengerForm : Form
    {
        private IPassengerService _passengerService;
        private Passenger _passenger;
        private bool _isEditMode;

        public PassengerForm(int passengerId, IPassengerService passengerService)
        {
            _passengerService = passengerService;
            _isEditMode = passengerId > 0;

            if (_isEditMode)
            {
                LoadPassenger(passengerId);
            }
            else
            {
                _passenger = new Passenger();
            }

            InitializeForm();
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            this.Text = _isEditMode ? $"Редактирование пассажира #{_passenger.PassengerId}" : "Добавление нового пассажира";
            this.Size = new System.Drawing.Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;

            // Основной контейнер
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            int top = 20;
            int leftLabel = 20;
            int leftControl = 150;
            int controlWidth = 250;
            int spacing = 40;

            // Заголовок
            var lblTitle = new Label()
            {
                Text = _isEditMode ? "РЕДАКТИРОВАНИЕ ПАССАЖИРА" : "НОВЫЙ ПАССАЖИР",
                Left = 20,
                Top = top,
                Width = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            top += 50;

            // ФИО
            var lblFullName = new Label() { Text = "ФИО:*", Left = leftLabel, Top = top, Width = 120 };
            var txtFullName = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _passenger.FullName ?? "",
                MaxLength = 100
            };
            top += spacing;

            // Телефон
            var lblPhone = new Label() { Text = "Телефон:", Left = leftLabel, Top = top, Width = 120 };
            var txtPhone = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _passenger.PhoneNumber ?? "",
                MaxLength = 20,
                PlaceholderText = "+7 (XXX) XXX-XX-XX"
            };
            top += spacing;

            // Email
            var lblEmail = new Label() { Text = "Email:", Left = leftLabel, Top = top, Width = 120 };
            var txtEmail = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _passenger.Email ?? "",
                MaxLength = 100,
                PlaceholderText = "example@mail.ru"
            };
            top += spacing;

            // Паспортные данные
            var lblPassport = new Label() { Text = "Паспорт:*", Left = leftLabel, Top = top, Width = 120 };
            var txtPassport = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _passenger.PassportData ?? "",
                MaxLength = 50,
                PlaceholderText = "XXXX XXXXXX"
            };
            top += 60;

            // Кнопки
            var btnSave = new Button()
            {
                Text = _isEditMode ? "💾 Сохранить изменения" : "➕ Добавить пассажира",
                Left = leftControl,
                Top = top,
                Width = 150,
                BackColor = System.Drawing.Color.LightGreen,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
            };
            btnSave.Click += (s, e) =>
            {
                if (ValidateForm(txtFullName, txtPhone, txtEmail, txtPassport))
                {
                    SavePassenger(txtFullName.Text, txtPhone.Text, txtEmail.Text, txtPassport.Text);
                }
            };

            var btnCancel = new Button()
            {
                Text = "Отмена",
                Left = leftControl + 160,
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
                lblFullName, txtFullName,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblPassport, txtPassport,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
            this.ResumeLayout(false);
        }

        private void LoadPassenger(int passengerId)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _passenger = _passengerService.GetPassengerById(passengerId);

                if (_passenger == null)
                {
                    MessageBox.Show("Пассажир не найден", "Ошибка",
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

        private bool ValidateForm(TextBox txtFullName, TextBox txtPhone, TextBox txtEmail, TextBox txtPassport)
        {
            // Проверка ФИО
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО пассажира", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            if (txtFullName.Text.Length < 5)
            {
                MessageBox.Show("ФИО должно содержать не менее 5 символов", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            // Проверка паспортных данных
            if (string.IsNullOrWhiteSpace(txtPassport.Text))
            {
                MessageBox.Show("Введите паспортные данные", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassport.Focus();
                return false;
            }

            // Проверка формата паспорта (пример: 1234 567890)
            var passportRegex = new Regex(@"^\d{4}\s\d{6}$");
            if (!passportRegex.IsMatch(txtPassport.Text))
            {
                MessageBox.Show("Паспортные данные должны быть в формате: XXXX XXXXXX", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassport.Focus();
                return false;
            }

            // Проверка телефона (если указан)
            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                var phoneRegex = new Regex(@"^\+?[78]\d{10}$");
                string cleanPhone = txtPhone.Text.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");

                if (!phoneRegex.IsMatch(cleanPhone))
                {
                    MessageBox.Show("Телефон должен быть в формате: +7XXXXXXXXXX или 8XXXXXXXXXX", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPhone.Focus();
                    return false;
                }
            }

            // Проверка email (если указан)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(txtEmail.Text))
                {
                    MessageBox.Show("Введите корректный email адрес", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }

        private void SavePassenger(string fullName, string phone, string email, string passport)
        {
            try
            {
                // Обновляем данные пассажира
                _passenger.FullName = fullName.Trim();
                _passenger.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
                _passenger.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
                _passenger.PassportData = passport.Trim();

                bool success;

                if (_isEditMode)
                {
                    success = _passengerService.UpdatePassenger(_passenger);
                }
                else
                {
                    success = _passengerService.CreatePassenger(_passenger);
                }

                if (success)
                {
                    string message = _isEditMode ?
                        "Данные пассажира успешно обновлены!" :
                        "Новый пассажир успешно добавлен!";

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
    }
}