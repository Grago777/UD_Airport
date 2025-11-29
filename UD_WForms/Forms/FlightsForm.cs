using System;
using System.Windows.Forms;
using System.Collections.Generic;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms.Controls;

namespace UD_WForms.Forms
{
    public partial class FlightsForm : Form
    {
        private IFlightService _flightService;
        private DataGridView dataGridView;
        private Button btnClose;
        private Button btnAdd;

        public FlightsForm()
        {
            InitializeComponent();
            try
            {
                _flightService = ServiceLocator.GetService<IFlightService>();
                LoadFlights();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление рейсами";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // Простая панель
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = 50;
            panel.Padding = new Padding(10);

            btnAdd = new Button() { Text = "Добавить рейс", Left = 10, Top = 10, Size = new System.Drawing.Size(100, 30) };
            btnAdd.Click += BtnAdd_Click;

            btnClose = new Button() { Text = "Закрыть", Left = 120, Top = 10, Size = new System.Drawing.Size(80, 30) };
            btnClose.Click += (s, e) => this.Close();

            panel.Controls.AddRange(new Control[] { btnAdd, btnClose });
            this.Controls.Add(panel);

            // DataGridView
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            DataGridViewHelper.ConfigureDataGridView(dataGridView);
            this.Controls.Add(dataGridView);

            this.ResumeLayout(false);
        }

        private void LoadFlights()
        {
            try
            {
                var flights = _flightService.GetAllFlights();
                dataGridView.DataSource = flights;

                if (flights.Count == 0)
                {
                    MessageBox.Show("Нет данных о рейсах. Возможно, база данных пуста.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рейсов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Форма добавления рейса будет открыта", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Простая форма для тестирования
                using (var form = new SimpleFlightForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadFlights();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshData()
        {
            LoadFlights();
        }
    }

    // Простая тестовая форма
    public class SimpleFlightForm : Form
    {
        public SimpleFlightForm()
        {
            this.Text = "Добавление рейса (тест)";
            this.Size = new System.Drawing.Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;

            var label = new Label()
            {
                Text = "Тестовая форма рейса\n\nЭта форма открылась успешно!",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            var btnOk = new Button() { Text = "OK", DialogResult = DialogResult.OK };
            btnOk.Location = new System.Drawing.Point(100, 120);

            this.Controls.AddRange(new Control[] { label, btnOk });
            this.AcceptButton = btnOk;
        }
    }
}