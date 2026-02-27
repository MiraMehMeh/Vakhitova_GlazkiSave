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

namespace Vakhitova_GlazkiSave
{
    /// <summary>
    /// Логика взаимодействия для PriorChangeWindow.xaml
    /// </summary>
    public partial class PriorChangeWindow : Window
    {
        public int NewPriority { get; private set; }

        public PriorChangeWindow(int maxPriority)
        {
            InitializeComponent();
            TBPriority.Text = maxPriority.ToString();
        }

        private void ChangePrior_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TBPriority.Text, out int newPriority) && newPriority > 0)
            {
                NewPriority = newPriority;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите целое  положительное число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
