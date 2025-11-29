using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;

namespace UD_WForms.Forms
{
    public partial class TicketsForm : Form
    {
        private TicketService _ticketService;
        private DataGridView dataGridView;

        public TicketsForm()
        {
            InitializeComponent();
            _ticketService = new TicketService();
            LoadTickets();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление билетами";
            this.Size = new System.Drawing.Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // DataGridView
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.Controls.Add(dataGridView);

            // Панель кнопок
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 50;

            Button addButton = new Button();
            addButton.Text = "Добавить билет";
            addButton.Location = new System.Drawing.Point(10, 10);
            addButton.Size = new System.Drawing.Size(120, 30);
            addButton.Click += AddButton_Click;

            Button refreshButton = new Button();
            refreshButton.Text = "Обновить";
            refreshButton.Location = new System.Drawing.Point(140, 10);
            refreshButton.Size = new System.Drawing.Size(80, 30);
            refreshButton.Click += (s, e) => LoadTickets();

            Button closeButton = new Button();
            closeButton.Text = "Закрыть";
            closeButton.Location = new System.Drawing.Point(230, 10);
            closeButton.Size = new System.Drawing.Size(80, 30);
            closeButton.Click += (s, e) => this.Close();

            buttonPanel.Controls.AddRange(new Control[] { addButton, refreshButton, closeButton });
            this.Controls.Add(buttonPanel);

            this.ResumeLayout(false);
        }

        private void LoadTickets()
        {
            try
            {
                var tickets = _ticketService.GetAllTickets();
                dataGridView.DataSource = tickets;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки билетов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // Форма добавления билета
            using (var form = new Form())
            {
                form.Text = "Добавить новый билет";
                form.Size = new System.Drawing.Size(400, 350);
                form.StartPosition = FormStartPosition.CenterParent;

                var ticket = new Ticket();

                // Поля формы
                var lblTicketNumber = new Label() { Text = "Номер билета:", Left = 10, Top = 20, Width = 120 };
                var txtTicketNumber = new TextBox() { Left = 140, Top = 20, Width = 200 };

                var lblFlightNumber = new Label() { Text = "Номер рейса:", Left = 10, Top = 60, Width = 120 };
                var txtFlightNumber = new TextBox() { Left = 140, Top = 60, Width = 200 };

                var lblPassengerId = new Label() { Text = "ID пассажира:", Left = 10, Top = 100, Width = 120 };
                var numPassengerId = new NumericUpDown() { Left = 140, Top = 100, Width = 100, Minimum = 1 };

                var lblClass = new Label() { Text = "Класс:", Left = 10, Top = 140, Width = 120 };
                var cmbClass = new ComboBox() { Left = 140, Top = 140, Width = 200 };
                cmbClass.Items.AddRange(new string[] { "Эконом", "Бизнес" });

                var lblPrice = new Label() { Text = "Стоимость:", Left = 10, Top = 180, Width = 120 };
                var numPrice = new NumericUpDown() { Left = 140, Top = 180, Width = 100, DecimalPlaces = 2, Minimum = 0 };

                var btnSave = new Button() { Text = "Сохранить", Left = 140, Top = 230, Width = 80 };
                var btnCancel = new Button() { Text = "Отмена", Left = 230, Top = 230, Width = 80 };

                btnSave.Click += (s, ev) =>
                {
                    if (string.IsNullOrEmpty(txtTicketNumber.Text) || string.IsNullOrEmpty(txtFlightNumber.Text))
                    {
                        MessageBox.Show("Заполните обязательные поля", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    ticket.TicketNumber = txtTicketNumber.Text;
                    ticket.FlightNumber = txtFlightNumber.Text;
                    ticket.PassengerId = (int)numPassengerId.Value;
                    ticket.Class = cmbClass.SelectedItem?.ToString() ?? "Эконом";
                    ticket.Status = "Активен";
                    ticket.Luggage = 0;
                    ticket.Price = numPrice.Value;

                    if (_ticketService.CreateTicket(ticket))
                    {
                        MessageBox.Show("Билет успешно создан!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        form.DialogResult = DialogResult.OK;
                        LoadTickets();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при создании билета", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnCancel.Click += (s, ev) => form.DialogResult = DialogResult.Cancel;

                form.Controls.AddRange(new Control[] {
                    lblTicketNumber, txtTicketNumber,
                    lblFlightNumber, txtFlightNumber,
                    lblPassengerId, numPassengerId,
                    lblClass, cmbClass,
                    lblPrice, numPrice,
                    btnSave, btnCancel
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Билет создан
                }
            }
        }
    }
}