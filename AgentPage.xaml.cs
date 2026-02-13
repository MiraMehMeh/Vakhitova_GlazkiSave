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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vakhitova_GlazkiSave
{
    /// <summary>
    /// Логика взаимодействия для AgentPage.xaml
    /// </summary>
    public partial class AgentPage : Page
    {
        public AgentPage()
        {
            InitializeComponent();

            //добавляем строки
            // загрузить в список из бд
            var currentAgents = Vakhitova_GlazkiSaveEntities.GetContext().Agent.ToList();
            // связываем с листвью
            AgentListView.ItemsSource = currentAgents;

            CBSort.SelectedIndex = 0;
            CBFilt.SelectedIndex = 0;

            UpdateAgents();
            
        }

        private void UpdateAgents()
        {
            // все агенты с типом
            var currentAgents = Vakhitova_GlazkiSaveEntities.GetContext()
                                    .Agent.Include("AgentType")
                                    .ToList();

            // фильтрация по типу агента
            string selectedType = (CBFilt.SelectedItem as TextBlock)?.Text;
            if (!string.IsNullOrEmpty(selectedType) && selectedType != "Все типы")
            {
                currentAgents = currentAgents
                    .Where(a => a.AgentType != null && a.AgentType.Title == selectedType)
                    .ToList();
            }

            // поиск по агентам
            if (!string.IsNullOrEmpty(TBSearch.Text))
            {
                string searchText = TBSearch.Text.ToLower();
                searchText = searchText.Replace("ё", "е").Replace("Ё", "Е");

                // Очищаем поисковый запрос для телефона (убираем все лишние символы)
                string cleanedSearchPhone = searchText
                    .Replace("+", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Replace("8", "7");

                currentAgents = currentAgents.Where(p =>
                    // Поиск по названию
                    (p.Title != null && p.Title.ToLower().Replace("ё", "е").Contains(searchText)) ||

                    // Поиск по email
                    (p.Email != null && p.Email.ToLower().Contains(searchText)) ||

                    // Поиск по телефону
                    (p.Phone != null && p.Phone
                        .Replace("+", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("-", "")
                        .Replace(" ", "")
                        .Replace("8", "7")
                        .Contains(cleanedSearchPhone))
                ).ToList();
            }

            // сортировка агентов
            string sortOption = (CBSort.SelectedItem as TextBlock)?.Text;
            if (!string.IsNullOrEmpty(sortOption) && sortOption != "Сортировка")
            {
                switch (sortOption)
                {
                    case "Наименование по возрастанию":
                        currentAgents = currentAgents.OrderBy(a => a.Title).ToList();
                        break;
                    case "Наименование по убыванию":
                        currentAgents = currentAgents.OrderByDescending(a => a.Title).ToList();
                        break;
                    case "Скидка по возрастанию":
                        currentAgents = currentAgents.OrderBy(a => a.Discount).ToList();
                        break;
                    case "Скидка по убыванию":
                        currentAgents = currentAgents.OrderByDescending(a => a.Discount).ToList();
                        break;
                    case "Приоритет по возрастанию":
                        currentAgents = currentAgents.OrderBy(a => a.Priority).ToList();
                        break;
                    case "Приоритет по убыванию":
                        currentAgents = currentAgents.OrderByDescending(a => a.Priority).ToList();
                        break;
                }
            }

            //вывод результата в ListView
            AgentListView.ItemsSource = currentAgents;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void AgentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AgentListView.SelectedItems.Count > 0)
            {
                ChangePriorityBtn.Visibility = Visibility.Visible;
            }

            else
            {
                ChangePriorityBtn.Visibility= Visibility.Hidden;
            }
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void CBSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void CBFilt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void ChangePriorityBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddAgentBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
