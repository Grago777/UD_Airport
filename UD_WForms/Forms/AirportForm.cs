using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;
using System.Text.RegularExpressions;

namespace UD_WForms.Forms
{
    public partial class AirportForm : Form
    {
        private IAirportService _airportService;
        private Airport _airport;
        private bool _isEditMode;

        public AirportForm(int airportId, IAirportService airportService)
        {
            _airportService = airportService;
            _isEditMode = airportId > 0;

            if (_isEditMode)
            {
                LoadAirport(airportId);
            }
            else
            {
                _airport = new Airport();
            }

            InitializeForm();
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            this.Text = _isEditMode ? $"Редактирование аэропорта #{_airport.AirportId}" : "Добавление нового аэропорта";
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
                Text = _isEditMode ? "РЕДАКТИРОВАНИЕ АЭРОПОРТА" : "НОВЫЙ АЭРОПОРТ",
                Left = 20,
                Top = top,
                Width = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            top += 50;

            // Название
            var lblName = new Label() { Text = "Название:*", Left = leftLabel, Top = top, Width = 120 };
            var txtName = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _airport.Name ?? "",
                MaxLength = 100
            };
            top += spacing;

            // Код IATA
            var lblIATA = new Label() { Text = "Код IATA:*", Left = leftLabel, Top = top, Width = 120 };
            var txtIATA = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = 100,
                Text = _airport.IATACode ?? "",
                MaxLength = 3,
                CharacterCasing = CharacterCasing.Upper
            };
            top += spacing;

            // Страна
            var lblCountry = new Label() { Text = "Страна:*", Left = leftLabel, Top = top, Width = 120 };
            var txtCountry = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _airport.Country ?? "",
                MaxLength = 50
            };
            top += spacing;

            // Город
            var lblCity = new Label() { Text = "Город:*", Left = leftLabel, Top = top, Width = 120 };
            var txtCity = new TextBox()
            {
                Left = leftControl,
                Top = top,
                Width = controlWidth,
                Text = _airport.City ?? "",
                MaxLength = 50
            };
            top += 60;

            // Кнопки
            var btnSave = new Button()
            {
                Text = _isEditMode ? "💾 Сохранить изменения" : "➕ Добавить аэропорт",
                Left = leftControl,
                Top = top,
                Width = 150,
                BackColor = System.Drawing.Color.LightGreen,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
            };
            btnSave.Click += (s, e) =>
            {
                if (ValidateForm(txtName, txtIATA, txtCountry, txtCity))
                {
                    SaveAirport(txtName.Text, txtIATA.Text, txtCountry.Text, txtCity.Text);
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
                lblName, txtName,
                lblIATA, txtIATA,
                lblCountry, txtCountry,
                lblCity, txtCity,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
            this.ResumeLayout(false);
        }

        private void LoadAirport(int airportId)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _airport = _airportService.GetAirportById(airportId);

                if (_airport == null)
                {
                    MessageBox.Show("Аэропорт не найден", "Ошибка",
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

        private bool ValidateForm(TextBox txtName, TextBox txtIATA, TextBox txtCountry, TextBox txtCity)
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название аэропорта", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            if (txtName.Text.Length < 3)
            {
                MessageBox.Show("Название аэропорта должно содержать не менее 3 символов", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            // Проверка кода IATA
            if (string.IsNullOrWhiteSpace(txtIATA.Text))
            {
                MessageBox.Show("Введите код IATA", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIATA.Focus();
                return false;
            }

            if (txtIATA.Text.Length != 3)
            {
                MessageBox.Show("Код IATA должен состоять из 3 символов", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIATA.Focus();
                return false;
            }

            var iataRegex = new Regex(@"^[A-Z]{3}$");
            if (!iataRegex.IsMatch(txtIATA.Text))
            {
                MessageBox.Show("Код IATA должен содержать только латинские буквы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIATA.Focus();
                return false;
            }

            // Проверка страны
            if (string.IsNullOrWhiteSpace(txtCountry.Text))
            {
                MessageBox.Show("Введите страну", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCountry.Focus();
                return false;
            }

            // Проверка города
            if (string.IsNullOrWhiteSpace(txtCity.Text))
            {
                MessageBox.Show("Введите город", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCity.Focus();
                return false;
            }

            return true;
        }

        private void SaveAirport(string name, string iata, string country, string city)
        {
            try
            {
                // Обновляем данные аэропорта
                _airport.Name = name.Trim();
                _airport.IATACode = iata.Trim().ToUpper();
                _airport.Country = country.Trim();
                _airport.City = city.Trim();

                bool success;

                if (_isEditMode)
                {
                    success = _airportService.UpdateAirport(_airport);
                }
                else
                {
                    success = _airportService.CreateAirport(_airport);
                }

                if (success)
                {
                    string message = _isEditMode ?
                        "Данные аэропорта успешно обновлены!" :
                        "Новый аэропорт успешно добавлен!";

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