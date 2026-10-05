using System;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using static Vulpes0.Program;

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

            this.Text = $"Маркетплейс — Вы вошли как: {UserSession.Fio} ({UserSession.Role})";

            var tabsToRemove = new System.Collections.Generic.List<TabPage>();

            if (UserSession.Role == "Продавец")
            {
                foreach (TabPage tab in tabControl1.TabPages)
                {
                    string title = tab.Text.Trim().ToLower();
                    if (title == "корзина" || title == "пользователи")
                        tabsToRemove.Add(tab);
                }
            }
            else if (UserSession.Role == "Курьер")
            {
                foreach (TabPage tab in tabControl1.TabPages)
                {
                    string title = tab.Text.Trim().ToLower();
                    if (title == "товары" || title == "корзина" || title == "состав заказа" || title == "пользователи")
                        tabsToRemove.Add(tab);
                }
            }
            else if (UserSession.Role == "Покупатель")
            {
                foreach (TabPage tab in tabControl1.TabPages)
                {
                    string title = tab.Text.Trim().ToLower();
                    if (title == "пользователи")
                        tabsToRemove.Add(tab);
                }
            }

            foreach (TabPage tab in tabsToRemove)
                tabControl1.TabPages.Remove(tab);

            ApplyRoleFilters();

            // Подписываемся на события
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;

            // И сразу вызываем, чтобы кнопки правильно отрисовались
            tabControl1_SelectedIndexChanged(null, null);
        }

        private void RefreshData()
        {
            try
            {
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

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;
            string currentTab = tabControl1.SelectedTab.Text.Trim().ToLower();

            // По умолчанию всё скрываем
            button1.Visible = false;
            button2.Visible = false;
            btnAssemble.Visible = false;
            btnAssignCourier.Visible = false;
            btnCheckout.Visible = false;
            btnCourierStatus.Visible = false;

            if (UserSession.Role == "Продавец")
            {
                button1.Visible = true; // Добавить
                button2.Visible = true; // Удалить

                if (currentTab == "заказы")
                {
                    btnAssemble.Visible = true;
                    btnAssignCourier.Visible = true;
                }
            }
            else if (UserSession.Role == "Покупатель")
            {
                if (currentTab == "корзина")
                {
                    button2.Visible = true;      // Удалить
                    btnCheckout.Visible = true;  // Оформить заказ
                }
            }
            else if (UserSession.Role == "Курьер")
            {
                if (currentTab == "заказы")
                {
                    btnCourierStatus.Visible = true;
                }
            }
        }

        // открываем карточку через дабл килл
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value == null) return;

            int idTov = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value);

            CardForm card = new CardForm(idTov);
            if (card.ShowDialog() == DialogResult.OK)
            {
                RefreshData();
                ApplyRoleFilters();
            }
        }

        // Клик по стандартной кнопке "button1" (Добавить новую запись)
        private void button1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;

            string currentTabText = tabControl1.SelectedTab.Text;
            AddForm popup = new AddForm(currentTabText);

            if (popup.ShowDialog() == DialogResult.OK)
            {
                RefreshData();
                ApplyRoleFilters();
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

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы уверены, что хотите сменить пользователя?",
                                      "Выход из системы",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UserSession.IdUser = 0;
                UserSession.Role = "";
                UserSession.Fio = "";

                this.Hide();

                AuthForm auth = new AuthForm();
                auth.ShowDialog();

                this.Close();
            }
        }
        private void ApplyRoleFilters()
        {
            string connString = Properties.Settings.Default.МаркетплейсConnectionString;

            if (UserSession.Role == "Продавец")
            {
                // 1. Товары — только этого продавца
                if (товарыBindingSource != null)
                    товарыBindingSource.Filter = $"id_seller = {UserSession.IdUser}";

                // 2. Собираем список id_order, где есть хотя бы один его товар
                var orderIds = new System.Collections.Generic.List<int>();
                try
                {
                    using (var conn = new System.Data.OleDb.OleDbConnection(connString))
                    {
                        conn.Open();
                        string q = @"SELECT DISTINCT СоставЗаказа.id_order 
                                     FROM СоставЗаказа 
                                     INNER JOIN Товары ON СоставЗаказа.id_tov = Товары.id_tov 
                                     WHERE Товары.id_seller = ?";
                        using (var cmd = new System.Data.OleDb.OleDbCommand(q, conn))
                        {
                            cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = UserSession.IdUser;
                            using (var rd = cmd.ExecuteReader())
                                while (rd.Read())
                                    orderIds.Add(Convert.ToInt32(rd["id_order"]));
                        }
                    }
                }
                catch { /* если что-то не так — фильтр будет пустой */ }

                string orderFilter = orderIds.Count == 0
                    ? "1 = 0"
                    : $"id_order IN ({string.Join(",", orderIds)})";

                // 3. Заказы, Состав заказа, Доставка — только по этим заказам
                if (заказыBindingSource != null)
                    заказыBindingSource.Filter = orderFilter;

                if (составЗаказаBindingSource != null)
                    составЗаказаBindingSource.Filter = orderFilter;

                if (доставкаBindingSource != null)
                    доставкаBindingSource.Filter = orderFilter;
            }
            else if (UserSession.Role == "Курьер")
            {
                // 1. Заказы — только его
                if (заказыBindingSource != null)
                    заказыBindingSource.Filter = $"id_courier = {UserSession.IdUser}";

                // 2. Собираем id_order этого курьера
                var orderIds = new System.Collections.Generic.List<int>();
                try
                {
                    using (var conn = new System.Data.OleDb.OleDbConnection(connString))
                    {
                        conn.Open();
                        using (var cmd = new System.Data.OleDb.OleDbCommand(
                            "SELECT id_order FROM Заказы WHERE id_courier = ?", conn))
                        {
                            cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = UserSession.IdUser;
                            using (var rd = cmd.ExecuteReader())
                                while (rd.Read())
                                    orderIds.Add(Convert.ToInt32(rd["id_order"]));
                        }
                    }
                }
                catch { }

                // 3. Доставка — только по его заказам
                if (доставкаBindingSource != null)
                    доставкаBindingSource.Filter = orderIds.Count == 0
                        ? "1 = 0"
                        : $"id_order IN ({string.Join(",", orderIds)})";
            }
            else if (UserSession.Role == "Покупатель")
            {
                if (заказыBindingSource != null)
                    заказыBindingSource.Filter = $"id_user = {UserSession.IdUser}";

                if (корзинаBindingSource != null)
                    корзинаBindingSource.Filter = $"id_user = {UserSession.IdUser}";
            }
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            CheckoutForm checkout = new CheckoutForm();
            if (checkout.ShowDialog() == DialogResult.OK)
            {
                RefreshData();
                ApplyRoleFilters();
            }
        }

        //  СБОРКА ЗАКАЗА 
        private void btnAssemble_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;
            if (tabControl1.SelectedTab.Text.Trim().ToLower() != "заказы")
            {
                MessageBox.Show("Сборка доступна только на вкладке 'Заказы'!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView3.CurrentRow == null || dataGridView3.CurrentRow.Cells[0].Value == null)
            {
                MessageBox.Show("Выберите заказ в таблице!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idOrder = Convert.ToInt32(dataGridView3.CurrentRow.Cells[0].Value);
            string status = dataGridView3.CurrentRow.Cells[2].Value?.ToString() ?? "";

            if (status != "Новый")
            {
                MessageBox.Show($"Этот заказ уже в работе (статус: {status}).", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connString = Properties.Settings.Default.МаркетплейсConnectionString;

            try
            {
                using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connString))
                {
                    conn.Open();

                    // Проверка наличия товаров на складе
                    string checkQuery = @"SELECT Товары.[name], Товары.stock, СоставЗаказа.kolvo
                                          FROM СоставЗаказа 
                                          INNER JOIN Товары ON СоставЗаказа.id_tov = Товары.id_tov
                                          WHERE СоставЗаказа.id_order = ?";
                    using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(checkQuery, conn))
                    {
                        cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = idOrder;
                        using (System.Data.OleDb.OleDbDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                string name = rd["name"].ToString();
                                int stock = Convert.ToInt32(rd["stock"]);
                                int kolvo = Convert.ToInt32(rd["kolvo"]);
                                if (stock < kolvo)
                                {
                                    MessageBox.Show($"Товар \"{name}\" отсутствует в нужном количестве на складе!\n" +
                                                    $"Нужно: {kolvo}, есть: {stock}.",
                                                    "Товара нет в наличии", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                        }
                    }

                    // В сборке
                    using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(
                        "UPDATE Заказы SET status = ? WHERE id_order = ?", conn))
                    {
                        cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.VarWChar).Value = "В сборке";
                        cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = idOrder;
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Заказ собран! Статус изменён на 'В сборке'.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshData();
                ApplyRoleFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сборке заказа:\n" + ex.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //  НАЗНАЧЕНИЕ КУРЬЕРА 
        private void btnAssignCourier_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;
            if (tabControl1.SelectedTab.Text.Trim().ToLower() != "заказы")
            {
                MessageBox.Show("Назначение курьера доступно только на вкладке 'Заказы'!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView3.CurrentRow == null || dataGridView3.CurrentRow.Cells[0].Value == null)
            {
                MessageBox.Show("Выберите заказ в таблице!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idOrder = Convert.ToInt32(dataGridView3.CurrentRow.Cells[0].Value);
            string status = dataGridView3.CurrentRow.Cells[2].Value?.ToString() ?? "";

            if (status == "Новый")
            {
                MessageBox.Show("Сначала соберите заказ (статус должен быть 'В сборке' или выше).", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (status == "Выполнен")
            {
                MessageBox.Show("Заказ уже выполнен, курьера менять нельзя.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CourierPickForm picker = new CourierPickForm();
            if (picker.ShowDialog() != DialogResult.OK) return;

            string connString = Properties.Settings.Default.МаркетплейсConnectionString;

            try
            {
                using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connString))
                {
                    conn.Open();
                    using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(
                        "UPDATE Заказы SET id_courier = ? WHERE id_order = ?", conn))
                    {
                        cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = picker.SelectedCourierId;
                        cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = idOrder;
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Курьер {picker.SelectedCourierName} назначен на заказ!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshData();
                ApplyRoleFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при назначении курьера:\n" + ex.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // КУРЬЕР ОБНОВИТЬ СТАТУС ЗАКАЗА 
        private void btnCourierStatus_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;
            if (tabControl1.SelectedTab.Text.Trim().ToLower() != "заказы")
            {
                MessageBox.Show("Изменение статуса доступно только на вкладке 'Заказы'!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView3.CurrentRow == null || dataGridView3.CurrentRow.Cells[0].Value == null)
            {
                MessageBox.Show("Выберите заказ в таблице!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idOrder = Convert.ToInt32(dataGridView3.CurrentRow.Cells[0].Value);
            string status = dataGridView3.CurrentRow.Cells[2].Value?.ToString() ?? "";

            string newStatus;
            string message;

            if (status == "В сборке")
            {
                newStatus = "Доставляется";
                message = "Заказ взят в доставку?";
            }
            else if (status == "Доставляется")
            {
                newStatus = "Выполнен";
                message = "Заказ доставлен покупателю?";
            }
            else if (status == "Выполнен")
            {
                MessageBox.Show("Заказ уже выполнен.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                MessageBox.Show($"Нельзя изменить статус \"{status}\" — заказ ещё не передан в доставку.",
                                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(message, "Подтверждение",
                                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            string connString = Properties.Settings.Default.МаркетплейсConnectionString;

            try
            {
                using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connString))
                {
                    conn.Open();
                    using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(
                        "UPDATE Заказы SET status = ? WHERE id_order = ?", conn))
                    {
                        cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.VarWChar).Value = newStatus;
                        cmd.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = idOrder;
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Статус заказа изменён на \"{newStatus}\".", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshData();
                ApplyRoleFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении статуса:\n" + ex.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}