using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AdoNetSamples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // localdb ye sağ tıklayıp property den connectionn stringi çekebiliriz.

        // Data Source = (localdb)\MSSQLLocalDB;Initial Catalog = master; Integrated Security = True; Connect Timeout = 30; Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False

        private string connstr = "Server=(localdb)\\MSSQLLocalDB; Database=Northwind;Trusted_Connection=True";


        private void btnBaglan_Click(object sender, EventArgs e)
        {
            // SQL e kullanıcı adı ve şifre ile bağlanmak istersek ;
            // Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;

            // Windows Authentication ile bağlanmak istersek;
            // Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;


            // 1. BAĞLANMA YÖNTEMİ

            //SqlConnection connection = new SqlConnection();
            //connection.ConnectionString = connstr;

            // 2. BAĞLANMA YÖNTEMİ
            SqlConnection connection = new SqlConnection(connstr);

            connection.Open();
            MessageBox.Show("Bağlantı Açıldı.");

            connection.Close();
            MessageBox.Show("Bağlantı Kapandı");
        }

        private void btnLoadCustomers_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connstr);   // Bağlantı nesnemizi oluşturduk

            string query = "SELECT * FROM Customers";

            // 1. YÖNTEM

            SqlCommand cmd = new SqlCommand(query, conn);

            // 2. YÖNTEM

            //SqlCommand cmd = new SqlCommand();
            //cmd.CommandText = query;
            //cmd.Connection = conn;

            // conn açar sorguyu çalıştırır ve conn kapatır.

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);  // sql data adaptörünü oluşturduk
            DataTable table = new DataTable();   // dgv için tablo oluşturduk

            adapter.Fill(table);  // tablonun için fill methodu ile doldurduk

            dgvCustomers.DataSource = table;  //dgv ye yazdırdık
        }

        private void btnLoadCustomers2_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connstr);
            string query = "SELECT CustomerID, CompanyName, ContactName FROM Customers";
            SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();  // data okuma işlemi yapılır

            DataTable table = new DataTable();
            table.Columns.Add("CustomerID", typeof(string));
            table.Columns.Add("CompanyName", typeof(string));
            table.Columns.Add("ContactName", typeof(string));

            while (reader.Read())
            {
                DataRow row = table.NewRow();
                row["CustomerID"] = reader["CustomerID"];
                row["CompanyName"] = reader["CompanyName"];
                row["ContactName"] = reader["ContactName"];

                table.Rows.Add(row);

                //if (row["CustomerID"].ToString().StartsWith("B"))   // sadece B harfi ile başlayanları getirir.
                //{
                //table.Rows.Add(row);
                //}

            }

            reader.Close();
            conn.Close();

            //reader.DisposeAsync();  // ram'den yok etmek için
            //conn.Dispose();

            dgvCustomers.DataSource = table;

        }

        private void btnInsertCustomer_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connstr);

            string custId = txtCustomerID.Text;
            string compName = txtCompanyName.Text;

            //TEK TIRNAKLA YAZMAK GÜVENLİK AÇIĞIDIR.

            //string query = $"INSERT INTO [dbo].[Customers] ([CustomerID] ,[CompanyName] ,[ContactName] ,[ContactTitle] ,[Address] ,[City] ,[Region] ,[PostalCode] ,[Country] ,[Phone] ,[Fax]) VALUES ('{custId}' ,'{compName}' ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL)";

            string query = $"INSERT INTO [dbo].[Customers] ([CustomerID] ,[CompanyName] ,[ContactName] ,[ContactTitle] ,[Address] ,[City] ,[Region] ,[PostalCode] ,[Country] ,[Phone] ,[Fax]) VALUES (@CustomerID, @CompanyName ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL ,NULL)";


            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CustomerID", custId);
            cmd.Parameters.AddWithValue("@CompanyName", compName);

            conn.Open();
            int count = cmd.ExecuteNonQuery(); // etkilenen satır sayısını döner. Insert / update / delete sorguları için kullanılır
            conn.Close();

            btnLoadCustomers_Click(btnLoadCustomers, EventArgs.Empty);

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

            SqlConnection conn = new SqlConnection(connstr);

            string custId = txtCustomerID.Text;
            string compName = txtCompanyName.Text;

            string query = $"UPDATE Customers SET CompanyName = @CompanyName WHERE CustomerID= @CustomerID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CustomerID", custId);
            cmd.Parameters.AddWithValue("@CompanyName", compName);

            conn.Open();
            int count = cmd.ExecuteNonQuery();
            conn.Close();

            btnLoadCustomers_Click(btnLoadCustomers, EventArgs.Empty);

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connstr);
            string custId = txtCustomerID.Text;

            string query = $"DELETE FROM Customers WHERE CustomerID=@CustomerID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CustomerID", custId);

            conn.Open();
            int count = cmd.ExecuteNonQuery();
            conn.Close();

            btnLoadCustomers_Click(btnLoadCustomers, EventArgs.Empty);
        }

        private void btnExecuteScalar_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connstr);
            string custId = txtCustomerID.Text;

            string query = $"SELECT COUNT(*) AS [COUNT] FROM Customers";

            SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();

            object result = cmd.ExecuteScalar();
            int count = (int)result;

            conn.Close();

            MessageBox.Show("Toplam Müşteri : " + count);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<Customer> customers = new List<Customer>();

            SqlConnection conn = new SqlConnection(connstr);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CustomerID, CompanyName, ContactTitle FROM Customers";

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Customer customer = new Customer()
                    {
                        CustomerID = (string)reader["CustomerID"],
                        CompanyName = reader.GetFieldValue<string>("CompanyName"),
                        ContactTitle = reader.IsDBNull("ContactTitle") ? null : reader.GetFieldValue<string>("ContactTitle")
                    };
                    customers.Add(customer);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata Oluştu!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed && conn.State != ConnectionState.Broken)
                {
                    conn.Close();
                }
            }

            cmbCustomers.DisplayMember = "CompanyName";
            cmbCustomers.ValueMember = "CustomerID";
            cmbCustomers.DataSource = customers;
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (cmbCustomers.SelectedIndex == -1)
            {
                MessageBox.Show("Müşteri seçmediniz..");
                return;
            }

            SqlConnection conn = new SqlConnection(connstr);

            string query = "SELECT * FROM Customers WHERE CustomerID = @CustomerID";
            SqlCommand cmd = new SqlCommand(query, conn);

            //Customer selected = cmbCustomers.SelectedItem as Customer;
            //cmd.Parameters.AddWithValue("@CustomerID", selected.CustomerID);

            cmd.Parameters.AddWithValue("@CustomerID", cmbCustomers.SelectedValue);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();

            adapter.Fill(table);

            dgvCustomers.DataSource = table;




        }
    }
}
