using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;

namespace UD_WForms.Forms
{
    public partial class AirportForm : Form
    {
        private IAirportService _airportService;
        private Airport _airport;
        private bool _isEditMode;

        public AirportForm(int airportId, IAirportService airportService)
        {
            InitializeComponent();
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
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = _isEditMode ? "Редактирование аэропорта" : "Добавление аэропорта";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Поля формы
            var lblName = new Label() { Text = "Название:*", Left = 10, Top = 20, Width = 120 };
            var txtName = new TextBox() { Left = 140, Top = 20, Width = 200 };

            var lblIATA = new Label() { Text = "Код IATA:*", Left = 10, Top = 60, Width = 120 };
            var txtIATA = new TextBox() { Left = 140, Top = 60, Width = 100, MaxLength = 3 };
            txtIATA.CharacterCasing = CharacterCasing.Upper;

            var lblCountry = new Label() { Text = "Страна:*", Left = 10, Top = 100, Width = 120 };
            var txtCountry = new TextBox() { Left = 140, Top = 100, Width = 200 };

            var lblCity = new Label() { Text = "Город:*", Left = 10, Top = 140, Width = 120 };
            var txtCity = new TextBox() { Left = 140, Top = 140, Width = 200 };

            var btnSave = new Button() { Text = "Сохранить", Left = 140, Top = 190, Width = 80 };
            var btnCancel = new Button() { Text = "Отмена", Left = 230, Top = 190, Width = 80 };

            // Загрузка данных для редактирования
            if (_isEditMode)
            {
                txtName.Text = _airport.Name;
                txtIATA.Text = _airport.IATACode;
                txtCountry.Text = _airport.Country;
                txtCity.Text = _airport.City;
            }

            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtIATA.Text) ||
                    string.IsNullOrEmpty(txtCountry.Text) || string.IsNullOrEmpty(txtCity.Text))
                {
                    MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtIATA.Text.Length != 3)
                {
                    MessageBox.Show("Код IATA должен состоять из 3 символов", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _airport.Name = txtName.Text;
                _airport.IATACode = txtIATA.Text.ToUpper();
                _airport.Country = txtCountry.Text;
                _airport.City = txtCity.Text;

                try
                {
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
                        MessageBox.Show("Данные аэропорта успешно сохранены!", "Успех",
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
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblIATA, txtIATA,
                lblCountry, txtCountry,
                lblCity, txtCity,
                btnSave, btnCancel
            });

            this.ResumeLayout(false);
        }

        private void LoadAirport(int airportId)
        {
            try
            {
                _airport = _airportService.GetAirportById(airportId);
                if (_airport == null)
                {
                    MessageBox.Show("Аэропорт не найден", "Ошибка",
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
    }
}