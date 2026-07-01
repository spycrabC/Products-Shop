using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace DAY1
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        Kabinet2Entities3 bd = new Kabinet2Entities3();
        EMPLOYEE user;
        private string _selectedPhotoPath;
        public UserWindow(EMPLOYEE emp)
        {
            InitializeComponent();
            try {
                user = emp;

                var history = new HISTORY
                {
                    FK_EMPLOYEE = user.PK_EMPLOYEE,
                    DATE_TIME = DateTime.Now
                };

                bd.HISTORY.Add(history);
                bd.SaveChanges();

                tbEmail.Text = user.EMAIL;
                tbLogin.Text = user.ACCESS_RIGHTS.LOGIN;
                tbName.Text = user.NAME;
                tbNumber.Text = user.NUMBER;
                tbPatronymic.Text = user.PATRONYMIC;
                tbSeriaNumber.Text = user.SERIA_AND_NUMBER;
                tbSurname.Text = user.SURNAME;
                string fullPath = "";
                try {
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, user.PHOTO.TrimStart('/'));
                }
                catch {
                    fullPath = null;
                }
               

                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.EndInit();
                    imageEmployee.Source = bitmap;
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
            }
            
        }

        private void btnPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string appImagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

                    if (!Directory.Exists(appImagesDir))
                    {
                        Directory.CreateDirectory(appImagesDir);
                    }

                    string fileExeption = Path.GetExtension(openFileDialog.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExeption;

                    string destinationPath = Path.Combine(appImagesDir, uniqueFileName);

                    File.Copy(openFileDialog.FileName, destinationPath, true);

                    _selectedPhotoPath = uniqueFileName;

                    imageEmployee.Source = new BitmapImage(new Uri(destinationPath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке фото:\n{ex.Message}");
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbSurname.Text) ||
                    string.IsNullOrWhiteSpace(tbName.Text) ||
                    string.IsNullOrWhiteSpace(tbPatronymic.Text) ||
                    string.IsNullOrWhiteSpace(tbLogin.Text) ||
                    string.IsNullOrWhiteSpace(tbNumber.Text) ||
                    string.IsNullOrWhiteSpace(tbSeriaNumber.Text) ||
                    string.IsNullOrWhiteSpace(tbEmail.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все текстовые поля!");
                    return;
                }


                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(tbEmail.Text.Trim(), emailPattern))
                {
                    MessageBox.Show("Ваш email не соответствует стандарту (пример@домен.ru)");
                    return;
                }

                var currentUser = bd.EMPLOYEE.FirstOrDefault(x => x.PK_EMPLOYEE == user.PK_EMPLOYEE);

                if (currentUser == null)
                {
                    MessageBox.Show("Ошибка: Пользователь не найден в текущей сессии базы данных.");
                    return;
                }

                string inputPassword = tbPassword.Password;
                if (!string.IsNullOrWhiteSpace(inputPassword))
                {
                    if (!Patterns.PasswordCheck(inputPassword))
                    {
                        MessageBox.Show("Новый пароль не соответствует стандарту безопасности!");
                        return;
                    }
                    currentUser.ACCESS_RIGHTS.PASSWORD = Hash.HashPassword(inputPassword);
                }


                string newLogin = tbLogin.Text.Trim();
                if (currentUser.ACCESS_RIGHTS != null && currentUser.ACCESS_RIGHTS.LOGIN != newLogin)
                {

                    bool loginExists = bd.ACCESS_RIGHTS.Any(x => x.LOGIN == newLogin);
                    if (loginExists)
                    {
                        MessageBox.Show("Этот логин уже занят другим пользователем!");
                        return;
                    }

                    string currentPassword = currentUser.ACCESS_RIGHTS.PASSWORD;


                    var oldRights = currentUser.ACCESS_RIGHTS;
                    currentUser.ACCESS_RIGHTS = null;
                    bd.ACCESS_RIGHTS.Remove(oldRights);


                    bd.SaveChanges();


                    var newRights = new ACCESS_RIGHTS
                    {
                        LOGIN = newLogin,
                        PASSWORD = currentPassword
                    };


                    bd.ACCESS_RIGHTS.Add(newRights);
                    currentUser.ACCESS_RIGHTS = newRights;
                }


                currentUser.SURNAME = tbSurname.Text.Trim();
                currentUser.NAME = tbName.Text.Trim();
                currentUser.PATRONYMIC = tbPatronymic.Text.Trim();
                currentUser.SERIA_AND_NUMBER = tbSeriaNumber.Text.Trim();
                currentUser.NUMBER = tbNumber.Text.Trim();
                currentUser.EMAIL = tbEmail.Text.Trim();
                currentUser.PHOTO = _selectedPhotoPath;


                bd.SaveChanges();


                user = currentUser;

                MessageBox.Show("Данные успешно изменены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить изменения. Ошибка системы:\n{ex.Message}");
            }
        }
    }
}
