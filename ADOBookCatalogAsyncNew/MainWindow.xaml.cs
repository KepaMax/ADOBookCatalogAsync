﻿using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ADOBookCatalogAsyncNew;

public partial class MainWindow : Window
{
    SqlConnection? connection = null;
    SqlDataAdapter? adapter = null;
    DataTable? table = null;
    public MainWindow()
    {
        InitializeComponent();
        Configuration();

        void Configuration()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            connection = new SqlConnection();
            connection.ConnectionString = configuration.GetConnectionString("Library");
            adapter = new SqlDataAdapter(string.Empty, connection);
            table = new();

            table.Columns.Add("Id");
            table.Columns.Add("Name");
            table.Columns.Add("Pages");
            table.Columns.Add("YearPress");
            table.Columns.Add("Id_Author");
            table.Columns.Add("Id_Themes");
            table.Columns.Add("Id_Category");
            table.Columns.Add("Id_Press");
            table.Columns.Add("Comment");
            table.Columns.Add("Quantity");
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SqlDataReader? result = null;


        try
        {
            connection?.Open();

            using SqlCommand command = new SqlCommand("WAITFOR DELAY '00:00:04'; SELECT * FROM Authors;", connection);
            result = await command.ExecuteReaderAsync();

            while (result.Read())
            {

                int? id = result["Id"] as int?;
                string? firstName = result["FirstName"] as string;
                string? lastName = result["LastName"] as string;

                AuthorsCombobox.Items.Add(id + " " + firstName + " " + lastName);
            }

            MessageBox.Show("Authors Loaded");

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
            result?.Close();
        }
    }

    private async void AuthorsCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CategoriesCombobox.IsEnabled)
            CategoriesCombobox.IsEnabled = !CategoriesCombobox.IsEnabled;

        CategoriesCombobox.Items.Clear();

        SqlDataReader? result = null;

        try
        {
            connection?.Open();

            var id = AuthorsCombobox.SelectedItem.ToString()?.Split(' ')[0];

            if (id is null)
                return;

            using SqlCommand command = new SqlCommand($"WAITFOR DELAY '00:00:02'; SELECT DISTINCT Categories.[Name] FROM Categories\r\nJOIN Books ON Id_Category = Categories.Id\r\nJOIN Authors ON Id_Author = Authors.Id\r\nWHERE Authors.Id = {id}", connection);
            result = await command.ExecuteReaderAsync();

            while (result.Read())
                CategoriesCombobox.Items.Add(result["Name"] as string);

            MessageBox.Show("Categories Loaded");

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
            result?.Close();
        }
    }

    private async void CategoriesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoriesCombobox.Items.IsEmpty || adapter is null)
            return;
        SqlDataReader? result = null;

        try
        {
            connection?.Open();

            var id = AuthorsCombobox.SelectedItem.ToString()?.Split(' ')[0];
            var name = CategoriesCombobox.SelectedItem.ToString();

            using SqlCommand command = new SqlCommand($"WAITFOR DELAY '00:00:02'; SELECT Books.Id, Books.Name, Pages, YearPress, Id_Themes, Id_Category, Id_Author, Id_Press, Comment, Quantity FROM Books\r\nJOIN Categories ON Categories.Id = Id_Category \r\nJOIN Authors ON Authors.Id = Id_Author \r\nWHERE Categories.Name = '{name}' AND Id_Author = {id}\r\n", connection);

            result = await command.ExecuteReaderAsync();

            while (result.Read())
            {
                var row = table?.NewRow();

                if (row != null)
                {
                    row["Id"] = result["Id"];
                    row["Name"] = result["Name"];
                    row["Pages"] = result["Pages"];
                    row["YearPress"] = result["YearPress"];
                    row["Id_Author"] = result["Id_Author"];
                    row["Id_Themes"] = result["Id_Themes"];
                    row["Id_Category"] = result["Id_Category"];
                    row["Id_Press"] = result["Id_Press"];
                    row["Comment"] = result["Comment"];
                    row["Quantity"] = result["Quantity"];

                    table?.Rows.Add(row);
                }
            }

            BooksGrid.ItemsSource = table?.AsDataView();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
            result?.Close();
        }
    }

    private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchTxt.Text) || connection?.State is ConnectionState.Open)
            return;

        SqlDataReader? result = null;

        table?.Rows.Clear();

        try
        {
            connection?.Open();

            using SqlCommand command = new SqlCommand($"WAITFOR DELAY '00:00:02'; SELECT * FROM Books\r\nWHERE Name LIKE '%{SearchTxt.Text}%'", connection);
            result = await command.ExecuteReaderAsync();

            while (result.Read())
            {
                var row = table?.NewRow();

                if (row != null)
                {
                    row["Id"] = result["Id"];
                    row["Name"] = result["Name"];
                    row["Pages"] = result["Pages"];
                    row["YearPress"] = result["YearPress"];
                    row["Id_Author"] = result["Id_Author"];
                    row["Id_Themes"] = result["Id_Themes"];
                    row["Id_Category"] = result["Id_Category"];
                    row["Id_Press"] = result["Id_Press"];
                    row["Comment"] = result["Comment"];
                    row["Quantity"] = result["Quantity"];

                    table?.Rows.Add(row);
                }

            }
            BooksGrid.ItemsSource = table?.AsDataView();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
            result?.Close();
        }
    }
}
