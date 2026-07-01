using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DAY1
{
    /// <summary>
    /// Логика взаимодействия для BasketWindow.xaml
    /// </summary>
    public partial class BasketWindow : Window
    {
        Kabinet2Entities3 bd = new Kabinet2Entities3();
        int id;
        int? finalPrice = 0;
        public BasketWindow(int idClient)
        {
            InitializeComponent();

            id = idClient;

            dgProducts.ItemsSource = bd.BASKET.Where(x => x.FK_CLIENT == id).ToList();


            BASKET[] basketArray = bd.BASKET.Where(x => x.FK_CLIENT == id).ToArray();

            FinalPrice(basketArray);

            lblPrice.Content = finalPrice;


        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            var selectProduct = dgProducts.SelectedItem as BASKET;
            if(selectProduct != null)
            {
                selectProduct.COUNT = (selectProduct.COUNT ?? 0) + 1;

                dgProducts.ItemsSource = bd.BASKET.Where(x => x.FK_CLIENT == id).ToList();

                BASKET[] basketArray = bd.BASKET.Where(x => x.FK_CLIENT == id).ToArray();

                finalPrice = 0;

                FinalPrice(basketArray);

                lblPrice.Content = finalPrice;

                bd.SaveChanges();
            }
            else
            {
                MessageBox.Show("Выберите товар");
            }
           

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var selectProduct = dgProducts.SelectedItem as BASKET;

            selectProduct.COUNT--;

            dgProducts.ItemsSource = bd.BASKET.Where(x => x.FK_CLIENT == id).ToList();

            BASKET[] basketArray = bd.BASKET.Where(x => x.FK_CLIENT == id).ToArray();

            finalPrice = 0;

            FinalPrice(basketArray);

            lblPrice.Content = finalPrice;

            if (selectProduct.COUNT == 0)
            {
                MessageBoxResult reuslt = MessageBox.Show("Вы точно хотите удалить товар из корзины?", "Предупреждение", MessageBoxButton.YesNo);

                if(reuslt == MessageBoxResult.Yes) 
                {
                    bd.BASKET.Remove(selectProduct);
                    bd.SaveChanges();

                    var updatedBasket = bd.BASKET.Where(x => x.FK_CLIENT == id).ToList();

                    dgProducts.ItemsSource = updatedBasket;


                    finalPrice = 0;

                    FinalPrice(basketArray);

                    lblPrice.Content = finalPrice;
                }
                else
                {
                    selectProduct.COUNT = 1;
                }
            }
            bd.SaveChanges();
        }

        private int? FinalPrice(BASKET[] basketArray)
        {
            for (int i = 0; i < basketArray.Length; i++)
            {
                var basket = basketArray[i];

                var selectProduct = bd.PRODUCT.FirstOrDefault(x => x.PK_PRODUCT == basket.FK_PRODUCT);

                if (selectProduct != null)
                {
                    finalPrice += (basket.COUNT ?? 0) * (selectProduct.PRICE ?? 0);
                }
            }
            return finalPrice;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OrderWindow orderWindow = new OrderWindow(id, finalPrice);
            this.Close();
            orderWindow.Show();
        }
    }

}
