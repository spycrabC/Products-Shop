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
using Word = Microsoft.Office.Interop.Word;
using System.Data.Entity;
using MimeKit;

namespace DAY1
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        int idClient = 0;
        int price = 0;
        int discount = 0;
        int finalPrice = 0;
        Kabinet2Entities3 bd = new Kabinet2Entities3();
        public OrderWindow(int id, int? totalPrice)
        {
            InitializeComponent();

            idClient = id;
            price = (int)totalPrice; 

            tbPrice.Text = $"Стоимость без скидки: {price.ToString()} ₽";

            if (price > 1000)
            {
                discount = 5;
                finalPrice = (int)((1 - (discount / 100.0m)) * price); 
            }
            else
            {
                discount = 0;
                finalPrice = price; 
            }
        

            tbDiscount.Text = $"Скидка: {discount.ToString()} %";
            tbFinalPrice.Text = $"Итоговая стоимость: {finalPrice} ₽";


            var curretClient = bd.CLIENT.FirstOrDefault(x => x.PK_CLIENT == idClient);

            tbNumber.Text = curretClient.NUMBER;
            tbName.Text = curretClient.NAME;
            tbSurname.Text = curretClient.SURNAME;
            tbPatronymic.Text = curretClient.PATRONYMIC;
            tbEmail.Text = curretClient.EMAIL;

            var addressClient = bd.ADDRESS.FirstOrDefault(x => x.FK_CLIENT == curretClient.PK_CLIENT);

            if (addressClient != null)
            {
                tbStreet.Text = addressClient.STREET;

                tbHouseNumber.Value = int.TryParse(addressClient.HOUSE_NUMBER, out int house) ? house : 1;
                tbApartmentNumber.Value = int.TryParse(addressClient.ROOM_NUMBER, out int room) ? room : 1;
                tbEntranceNumber.Value = addressClient.ENTRANCE_NUMBER ?? 1;
                tbFloorNumber.Value = addressClient.FLOOR_NUMBER ?? 1;
            }
        }

        private void VkButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://vk.com",
                UseShellExecute = true
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(tbNumber.Text) 
               && !string.IsNullOrWhiteSpace(tbSurname.Text)
               && !string.IsNullOrWhiteSpace(tbName.Text)
               && !string.IsNullOrWhiteSpace(tbStreet.Text)
               && !string.IsNullOrWhiteSpace(tbPatronymic.Text)
               && tbHouseNumber.Value > 0
               && tbApartmentNumber.Value > 0
               )
            {
                var currectBasket = bd.BASKET.FirstOrDefault(x => x.FK_CLIENT == idClient);
                var newOrder = new ORDERS
                {
                    DATE_ORDER = DateTime.Now,
                    FK_CLIENT = idClient,
                    FINAL_PRICE = finalPrice
                };
                bd.ORDERS.Add(newOrder);
                bd.SaveChanges();
                var clientBasketItems = bd.BASKET.Include(x => x.PRODUCT).Where(x => x.FK_CLIENT == idClient).ToList();

                foreach (var basketItem in clientBasketItems)
                {
                    var newProductInOrder = new PRODUCT_IN_ORDER
                    {
                        FK_ORDERS = newOrder.PK_ORDERS,
                        FK_PRODUCT = basketItem.FK_PRODUCT,
                        COUNT = (int)basketItem.COUNT,
                        PRICE_AT_PURCHASE = (int)basketItem.PRODUCT.PRICE
                    };

                    bd.PRODUCT_IN_ORDER.Add(newProductInOrder);
 
                }


                Word.Application winword = null;
                Word.Document document = null;

                try
                {
                    string templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Шаблон чека.docx");

                    winword = new Word.Application();

                    document = winword.Documents.Open(templatePath, ReadOnly: true);

                    ReplaceWordStub("{OrderNumber}", newOrder.PK_ORDERS.ToString(), document);
                    ReplaceWordStub("{Date}", newOrder.DATE_ORDER?.ToString("dd.MM.yyyy HH:mm"), document);
                    ReplaceWordStub("{ClientName}", $"{tbSurname.Text} {tbName.Text}", document);
                    ReplaceWordStub("{FinalPrice}", $"{this.finalPrice} ₽", document);
                    ReplaceWordStub("{Discount}", $"{discount} %", document);
                    int totalCount = clientBasketItems.Sum(x => (int)(x.COUNT ?? 0));

                    Word.Range tableRange = document.Content;
                    tableRange.Find.ClearFormatting();
                    if (tableRange.Find.Execute(FindText: "{Product}"))
                    {
                        int rowsCount = clientBasketItems.Count + 1;
                        Word.Table productTable = document.Tables.Add(tableRange, rowsCount, 2);
                        productTable.Borders.Enable = 1;

                        productTable.Cell(1, 1).Range.Text = "Наименование товара";
                        productTable.Cell(1, 2).Range.Text = "Кол-во";
                        productTable.Rows[1].Range.Bold = 1;

                        int rowIndex = 2;
                        foreach (var basketItem in clientBasketItems)
                        {
                            string productName = basketItem.PRODUCT?.NAME_PRODUCT ?? "Товар";
                            string countText = $"{basketItem.COUNT} шт.";

                            productTable.Cell(rowIndex, 1).Range.Text = productName;
                            productTable.Cell(rowIndex, 2).Range.Text = countText;

                            rowIndex++;
                        }
                    }

                    string savePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Чек_{newOrder.PK_ORDERS}.docx");

                    document.SaveAs2(savePath);
                    document.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
                    document = null; 


                    bd.BASKET.RemoveRange(clientBasketItems);
                    bd.SaveChanges();


                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Магазин 'всякая всячена'", "makar.evseev@yandex.ru"));
                    message.To.Add(new MailboxAddress($"{tbSurname.Text} {tbName.Text}", $"{tbEmail.Text}"));
                    message.Subject = "Мы вас любим";

                    var builder = new BodyBuilder();
                    builder.TextBody = "Многоважаемый Макар Сергеевич, приветствуем вас. Вот ваш чек";
                    builder.Attachments.Add(savePath);
                    message.Body = builder.ToMessageBody();

                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.Connect("smtp.yandex.ru", 465, true);
                        client.Authenticate("makar.evseev@yandex.ru", "elxhfbhdbnwfslep");
                        client.Send(message);
                        client.Disconnect(true);
                    }

                    MessageBox.Show("Заказ успешно создан");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при заполнении шаблона: {ex.Message}");
                }
                finally
                {

                    if (document != null)
                    {
                        try { document.Close(Word.WdSaveOptions.wdDoNotSaveChanges); } catch { }
                    }
                    if (winword != null)
                    {
                        try { winword.Quit(); } catch { }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(winword);
                    }
                }


            }
            else
            {
                MessageBox.Show("Все поля, кроме подъезда и этажа являются обязательными");
            }
        }
        private void ReplaceWordStub(string stubToReplace, string textToReplace, Microsoft.Office.Interop.Word.Document doc)
        {
            var range = doc.Content;
            range.Find.ClearFormatting();
            range.Find.Execute(
                FindText: stubToReplace,
                ReplaceWith: textToReplace,
                Replace: Microsoft.Office.Interop.Word.WdReplace.wdReplaceAll
            );
        }
    }
}
