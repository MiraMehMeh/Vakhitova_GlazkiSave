using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
//using System.Windows.Shapes;
using System.IO;
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
using System.Windows.Navigation;


namespace Vakhitova_GlazkiSave
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        Agent currentAgent;

        Vakhitova_GlazkiSaveEntities _context; // вот здесь

        List<Product> _allProducts; // список всех продуктов
        List<ProductSale> _sales;    // текущие продажи агента 
        ICollectionView _productsView;

        public AddEditPage(Agent agent)
        {
            InitializeComponent();

            _context = new Vakhitova_GlazkiSaveEntities(); // создаём свой контекст

            // Загружаем все продукты для ComboBox
            _allProducts = _context.Product.ToList();
            _productsView = CollectionViewSource.GetDefaultView(_allProducts);
            ProductCombo.ItemsSource = _productsView;

            if (agent != null && agent.ID != 0) // существующий агент
            {
                // Загружаем агента из БД в новый контекст вместе с продажами и продуктами
                currentAgent = _context.Agent
                    .Include(a => a.ProductSale.Select(ps => ps.Product))
                    .FirstOrDefault(a => a.ID == agent.ID);

                if (currentAgent == null)
                {
                    // если не найден создаём нового
                    currentAgent = new Agent();
                    currentAgent.ProductSale = new List<ProductSale>();
                    DeleteBtn.Visibility = Visibility.Hidden;
                }
                else
                {
                    ComboType.SelectedIndex = currentAgent.AgentTypeID - 1;
                    DeleteBtn.Visibility = Visibility.Visible;
                    _sales = currentAgent.ProductSale.ToList();
                }
            }
            else // новый агент
            {
                currentAgent = new Agent();
                currentAgent.ProductSale = new List<ProductSale>();
                DeleteBtn.Visibility = Visibility.Hidden;
                _sales = new List<ProductSale>();
            }

            DataContext = currentAgent;
            SalesListView.ItemsSource = _sales;

            // подписка на TextChanged 
            ProductCombo.Loaded += (s, e) =>
            {
                if (ProductCombo.Template.FindName("PART_EditableTextBox", ProductCombo) is TextBox textBox)
                    textBox.TextChanged += ProductCombo_TextChanged;
            };
        }

        
        private void ProductCombo_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = ((TextBox)sender).Text;
            _productsView.Filter = obj =>
            {
                if (string.IsNullOrEmpty(filter)) return true;
                var product = obj as Product;
                return product.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
            };

            // Раскрываем список, если есть результаты и фильтр не пустой
            if (!string.IsNullOrEmpty(filter) && !_productsView.IsEmpty)
                ProductCombo.IsDropDownOpen = true;
        }

        private void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myOpenFileDialog = new OpenFileDialog();
            myOpenFileDialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*";

            if (myOpenFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFile = myOpenFileDialog.FileName;
                    if (!File.Exists(sourceFile))
                    {
                        MessageBox.Show("Исходный файл не найден.");
                        return;
                    }

                    // Папка назначения: imgs/agents в каталоге приложения
                    string imgsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imgs", "agents");
                    Directory.CreateDirectory(imgsFolder);

                    string fileName = Path.GetFileName(sourceFile);
                    string destPath = Path.Combine(imgsFolder, fileName);

                    // Если файл с таким именем уже существует – добавляем суффикс
                    int count = 1;
                    while (File.Exists(destPath))
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        string ext = Path.GetExtension(fileName);
                        string newName = $"{nameWithoutExt}_{count}{ext}";
                        destPath = Path.Combine(imgsFolder, newName);
                        count++;
                    }

                    File.Copy(sourceFile, destPath);

                    // Сохраняем только имя файла (без пути)
                    currentAgent.Logo = Path.GetFileName(destPath);

                    // Обновляем изображение на странице редактирования
                    LogoImage.Source = new BitmapImage(new Uri(destPath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при копировании файла: " + ex.Message);
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(currentAgent.Title))
                errors.AppendLine("Укажите наименование агента");

            if (string.IsNullOrWhiteSpace(currentAgent.Address))
                errors.AppendLine("Укажите адрес агента");

            if (string.IsNullOrWhiteSpace(currentAgent.DirectorName))
                errors.AppendLine("Укажите ФИО директора");

            if (ComboType.SelectedItem == null)
                errors.AppendLine("Укажите тип агента");
            else
                currentAgent.AgentTypeID = ComboType.SelectedIndex + 1;

            if (string.IsNullOrWhiteSpace(currentAgent.Priority.ToString()))
                errors.AppendLine("Укажите приоритет агента");

            if (currentAgent.Priority <= 0)
                errors.AppendLine("Укажите положительный приоритет агента");

            if (string.IsNullOrWhiteSpace(currentAgent.INN))
                errors.AppendLine("Укажите ИНН агента");

            if (string.IsNullOrWhiteSpace(currentAgent.KPP))
                errors.AppendLine("Укажите КПП агента");

            if (string.IsNullOrWhiteSpace(currentAgent.Phone))
                errors.AppendLine("Укажите телефон агента");

            else
            {
                // оставляем только цифры
                string digits = new string(currentAgent.Phone.Where(char.IsDigit).ToArray());

                // должно быть ровно 11 цифр (код страны + номер)
                if (digits.Length != 11)
                {
                    errors.AppendLine("Номер телефона должен содержать 11 цифр (с кодом страны)");
                }
            }

            if (string.IsNullOrWhiteSpace(currentAgent.Email))
                errors.AppendLine("Укажите почту агента");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (currentAgent.ID == 0)
                _context.Agent.Add(currentAgent);

            try
            {
                _context.SaveChanges();
                
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentAgent.SalesForYear > 0) // проверка наличия продаж
            {
                MessageBox.Show("Невозможно выполнить удаление", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы действительно хотите удалить агента?", "Внимание!",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Agent.Remove(currentAgent);
                    _context.SaveChanges();
                    Manager.MainFrame.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void AddSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(ProductCountBox.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Введите положительное целое число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (SaleDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получаем выбранный продукт
            Product selectedProduct = (Product)ProductCombo.SelectedItem;

            ProductSale newSale = new ProductSale
            {
                ProductID = selectedProduct.ID,
                ProductCount = count,
                SaleDate = SaleDatePicker.SelectedDate.Value
            };

            currentAgent.ProductSale.Add(newSale);
            newSale.Product = selectedProduct; // для отображения

            _sales.Add(newSale);
            SalesListView.Items.Refresh();

            // очистка
            ProductCountBox.Text = "";
            SaleDatePicker.SelectedDate = null;
            ProductCombo.SelectedItem = null;
        }

        private void DeleteSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            ProductSale sale = btn.Tag as ProductSale;
            if (sale != null)
            {
                _context.ProductSale.Remove(sale); // обязательно удаляем из контекста
                currentAgent.ProductSale.Remove(sale);
                _sales.Remove(sale);
                SalesListView.Items.Refresh();
            }
        }
    }
}
