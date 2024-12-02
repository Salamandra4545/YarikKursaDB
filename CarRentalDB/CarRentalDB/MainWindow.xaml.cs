using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace CarRentalApp
{
    public partial class MainWindow : Window
    {
        private DataSet dataSet = new DataSet();
        private SqlDataAdapter customersAdapter;
        private DataTable customersTable;
        private SqlDataAdapter subscriptionsAdapter;
        private DataTable subscriptionsTable;
        private SqlDataAdapter carsAdapter;
        private DataTable carsTable;
        private SqlDataAdapter rentalsAdapter;
        private DataTable rentalsTable;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            LoadFunctions();

            // Инициализация dataSet
            InitializeDataSet();
            EnableEditingForAllTables(); // Вызов метода для включения редактирования
        }

        private void LoadData()
        {
            var connectionString = GetConnectionString();

            // Загрузка данных о клиентах
            LoadCustomersData();

            // Загрузка данных о подписках
            LoadSubscriptionsData();

            // Загрузка данных о арендах
            string queryRentals = "SELECT RentalID, CarID, CustomerID, StartDate, EndDate, TotalAmount, Status, SubscriptionID FROM dbo.Rentals";
            rentalsAdapter = new SqlDataAdapter(queryRentals, connectionString);
            SqlCommandBuilder builder = new SqlCommandBuilder(rentalsAdapter);
            rentalsAdapter.InsertCommand = builder.GetInsertCommand();
            rentalsAdapter.UpdateCommand = builder.GetUpdateCommand();
            rentalsAdapter.DeleteCommand = builder.GetDeleteCommand();

            rentalsTable = new DataTable();
            rentalsAdapter.Fill(rentalsTable);

            RentalsDataGrid.ItemsSource = rentalsTable.DefaultView;

            // Загрузка данных о подписках клиентов
            string queryCustomerSubscriptions = "SELECT TOP (1000) CustomerSubscriptionID, CustomerID, SubscriptionID, StartDate, EndDate FROM dbo.CustomerSubscriptions";
            LoadDataToGrid(queryCustomerSubscriptions, connectionString, CustomerSubscriptionsDataGrid);

            // Загрузка данных о автомобилях
            LoadCarsData();
        }

        private void LoadCustomersData()
        {
            var connectionString = GetConnectionString();
            customersAdapter = new SqlDataAdapter("SELECT * FROM dbo.Customers", connectionString);
            SqlCommandBuilder builder = new SqlCommandBuilder(customersAdapter);

            customersTable = new DataTable();
            customersAdapter.Fill(customersTable);

            CustomersDataGrid.ItemsSource = customersTable.DefaultView;
        }

        private void LoadSubscriptionsData()
        {
            var connectionString = GetConnectionString();
            subscriptionsAdapter = new SqlDataAdapter("SELECT SubscriptionID, Name, MonthlyFee, DiscountRate FROM dbo.Subscriptions", connectionString);
            SqlCommandBuilder builder = new SqlCommandBuilder(subscriptionsAdapter);
            subscriptionsAdapter.InsertCommand = builder.GetInsertCommand();
            subscriptionsAdapter.UpdateCommand = builder.GetUpdateCommand();
            subscriptionsAdapter.DeleteCommand = builder.GetDeleteCommand();

            subscriptionsTable = new DataTable();
            subscriptionsAdapter.Fill(subscriptionsTable);

            SubscriptionsDataGrid.ItemsSource = subscriptionsTable.DefaultView;
        }

        private void LoadCarsData()
        {
            var connectionString = GetConnectionString();
            carsAdapter = new SqlDataAdapter("SELECT CarID, Make, Model, Year, Status, DailyRate FROM dbo.Cars", connectionString);
            SqlCommandBuilder builder = new SqlCommandBuilder(carsAdapter);
            carsAdapter.InsertCommand = builder.GetInsertCommand();
            carsAdapter.UpdateCommand = builder.GetUpdateCommand();
            carsAdapter.DeleteCommand = builder.GetDeleteCommand();

            carsTable = new DataTable();
            carsAdapter.Fill(carsTable);

            CarsDataGrid.ItemsSource = carsTable.DefaultView;
        }

        private void LoadDataToGrid(string query, string connectionString, DataGrid dataGrid)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dataGrid.ItemsSource = dataTable.DefaultView;
            }
        }

        private void LoadFunctions()
        {
            var connectionString = GetConnectionString();

            // Выполнение SQL-функций
            string activeRentalsQuery = "SELECT * FROM GetActiveRentalsByCustomer(1)";
            var activeRentals = ExecuteQuery(activeRentalsQuery, connectionString);

            string customerInfoQuery = "SELECT * FROM GetCustomerInfo(1)";
            var customerInfo = ExecuteQuery(customerInfoQuery, connectionString);

            string customerSubscriptionQuery = "SELECT * FROM GetCustomerSubscription(1)";
            var customerSubscription = ExecuteQuery(customerSubscriptionQuery, connectionString);

            string allSubscriptionsQuery = "SELECT * FROM GetAllSubscriptions()";
            var allSubscriptions = ExecuteQuery(allSubscriptionsQuery, connectionString);

            // Вывод результатов в ListBox
            FunctionsListBox.Items.Clear();
            FunctionsListBox.Items.Add("---- Результаты выполнения функций ----");

            FunctionsListBox.Items.Add("\n1. **Активные аренды клиента:**");
            FunctionsListBox.Items.Add(activeRentals);

            FunctionsListBox.Items.Add("\n2. **Информация о клиенте:**");
            FunctionsListBox.Items.Add(customerInfo);

            FunctionsListBox.Items.Add("\n3. **Информация о подписке клиента:**");
            FunctionsListBox.Items.Add(customerSubscription);

            FunctionsListBox.Items.Add("\n4. **Все доступные подписки:**");
            FunctionsListBox.Items.Add(allSubscriptions);
        }

        private string ExecuteScalarQuery(string query, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                var result = command.ExecuteScalar();
                return result?.ToString() ?? "No result";
            }
        }

        private string ExecuteQuery(string query, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                return FormatDataTable(dataTable);
            }
        }

        private string FormatDataTable(DataTable dataTable)
        {
            string formatted = "";
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    formatted += $"{col.ColumnName}: {row[col]}, ";
                }
                formatted = formatted.TrimEnd(',', ' ') + "\n";
            }
            return formatted;
        }

        private void SubscriptionsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Удаление отмены автоматического сохранения
            }
        }

        private void CustomersDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Удаление отмены автоматического сохранения
            }
        }

        private void RentalsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Удаление отмены автоматического сохранения
            }
        }

        private void CustomerSubscriptionsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Удаление отмены автоматического сохранения
            }
        }

        private void CarsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Удаление отмены автоматического сохранения
            }
        }

        private void UpdateDatabase(object rowItem, string tableName)
        {
            var connectionString = GetConnectionString();
            var updatedRow = rowItem as DataRowView;

            if (updatedRow != null)
            {
                var columns = updatedRow.Row.Table.Columns;
                var setClause = "";
                foreach (DataColumn column in columns)
                {
                    if (column.AutoIncrement) 
                    {
                        continue; // Пропустить автоинкрементные столбцы
                    }

                    var value = updatedRow[column.ColumnName];
                    setClause += $"{column.ColumnName} = '{value}', ";
                }
                setClause = setClause.TrimEnd(',', ' ');

                var primaryKey = updatedRow.Row[columns[0].ColumnName]; // Предположим, что первый столбец — это ключ

                string query = $"UPDATE {tableName} SET {setClause} WHERE {columns[0].ColumnName} = {primaryKey}";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is DataRowView selectedRow)
            {
                var customerId = selectedRow["CustomerID"];
                var connectionString = GetConnectionString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string deleteQuery = $"DELETE FROM Customers WHERE CustomerID = {customerId}";
                    SqlCommand command = new SqlCommand(deleteQuery, connection);
                    command.ExecuteNonQuery();
                }

                // Удаление из DataGrid
                (CustomersDataGrid.ItemsSource as DataView).Table.Rows.Remove(selectedRow.Row);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для удаления.");
            }
        }

        private void DeleteSelectedRows_Click(object sender, RoutedEventArgs e)
        {
            // Перебор всех DataGrid и удаление выбранных строк
            foreach (DataGrid dg in new DataGrid[] { CustomersDataGrid, SubscriptionsDataGrid, RentalsDataGrid, CustomerSubscriptionsDataGrid, CarsDataGrid })
            {
                DeleteSelectedRowsFromDataGrid(dg);
            }
        }

        private void DeleteSelectedRowsFromDataGrid(DataGrid dataGrid)
        {
            if (dataGrid.SelectedItems.Count > 0)
            {
                var rowsToDelete = dataGrid.SelectedItems.Cast<DataRowView>().ToList();
                foreach (var rowView in rowsToDelete)
                {
                    rowView.Row.Delete();
                }
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                customersAdapter.Update(customersTable);
                subscriptionsAdapter.Update(subscriptionsTable);
                rentalsAdapter.Update(rentalsTable);
                carsAdapter.Update(carsTable);
                // Добавлено обновление для всех таблиц

                MessageBox.Show("Изменения сохранены успешно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}");
            }
        }

        private void SaveSubscriptionsChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                subscriptionsAdapter.Update(subscriptionsTable);
                MessageBox.Show("Изменения в подписках сохранены успешно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений в подписках: {ex.Message}");
            }
        }

        private void SaveCarsChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                carsAdapter.Update(carsTable);
                MessageBox.Show("Изменения в автомобилях сохранены успешно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений в автомобилях: {ex.Message}");
            }
        }

        private void AddNewRecord(DataGrid dataGrid, string tableName)
        {
            DataTable dt = dataSet.Tables[tableName];
            DataRow newRow = dt.NewRow();
            // Здесь вы можете установить значения по умолчанию для новой строки
            dt.Rows.Add(newRow);
            dataGrid.ItemsSource = dt.DefaultView;

            if (tableName == "Customers")
            {
                customersAdapter.Update(dt);
            }
        }

        private void InitializeDataSet()
        {
            var connectionString = GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Загрузка данных в dataSet
                string[] tableNames = { "Customers", "Subscriptions", "Rentals", "CustomerSubscriptions", "Cars" };
                foreach (var tableName in tableNames)
                {
                    SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM {tableName}", connection);
                    adapter.Fill(dataSet, tableName);
                }
            }
        }

        private string GetConnectionString()
        {
            string configPath = @"C:\Users\User\Desktop\CarRentalDB\CarRentalDB\ConnectionSettings.json";
            if (File.Exists(configPath))
            {
                var configJson = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<dynamic>(configJson);
                return config.ConnectionString;
            }
            else
            {
                MessageBox.Show($"Config file not found at: {Path.GetFullPath(configPath)}");
                throw new FileNotFoundException($"Connection settings file not found at: {Path.GetFullPath(configPath)}");
            }
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            AddNewRecord(CustomersDataGrid, "Customers");
        }

        private void AddSubscription_Click(object sender, RoutedEventArgs e)
        {
            AddNewSubscription();
        }

        private void AddRental_Click(object sender, RoutedEventArgs e)
        {
            AddNewRecord(RentalsDataGrid, "Rentals");
        }

        private void AddCustomerSubscription_Click(object sender, RoutedEventArgs e)
        {
            AddNewRecord(CustomerSubscriptionsDataGrid, "CustomerSubscriptions");
        }

        private void AddCar_Click(object sender, RoutedEventArgs e)
        {
            AddNewCar();
        }

        private void CustomersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AddNewSubscription()
        {
            DataRow newRow = subscriptionsTable.NewRow();
            newRow["Name"] = "New Subscription"; 
            newRow["MonthlyFee"] = 0; 
            newRow["DiscountRate"] = 0; 
            subscriptionsTable.Rows.Add(newRow);
            newRow.EndEdit(); // Завершение редактирования новой строки
            SubscriptionsDataGrid.SelectedItem = newRow;
            SubscriptionsDataGrid.ScrollIntoView(newRow);
            SubscriptionsDataGrid.BeginEdit();
            subscriptionsAdapter.Update(subscriptionsTable); // Добавить вызов метода Update для сохранения изменений в базе данных
        }

        private void AddNewCar()
        {
            DataRow newRow = carsTable.NewRow();
            // Установите значения по умолчанию для новой строки
            carsTable.Rows.Add(newRow);
            carsAdapter.Update(carsTable);
        }

        private void EnableEditingForAllTables()
        {
            customersTable.PrimaryKey = new DataColumn[] { customersTable.Columns["CustomerID"] };
            subscriptionsTable.PrimaryKey = new DataColumn[] { subscriptionsTable.Columns["SubscriptionID"] };
            rentalsTable.PrimaryKey = new DataColumn[] { rentalsTable.Columns["RentalID"] };
            carsTable.PrimaryKey = new DataColumn[] { carsTable.Columns["CarID"] };
            // Добавьте первичные ключи для других таблиц, если необходимо

            customersTable.AcceptChanges();
            subscriptionsTable.AcceptChanges();
            rentalsTable.AcceptChanges();
            carsTable.AcceptChanges();
            // Примите изменения для других таблиц, если необходимо
        }
    }
}