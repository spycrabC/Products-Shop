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
using System.Windows.Shapes;
using System.Linq;

namespace DAY1
{
    /// <summary>
    /// Логика взаимодействия для ProductsWindow.xaml
    /// </summary>
    public partial class ProductsWindow : Window
    {
        Kabinet2Entities3 bd = new Kabinet2Entities3();
        private int count = 0;
        int id = 0;
        public ProductsWindow(CATEGORY_PRODUCT category, int idClient)
        {
            id = idClient;
            InitializeComponent();
            dgProducts.ItemsSource = bd.PRODUCT.Where(x => x.FK_CATEGORY_PRODUCT == category.PK_CATEGORY_PRODUCT).ToList();



        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            count = 0;
            btnBasket.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = dgProducts.SelectedItem as PRODUCT;

            if (selectedProduct != null)
            {
                var existingBasketItem = bd.BASKET.FirstOrDefault(x => x.FK_CLIENT == id && x.FK_PRODUCT == selectedProduct.PK_PRODUCT);

                if (existingBasketItem != null)
                {
                    existingBasketItem.COUNT = (existingBasketItem.COUNT ?? 0) + 1;

                    lblCount.Content = existingBasketItem.COUNT;
                }
                else
                {
                    var newProductInBasket = new BASKET
                    {
                        FK_CLIENT = id,
                        FK_PRODUCT = selectedProduct.PK_PRODUCT,
                        COUNT = 1
                    };
                    bd.BASKET.Add(newProductInBasket);

                    lblCount.Content = 1;
                }


                bd.SaveChanges();
            }
            else
            {
                MessageBox.Show("Выберите товар");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedProduct = dgProducts.SelectedItem as PRODUCT;
            if (selectedProduct == null)
            {
                count = 0;
                lblCount.Content = count;

                MessageBox.Show("Пожалуйста, выберите товар!");
                
                return;
            }

           
            var productInBasket = bd.BASKET.FirstOrDefault(b => b.FK_CLIENT == id && b.FK_PRODUCT == selectedProduct.PK_PRODUCT);

            if (productInBasket != null)
            {

                productInBasket.COUNT--;


                if (productInBasket.COUNT <= 0)
                {
                    bd.BASKET.Remove(productInBasket);
                    count = 0; 
                }
                else
                {
                    count = (int)productInBasket.COUNT;
                }
                bd.SaveChanges();
            }
            else
            {
               
                count = 0;
            }


            lblCount.Content = count;

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
           
            btnBasket.IsEnabled = false;

           

            BasketWindow basketWindow = new BasketWindow(id);
            basketWindow.Show();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var client = bd.CLIENT.FirstOrDefault(x => x.PK_CLIENT == id);
            CategoryProduct category = new CategoryProduct(client);
            this.Hide();
            category.Show();
        }
    }
}
