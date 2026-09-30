using System;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Vulpes0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Вызывается при запуске приложения
        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        // Универсальный метод для обновления данных в таблицах на экране
        private void RefreshData()
        {
            try
            {
                // Visual Studio сама сгенерировала эти строки. Они берут данные из DataSet.
                this.пользователиTableAdapter.Fill(this.маркетплейсDataSet.Пользователи);
                this.доставкаTableAdapter.Fill(this.маркетплейсDataSet.Доставка);
                this.составЗаказаTableAdapter.Fill(this.маркетплейсDataSet.СоставЗаказа);
                this.заказыTableAdapter.Fill(this.маркетплейсDataSet.Заказы);
                this.корзинаTableAdapter.Fill(this.маркетплейсDataSet.Корзина);
                this.товарыTableAdapter.Fill(this.маркетплейсDataSet.Товары);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке или обновлении таблиц:\n" + ex.Message,
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Клик по стандартной кнопке "button1" (Добавить новую запись)
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли какая-то вкладка в вашем TabControl
            if (tabControl1.SelectedTab != null)
            {
                // Берем русский текст вкладки ("Товары" или "Пользователи")
                string currentTab = tabControl1.SelectedTab.Text;

                // Открываем всплывающее окошко и передаем ему имя вкладки
                AddForm popup = new AddForm(currentTab);

                // Если в окошке успешно нажали "Добавить" и данные записались в Access
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    // Мгновенно обновляем таблицы на главной форме, чтобы увидеть новую строку
                    RefreshData();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;

            string currentTab = tabControl1.SelectedTab.Text.Trim().ToLower();
            DataGridView activeGrid = null;
            string primaryKeyName = "";
            string tableNameInAccess = "";

            // Привязываем ваши упорядоченные сетки к вкладкам
            if (currentTab == "товары")
            {
                activeGrid = dataGridView1;
                primaryKeyName = "id_tov";
                tableNameInAccess = "Товары";
            }
            else if (currentTab == "корзина")
            {
                activeGrid = dataGridView2;
                primaryKeyName = "id_korz";
                tableNameInAccess = "Корзина";
            }
            else if (currentTab == "заказы")
            {
                activeGrid = dataGridView3;
                primaryKeyName = "id_order";
                tableNameInAccess = "Заказы";
            }
            else if (currentTab == "состав заказа")
            {
                activeGrid = dataGridView4;
                primaryKeyName = "id_detal";
                tableNameInAccess = "СоставЗаказа";
            }
            else if (currentTab == "доставка")
            {
                activeGrid = dataGridView5;
                primaryKeyName = "id_delivery";
                tableNameInAccess = "Доставка";
            }
            else if (currentTab == "пользователи")
            {
                activeGrid = dataGridView6;
                primaryKeyName = "id_user";
                tableNameInAccess = "Пользователи";
            }

            // Проверяем, выбрал ли пользователь строку в сетке
            if (activeGrid == null || activeGrid.CurrentRow == null || activeGrid.CurrentRow.Cells[0].Value == null)
            {
                MessageBox.Show("Пожалуйста, выберите кликом мыши строку в таблице для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем уникальный ID строки
            int selectedId = Convert.ToInt32(activeGrid.CurrentRow.Cells[0].Value);

            // Спрашиваем подтверждение
            DialogResult confirm = MessageBox.Show($"Вы уверены, что хотите безвозвратно удалить запись с ID {selectedId} из таблицы '{tableNameInAccess}'?",
                                                   "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string connString = Properties.Settings.Default.МаркетплейсConnectionString;
                string deleteQuery = $"DELETE FROM {tableNameInAccess} WHERE {primaryKeyName} = ?";

                try
                {
                    using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connString))
                    {
                        using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("?", selectedId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Запись успешно удалена из базы данных!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Вызываем ваш метод перезагрузки DataSet, чтобы строка пропала с экрана
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось удалить запись. Скорее всего, она связана с данными из других таблиц (например, вы пытаетесь удалить товар, который уже добавлен в чей-то заказ).\n\nДетали: " + ex.Message,
                                    "Ошибка целостности данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;

            string currentTab = tabControl1.SelectedTab.Text.Trim().ToLower();
            BindingSource bs = null;

            // 1. Привязываем источник данных в зависимости от вкладки
            if (currentTab == "товары") { bs = товарыBindingSource; }
            else if (currentTab == "корзина") { bs = корзинаBindingSource; }
            else if (currentTab == "заказы") { bs = заказыBindingSource; }
            else if (currentTab == "состав заказа") { bs = составЗаказаBindingSource; }
            else if (currentTab == "доставка") { bs = доставкаBindingSource; }
            else if (currentTab == "пользователи") { bs = пользователиBindingSource; }

            if (bs == null) return;

            string SearchText = textBox1.Text.Trim();

            try
            {
                // Если поле поиска пустое — сбрасываем фильтр и показываем всё
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    bs.Filter = "";
                    return;
                }

                // 2. Строим "умный" фильтр под каждую таблицу
                if (currentTab == "товары")
                {
                    // Ищет везде: в названии, артикуле и цвете
                    bs.Filter = $"name LIKE '%{SearchText}%' OR artikul LIKE '%{SearchText}%' OR color LIKE '%{SearchText}%'";
                }
                else if (currentTab == "доставка")
                {
                    // Ищет по адресу
                    bs.Filter = $"address LIKE '%{SearchText}%'";
                }
                else if (currentTab == "пользователи")
                {
                    // Теперь ищет по ФИО, телефону И роли (Курьер, Продавец, Покупатель)
                    bs.Filter = $"fio LIKE '%{SearchText}%' OR phone LIKE '%{SearchText}%' OR role LIKE '%{SearchText}%'";
                }
                // ТАКТИКА ДЛЯ ТАБЛИЦ С ЧИСЛАМИ: проверяем, ввёл ли пользователь вообще число
                else if (currentTab == "корзина")
                {
                    if (int.TryParse(SearchText, out int num))
                        bs.Filter = $"id_tov = {num} OR id_user = {num}";
                    else
                        bs.Filter = "1 = 0"; // Хитрость: заставляем фильтр вернуть пустой результат, если ввели буквы
                }
                // ... (блок товаров, доставки и пользователей оставляем без изменений)
                else if (currentTab == "заказы")
                {
                    // 1. Сначала ищем по текстовым колонкам (номер заказа и статус)
                    string filterText = $"nomer LIKE '%{SearchText}%' OR status LIKE '%{SearchText}%'";

                    // 2. Если ввели число, то добавляем поиск по числовым ID пользователей или заказа
                    if (int.TryParse(SearchText, out int num))
                    {
                        filterText += $" OR id_order = {num} OR id_user = {num}";
                    }

                    bs.Filter = filterText;
                }
                else if (currentTab == "корзина")
                {
                    if (int.TryParse(SearchText, out int num))
                        bs.Filter = $"id_tov = {num} OR id_user = {num}";
                    else
                        bs.Filter = "1 = 0";
                }
                else if (currentTab == "состав заказа")
                {
                    if (int.TryParse(SearchText, out int num))
                        bs.Filter = $"id_order = {num} OR id_tov = {num}";
                    else
                        bs.Filter = "1 = 0";
                }
                else if (currentTab == "состав заказа")
                {
                    if (int.TryParse(SearchText, out int num))
                        bs.Filter = $"id_order = {num} OR id_tov = {num}";
                    else
                        bs.Filter = "1 = 0";
                }

                // 3. Если по итогу фильтрации ничего не нашлось
                if (bs.Count == 0)
                {
                    MessageBox.Show("По вашему запросу ничего не найдено!", "Результаты поиска", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    bs.Filter = "";     // Сбрасываем, чтобы таблица не оставалась пустой
                    textBox1.Text = ""; // Очищаем поле ввода для удобства
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении поиска:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                bs.Filter = "";
            }
        }
    }
}
