using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms;

namespace UD_WForms.Forms
{
    public partial class PassengerForm : Form
    {
        private IPassengerService _passengerService;
        private Passenger _passenger;
        private bool _isEditMode;

        public PassengerForm(int passengerId, IPassengerService passengerService)
        {
            InitializeComponent();
            /*_passengerService = ServiceLocator.GetService<IPassengerService>();
            LoadPassengers();*/
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
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = _isEditMode ? "Редактирование пассажира" : "Добавление пассажира";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Поля формы
            var lblFullName = new Label() { Text = "ФИО:*", Left = 10, Top = 20, Width = 120 };
            var txtFullName = new TextBox() { Left = 140, Top = 20, Width = 200 };

            var lblPhone = new Label() { Text = "Телефон:", Left = 10, Top = 60, Width = 120 };
            var txtPhone = new TextBox() { Left = 140, Top = 60, Width = 200 };

            var lblEmail = new Label() { Text = "Email:", Left = 10, Top = 100, Width = 120 };
            var txtEmail = new TextBox() { Left = 140, Top = 100, Width = 200 };

            var lblPassport = new Label() { Text = "Паспортные данные:*", Left = 10, Top = 140, Width = 120 };
            var txtPassport = new TextBox() { Left = 140, Top = 140, Width = 200 };

            var btnSave = new Button() { Text = "Сохранить", Left = 140, Top = 190, Width = 80 };
            var btnCancel = new Button() { Text = "Отмена", Left = 230, Top = 190, Width = 80 };

            // Загрузка данных для редактирования
            if (_isEditMode)
            {
                txtFullName.Text = _passenger.FullName;
                txtPhone.Text = _passenger.PhoneNumber;
                txtEmail.Text = _passenger.Email;
                txtPassport.Text = _passenger.PassportData;
            }

            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtFullName.Text) || string.IsNullOrEmpty(txtPassport.Text))
                {
                    MessageBox.Show("Заполните обязательные поля (ФИО и Паспортные данные)", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _passenger.FullName = txtFullName.Text;
                _passenger.PhoneNumber = string.IsNullOrEmpty(txtPhone.Text) ? null : txtPhone.Text;
                _passenger.Email = string.IsNullOrEmpty(txtEmail.Text) ? null : txtEmail.Text;
                _passenger.PassportData = txtPassport.Text;

                try
                {
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
                        MessageBox.Show("Данные пассажира успешно сохранены!", "Успех",
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
                lblFullName, txtFullName,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblPassport, txtPassport,
                btnSave, btnCancel
            });

            this.ResumeLayout(false);
        }

        private void LoadPassenger(int passengerId)
        {
            try
            {
                _passenger = _passengerService.GetPassengerById(passengerId);
                if (_passenger == null)
                {
                    MessageBox.Show("Пассажир не найден", "Ошибка",
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
        /*public void RefreshData()
        {
            LoadPassengers();
        }*/
    }
}