using System;
using System.Windows.Forms;
using UD_WForms.Models.Database;

namespace UD_WForms.Forms
{
    public partial class ConnectionSettingsForm : Form
    {
        public ConnectionSettingsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Настройки подключения к БД";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Поля формы
            var lblHost = new Label() { Text = "Хост:", Left = 10, Top = 20, Width = 100 };
            var txtHost = new TextBox() { Left = 120, Top = 20, Width = 200, Text = "localhost" };

            var lblPort = new Label() { Text = "Порт:", Left = 10, Top = 60, Width = 100 };
            var txtPort = new TextBox() { Left = 120, Top = 60, Width = 200, Text = "5432" };

            var lblDatabase = new Label() { Text = "База данных:", Left = 10, Top = 100, Width = 100 };
            var txtDatabase = new TextBox() { Left = 120, Top = 100, Width = 200, Text = "AviaDB" };

            var lblUsername = new Label() { Text = "Пользователь:", Left = 10, Top = 140, Width = 100 };
            var txtUsername = new TextBox() { Left = 120, Top = 140, Width = 200, Text = "postgres" };

            var lblPassword = new Label() { Text = "Пароль:", Left = 10, Top = 180, Width = 100 };
            var txtPassword = new TextBox() { Left = 120, Top = 180, Width = 200, UseSystemPasswordChar = true };

            var btnTest = new Button() { Text = "Тест подключения", Left = 120, Top = 220, Width = 120 };
            var btnSave = new Button() { Text = "Сохранить", Left = 250, Top = 220, Width = 80 };
            var btnCancel = new Button() { Text = "Отмена", Left = 10, Top = 220, Width = 80 };

            btnTest.Click += (s, e) =>
            {
                try
                {
                    ConnectionFactory.SetConnectionString(txtHost.Text, txtPort.Text,
                        txtDatabase.Text, txtUsername.Text, txtPassword.Text);

                    if (ConnectionFactory.TestConnection())
                    {
                        MessageBox.Show("Подключение успешно!", "Тест подключения",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось подключиться к серверу", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnSave.Click += (s, e) =>
            {
                ConnectionFactory.SetConnectionString(txtHost.Text, txtPort.Text,
                    txtDatabase.Text, txtUsername.Text, txtPassword.Text);
                MessageBox.Show("Настройки сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new Control[] {
                lblHost, txtHost,
                lblPort, txtPort,
                lblDatabase, txtDatabase,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                btnTest, btnSave, btnCancel
            });

            this.ResumeLayout(false);
        }

        private void LoadCurrentSettings()
        {
            // Можно добавить загрузку текущих настроек из конфигурации
        }
    }
}