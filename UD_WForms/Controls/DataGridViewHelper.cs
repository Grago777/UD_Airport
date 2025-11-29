using System.Windows.Forms;

namespace UD_WForms.Controls
{
    public static class DataGridViewHelper
    {
        public static void ConfigureDataGridView(DataGridView dataGridView)
        {
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.RowHeadersVisible = false;

            // Стиль для лучшего внешнего вида
            dataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
        }

        public static void AddActionButtons(DataGridView dataGridView)
        {
            // Колонка для кнопки редактирования
            var editButton = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "Действие",
                Text = "✏️",
                UseColumnTextForButtonValue = true,
                Width = 60
            };

            // Колонка для кнопки удаления
            var deleteButton = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "Удалить",
                Text = "🗑️",
                UseColumnTextForButtonValue = true,
                Width = 60
            };

            dataGridView.Columns.Add(editButton);
            dataGridView.Columns.Add(deleteButton);
        }
    }
}