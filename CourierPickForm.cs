using System;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace Vulpes0
{
    public partial class CourierPickForm : Form
    {
        private string connectionString;
        private ComboBox cmbCouriers = new ComboBox();
        public int SelectedCourierId { get; private set; } = 0;
        public string SelectedCourierName { get; private set; } = "";

        public CourierPickForm()
        {
            InitializeComponent();
            connectionString = Properties.Settings.Default.МаркетплейсConnectionString;
            BuildInterface();
            LoadCouriers();
        }

        private void BuildInterface()
        {
            this.Text = "Назначение курьера";
            this.Size = new Size(420, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Label lbl = new Label();
            lbl.Text = "Выберите курьера:";
            lbl.Font = new Font("Segoe UI", 10);
            lbl.Location = new Point(20, 20);
            lbl.Size = new Size(360, 25);
            this.Controls.Add(lbl);

            cmbCouriers.Font = new Font("Segoe UI", 10);
            cmbCouriers.Location = new Point(20, 50);
            cmbCouriers.Size = new Size(360, 25);
            cmbCouriers.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbCouriers);

            Button btnOk = new Button();
            btnOk.Text = "Назначить";
            btnOk.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnOk.BackColor = Color.FromArgb(46, 125, 50);
            btnOk.ForeColor = Color.White;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Size = new Size(170, 40);
            btnOk.Location = new Point(20, 90);
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCancel.BackColor = Color.FromArgb(219, 68, 85);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Size = new Size(170, 40);
            btnCancel.Location = new Point(210, 90);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void LoadCouriers()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    string q = "SELECT id_user, fio FROM Пользователи WHERE [role] = ? ORDER BY fio";
                    using (OleDbCommand cmd = new OleDbCommand(q, conn))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = "Курьер";
                        using (OleDbDataReader rd = cmd.ExecuteReader())
                        {
                            bool any = false;
                            while (rd.Read())
                            {
                                int id = Convert.ToInt32(rd["id_user"]);
                                string fio = rd["fio"].ToString();
                                cmbCouriers.Items.Add(new CourierItem { Id = id, Fio = fio });
                                any = true;
                            }
                            if (!any)
                            {
                                MessageBox.Show("В системе нет ни одного курьера!", "Внимание",
                                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                this.Close();
                            }
                        }
                    }
                }
                if (cmbCouriers.Items.Count > 0) cmbCouriers.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки курьеров:\n" + ex.Message,
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (cmbCouriers.SelectedItem == null)
            {
                MessageBox.Show("Выберите курьера из списка!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var item = (CourierItem)cmbCouriers.SelectedItem;
            SelectedCourierId = item.Id;
            SelectedCourierName = item.Fio;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Маленький внутренний класс, чтобы ComboBox показывал ФИО, но хранил id
        private class CourierItem
        {
            public int Id { get; set; }
            public string Fio { get; set; }
            public override string ToString() => Fio;
        }
    }
}