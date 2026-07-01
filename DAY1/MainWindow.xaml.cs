using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DAY1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
   
    public partial class MainWindow : Window
    {
        Kabinet2Entities3 bd = new Kabinet2Entities3();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tbLogin.Text) && !string.IsNullOrWhiteSpace(pbPassword.Password))
                {
                    string enteredPasswordHash = Hash.HashPassword(pbPassword.Password);

                    var sign = bd.EMPLOYEE.FirstOrDefault(x => x.ACCESS_RIGHTS.LOGIN == tbLogin.Text && x.ACCESS_RIGHTS.PASSWORD == enteredPasswordHash);

                    var simpleUser = bd.CLIENT.FirstOrDefault(x => x.ACCESS_RIGHTS.LOGIN == tbLogin.Text && x.ACCESS_RIGHTS.PASSWORD == enteredPasswordHash);

                    if (sign != null)
                    {
                        if (sign.POST.NAME_POST == "Администратор")
                        {
                            AdminWindow adminWindow = new AdminWindow(sign);
                            adminWindow.Show();
                            this.Close();

                        }
                        else if(sign.POST.NAME_POST == "Сотрудник")
                        {
                            UserWindow userWindow = new UserWindow(sign);
                            userWindow.Show();
                            this.Close();
                        }
                    }
                    else if (simpleUser != null)
                    {
                        CategoryProduct categoryProduct = new CategoryProduct(simpleUser);
                        this.Close();
                        categoryProduct.Show();
                    }
                    else
                    {
                        MessageBox.Show("Неправильно введен пароль или логин");
                    }
                    
                }
                else
                {
                    MessageBox.Show("Пожалуйста, заполните все поля");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка:\n{ex}");
            }

        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if
                    (
                    !string.IsNullOrWhiteSpace(tbLogin2.Text) 
                    && !string.IsNullOrWhiteSpace(pbPassword2.Password) 
                    && !string.IsNullOrWhiteSpace(tbNumber.Text) 
                    && !string.IsNullOrWhiteSpace(dpBirthday.SelectedDate.ToString()) 
                    && (rbFemale.IsChecked == true || rbMale.IsChecked == true)
                    && !string.IsNullOrWhiteSpace (tbName.Text)
                    && !string.IsNullOrWhiteSpace(tbSurname.Text)
                    && !string.IsNullOrWhiteSpace(tbEmail.Text)
                    && !string.IsNullOrWhiteSpace(tbPatronymic.Text)
                   )

                {

                    if (Patterns.NumberCheck(tbNumber.Text))
                    {   
                        if(Patterns.EmailCheck(tbEmail.Text)) 
                        {
                            if(Patterns.PasswordCheck(pbPassword2.Password))
                            {
                                var newLogin = new ACCESS_RIGHTS
                                {
                                    LOGIN = tbLogin2.Text,
                                    PASSWORD = Hash.HashPassword(pbPassword2.Password)
                                };

                                bd.ACCESS_RIGHTS.Add(newLogin);
                                bd.SaveChanges();





                                var newClient = new CLIENT
                                {
                                    SURNAME = tbSurname.Text,
                                    NAME = tbName.Text,
                                    PATRONYMIC = tbPatronymic.Text,
                                    EMAIL = tbEmail.Text,
                                    NUMBER = tbNumber.Text,
                                    BIRTHDAY = dpBirthday.SelectedDate,
                                    GENDER = rbMale.IsChecked == true ? "М" : "Ж",
                                    FK_LOGIN = newLogin.LOGIN

                                };

                                bd.CLIENT.Add(newClient);
                                bd.SaveChanges();
                                if (!string.IsNullOrWhiteSpace(tbStreet.Text)
                                    || !string.IsNullOrWhiteSpace(tbHouseNumber.Text)
                                    || !string.IsNullOrWhiteSpace(tbRoomNumber.Text)
                                    || tbEntranceNumber.Value < 0
                                    || tbFloorNumber.Value < 0)
                                {

                                    var newAddress = new ADDRESS
                                    {
                                        FK_CLIENT = newClient.PK_CLIENT,
                                        STREET = string.IsNullOrWhiteSpace(tbStreet.Text) ? null : tbStreet.Text,
                                        HOUSE_NUMBER = string.IsNullOrWhiteSpace(tbHouseNumber.Text) ? null : tbHouseNumber.Text,
                                        ROOM_NUMBER = string.IsNullOrWhiteSpace(tbRoomNumber.Text) ? null : tbRoomNumber.Text,
                                        ENTRANCE_NUMBER = tbEntranceNumber.Value,
                                        FLOOR_NUMBER = tbFloorNumber.Value
                                    };

                                    bd.ADDRESS.Add(newAddress);
                                    bd.SaveChanges();

                                }

                                MessageBox.Show("Добро пожаловать");
                            }
                            else
                            {
                                MessageBox.Show("Пароль заполнен не по шаблону");
                            }
                          
                        }
                        else
                        {
                            MessageBox.Show("E-mail заполнен не по шаблону");
                        }
                        
                    }
                    else
                    {
                        MessageBox.Show("Номер телефона заполнен не по формату");
                    }
                   
                }
                else
                {
                    MessageBox.Show("Заполните обязательные поля.");
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Произошла ошибка: " + ex);
            }
        }

        private void tbNumber_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
