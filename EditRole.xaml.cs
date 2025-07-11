﻿using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Input;

namespace ManagerAppV4._0
{
    public partial class EditRole : Window
    {
        ConnectHelper CH = new ConnectHelper();
        public event Action RoleChanged; // событие на изменение или удаление роли

        public EditRole()
        {
            InitializeComponent();
            LoadRolesIntoComboBox();
        }
        private void LoadRolesIntoComboBox()
        {
            string query = "SELECT DISTINCT role FROM `roles` WHERE id != 0;";
            List<string> roles = new List<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                roles.Add(reader.GetString(0));
                        }
                    }
                }

                RoleComboBox.ItemsSource = roles;
                if (roles.Count > 0)
                    RoleComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки ролей: " + ex.Message);
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль для удаления.");
                return;
            }

            string selectedRole = RoleComboBox.SelectedItem.ToString();
            string query = "DELETE FROM `roles` WHERE role = @role;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@role", selectedRole);
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show($"Роль '{selectedRole}' успешно удалена.");
                            RoleChanged?.Invoke();
                            LoadRolesIntoComboBox();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: Роль не найдена.");
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении роли: " + ex.Message);
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(NewRoleNameTextBox.Text))
            {
                MessageBox.Show("Выберите роль и введите новое имя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string selectedRole = RoleComboBox.SelectedItem.ToString();
            string newRoleName = NewRoleNameTextBox.Text.Trim();
            string query = "UPDATE `roles` SET role = @newRole WHERE role = @oldRole;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@newRole", newRoleName);
                        cmd.Parameters.AddWithValue("@oldRole", selectedRole);
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show($"Роль '{selectedRole}' переименована в '{newRoleName}'.");
                            RoleChanged?.Invoke();
                            LoadRolesIntoComboBox();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: Роль не найдена.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при изменении роли: " + ex.Message);
            }
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow != null)
            {
                mainWindow.UpdateAll();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewRoleNameTextBox.Text))
            {
                try
                {
                    string query = $"INSERT INTO roles(role) VALUE(@role);";

                    using (MySqlConnection connection = new MySqlConnection(CH.GetConnectionString()))
                    {
                        var cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@role", NewRoleNameTextBox.Text);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Роль {NewRoleNameTextBox.Text} успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Вызов события для AddUser
                    RoleChanged?.Invoke();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при добавлении роли: " + ex.Message);
                }
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (mainWindow != null)
                {
                    mainWindow.UpdateAll();
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите название роли", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close(); // Закрываем текущее окно
            }
        }
    }
}
