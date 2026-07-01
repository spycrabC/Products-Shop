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
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {

        Kabinet2Entities3 bd = new Kabinet2Entities3();
        private string _selectedPhotoPath = null;
        public AdminWindow(EMPLOYEE admin)
        {
            InitializeComponent();

            var history = new HISTORY 
            { 
                FK_EMPLOYEE = admin.PK_EMPLOYEE,
                DATE_TIME = DateTime.Now
            };

            bd.HISTORY.Add(history);
            bd.SaveChanges();


            lbEmployee.ItemsSource = bd.EMPLOYEE.ToList();
            lbHistory.ItemsSource = bd.HISTORY.ToList();

            cbPost.ItemsSource = bd.POST.ToList();
            cbSubd.ItemsSource = bd.SUBDVISION.ToList();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectEmployee = lbEmployee.SelectedItem as EMPLOYEE;

                if (Patterns.EmailCheck(tbEmail.Text))
                {
                    selectEmployee.EMAIL = tbEmail.Text;
                }
                else
                {
                    MessageBox.Show("email заполнен не по формату");
                    return;
                }

                selectEmployee.SURNAME = tbSurname.Text;
                selectEmployee.NAME = tbName.Text;
                selectEmployee.PATRONYMIC = tbPatronymic.Text;
                
               
                selectEmployee.SERIA_AND_NUMBER = tbSeriaNumber.Text;
                selectEmployee.NUMBER = tbNumber.Text;
                selectEmployee.POST = cbPost.SelectedItem as POST;
                selectEmployee.SUBDVISION = cbSubd.SelectedItem as SUBDVISION;

                if (!string.IsNullOrEmpty(_selectedPhotoPath))
                {
                    selectEmployee.PHOTO = _selectedPhotoPath;
                }

                if (selectEmployee != null)
                {
                    if(Patterns.PasswordCheck(tbPassword.Password) )
                    {
                        if (selectEmployee.ACCESS_RIGHTS != null && selectEmployee.ACCESS_RIGHTS.LOGIN != tbLogin.Text)
                        {
                            string currentPassword = string.IsNullOrWhiteSpace(tbPassword.Password)
                                ? selectEmployee.ACCESS_RIGHTS.PASSWORD
                                : Hash.HashPassword(tbPassword.Password);


                            bd.ACCESS_RIGHTS.Remove(selectEmployee.ACCESS_RIGHTS);

                            var newRights = new ACCESS_RIGHTS
                            {
                                LOGIN = tbLogin.Text,
                                PASSWORD = currentPassword
                            };

                            bd.ACCESS_RIGHTS.Add(newRights);
                            selectEmployee.ACCESS_RIGHTS = newRights;
                        }
                        else if (selectEmployee.ACCESS_RIGHTS != null)
                        {

                            if (!string.IsNullOrWhiteSpace(tbPassword.Password))
                            {
                                selectEmployee.ACCESS_RIGHTS.PASSWORD = Hash.HashPassword(tbPassword.Password);
                            }
                        }
                        else
                        {

                            var newRights = new ACCESS_RIGHTS
                            {
                                LOGIN = tbLogin.Text,
                                PASSWORD = Hash.HashPassword(tbPassword.Password)
                            };
                            bd.ACCESS_RIGHTS.Add(newRights);
                            selectEmployee.ACCESS_RIGHTS = newRights;
                        }
                        bd.SaveChanges();

                        MessageBox.Show("Сотрудник успешно изменен");
                    }
                    else
                    {
                        MessageBox.Show("Пароль должен состоять больше 8 символов, написан на латинице, а так же иметь цифры");
                    }
                    

                }
                else
                {
                    MessageBox.Show("Не выбран пользователь");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка:\n {ex}");
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lbEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectEmployee = lbEmployee.SelectedItem as EMPLOYEE;

            if (selectEmployee != null)
            {
                tbSurname.Text = selectEmployee.SURNAME;
                tbName.Text = selectEmployee.NAME;
                tbPatronymic.Text = selectEmployee.PATRONYMIC;
                tbEmail.Text = selectEmployee.EMAIL;
                tbSeriaNumber.Text = selectEmployee.SERIA_AND_NUMBER;
                cbPost.SelectedItem = selectEmployee.POST;
                cbSubd.SelectedItem = selectEmployee.SUBDVISION;
                tbEmail.Text = selectEmployee.EMAIL;
                tbNumber.Text = selectEmployee.NUMBER;
                tbLogin.Text = selectEmployee.ACCESS_RIGHTS.LOGIN;
                imageEmployee.Source = null;

                if (!string.IsNullOrEmpty(selectEmployee.PHOTO))
                {
                    try
                    {
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectEmployee.PHOTO.TrimStart('/'));

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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка визуализации фото: {ex.Message}");
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!string.IsNullOrWhiteSpace(tbSurname.Text) || !string.IsNullOrWhiteSpace(tbName.Text) ||
                    !string.IsNullOrWhiteSpace(tbPatronymic.Text) || !string.IsNullOrWhiteSpace(tbLogin.Text) || !string.IsNullOrWhiteSpace(tbNumber.Text)
                    || !string.IsNullOrWhiteSpace(tbSeriaNumber.Text) || !string.IsNullOrWhiteSpace(tbEmail.Text) || !string.IsNullOrWhiteSpace(cbPost.Text)
                    || !string.IsNullOrWhiteSpace(cbSubd.Text))
                {


                    var post = cbPost.SelectedItem as POST;
                    var subd = cbSubd.SelectedItem as SUBDVISION;


                    if (Patterns.PasswordCheck(tbPassword.Password))
                    {
                        if(Patterns.EmailCheck(tbEmail.Text))
                        {
                            var notUser = bd.ACCESS_RIGHTS.FirstOrDefault(x => x.LOGIN == tbLogin.Text);


                            if(notUser == null)
                            {
                                var addRights = new ACCESS_RIGHTS
                                {
                                    LOGIN = tbLogin.Text,
                                    PASSWORD = Hash.HashPassword(tbPassword.Password)
                                };

                                bd.ACCESS_RIGHTS.Add(addRights);
                                bd.SaveChanges();

                                var addEmployee = new EMPLOYEE
                                {
                                    SURNAME = tbSurname.Text,
                                    NAME = tbName.Text,
                                    PATRONYMIC = tbPatronymic.Text,
                                    NUMBER = tbNumber.Text,
                                    SERIA_AND_NUMBER = tbSeriaNumber.Text,
                                    EMAIL = tbEmail.Text,
                                    FK_POST = post.PK_POST,
                                    FK_SUBDVISION = subd.PK_SUBDVISION,
                                    PHOTO = _selectedPhotoPath,
                                    ACCESS_RIGHTS = addRights
                                };

                                bd.EMPLOYEE.Add(addEmployee);
                                bd.SaveChanges();

                                MessageBox.Show("Сотрудник успешно добавлен!");

                                lbEmployee.ItemsSource = bd.EMPLOYEE.ToList();
                            }
                            else
                            {
                                MessageBox.Show("Такой пользователь уже есть");
                            }
                            
                        }
                        else
                        {
                            MessageBox.Show("email заполнен не по шаблону");
                        }
                        
                    }
                    else
                    {
                        MessageBox.Show("Заполните все поля перед регистрацией сотрудника");
                    }
                }
                else
                {
                    MessageBox.Show("Пароль должен состоять больше 8 символов, написан на латинице, а так же иметь цифры");
                }
                    
            }
            catch
            {
                MessageBox.Show("Произошла ошибка");
            }

            
        }

        private void btnPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if(openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string appImagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

                    if(!Directory.Exists(appImagesDir)) 
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

        private void ApplyHistoryFilter()
        {
            var selectEmployee = lbEmployee.SelectedItem as EMPLOYEE;

            if (selectEmployee == null) return;

            
            string searchText = tbHistorySearch.Text.Trim().ToLower();


            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var query = bd.HISTORY.Where(x => x.EMPLOYEE.SURNAME.ToLower().Contains(searchText) ||
                                     x.EMPLOYEE.NAME.ToLower().Contains(searchText));
                lbHistory.ItemsSource = query.ToList();
            }

            if (dpDateFilter.SelectedDate.HasValue)
            {
                DateTime selectedDate = dpDateFilter.SelectedDate.Value.Date;

                var query = bd.HISTORY.Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.DATE_TIME) == selectedDate);
                lbHistory.ItemsSource = query.ToList();
            }

            
        }

        private void tbHistorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyHistoryFilter();
        }

        private void dpDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyHistoryFilter();
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lbEmployee.SelectedIndex = -1;
           
        }
    }
}
