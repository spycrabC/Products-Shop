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

namespace DAY1
{
    /// <summary>
    /// Логика взаимодействия для CategoryProduct.xaml
    /// </summary>
    public partial class CategoryProduct : Window
    {
        Kabinet2Entities3 bd = new Kabinet2Entities3();
        CLIENT cl;
        public CategoryProduct(CLIENT client)
        {
            cl = client;
            InitializeComponent();

            cbCategory.ItemsSource = bd.CATEGORY_PRODUCT.ToList();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectCategory = cbCategory.SelectedItem as CATEGORY_PRODUCT;
            
            if(selectCategory != null)
            {
                ProductsWindow productsWindow = new ProductsWindow(selectCategory, cl.PK_CLIENT);
                productsWindow.Show();
                this.Close();

            }
        }

        private void VkButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo{
                FileName = "https://vk.com",
                UseShellExecute = true
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BasketWindow basketWindow = new BasketWindow(cl.PK_CLIENT);
            basketWindow.Show();

           
        }
    }
}
