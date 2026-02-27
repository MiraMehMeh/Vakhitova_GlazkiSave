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
using System.Data.Entity;


namespace Vakhitova_GlazkiSave
{
    /// <summary>
    /// Логика взаимодействия для AgentPage.xaml
    /// </summary>
    public partial class AgentPage : Page
    {
        private List<Agent> _filteredAgents;
        private int pageSize = 10;
        private int currentPage = 1;

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


            // подписка на событие завершения навигации
            this.Loaded += (s, e) =>
            {
                var navService = NavigationService.GetNavigationService(this);
                if (navService != null)
                {
                    navService.Navigated += (sender, args) =>
                    {
                        // Если текущая страница снова стала активной (после GoBack)
                        if (args.Content == this)
                        {
                            UpdateAgents(); // перезагружаем данные из БД
                        }
                    };
                }
            };
        }


        private void UpdateAgents()
        {
            // все агенты с типом
            var currentAgents = Vakhitova_GlazkiSaveEntities.GetContext()
                                    .Agent.Include("AgentType")
                                    .Include("ProductSale")  // добавьте эту строку
                                    .ToList();

            // фильтрация по типу агента
            if (CBFilt.SelectedIndex == 1)
            {
                currentAgents = currentAgents.Where(a => a.AgentType.Title == "ЗАО").ToList();
            }

            if (CBFilt.SelectedIndex == 2)
            {
                currentAgents = currentAgents.Where(a => a.AgentType.Title == "МКК").ToList();
            }

            if (CBFilt.SelectedIndex == 3)
            {
                currentAgents = currentAgents.Where(a => a.AgentType.Title == "МФО").ToList();
            }

            if (CBFilt.SelectedIndex == 4)
            {
                currentAgents = currentAgents.Where(a => a.AgentType.Title == "ОАО").ToList();
            }

            if (CBFilt.SelectedIndex == 5)
            {
                currentAgents = currentAgents.Where(a => a.AgentType.Title == "ООО").ToList();
            }

            if (CBFilt.SelectedIndex == 6)
            {
                currentAgents = currentAgents.Where(a => a.AgentType.Title == "ПАО").ToList();
            }


            // поиск по агентам
            string searchText = TBSearch.Text.ToLower();
            // Очищаем поисковый запрос для телефона (убираем все лишние символы)
            string cleanedSearchPhone = searchText
                .Replace("+", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Replace("8", "7");

            currentAgents = currentAgents.Where(a =>
                    // Поиск по названию
                    (a.Title.ToLower().Contains(searchText)) ||

                    // Поиск по email
                    (a.Email.ToLower().Contains(searchText)) ||

                    // Поиск по телефону
                    (a.Phone
                        .Replace("+", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("-", "")
                        .Replace(" ", "")
                        .Replace("8", "7")
                        .StartsWith(cleanedSearchPhone))
                ).ToList();


            // сортировка агентов
            if (CBSort.SelectedIndex == 1)
            {
                currentAgents = currentAgents.OrderBy(a => a.Title).ToList();
            }

            if (CBSort.SelectedIndex == 2)
            {
                currentAgents = currentAgents.OrderByDescending(a => a.Title).ToList();
            }

            if (CBSort.SelectedIndex == 3)
            {
                currentAgents = currentAgents.OrderBy(a => a.Discount).ToList();
            }

            if (CBSort.SelectedIndex == 4)
            {
                currentAgents = currentAgents.OrderByDescending(a => a.Discount).ToList();
            }

            if (CBSort.SelectedIndex == 5)
            {
                currentAgents = currentAgents.OrderBy(a => a.Priority).ToList();
            }

            if (CBSort.SelectedIndex == 6)
            {
                currentAgents = currentAgents.OrderByDescending(a => a.Priority).ToList();
            }


            //вывод результата в ListView
            AgentListView.ItemsSource = currentAgents;



            _filteredAgents = currentAgents;
            currentPage = 1;
            ChangePage();
        }


        //функция отв за разделение listа
        private void ChangePage()
        {
            PageListBox.Items.Clear();

            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;

            for (int i = 1; i <= totalPages; i++)
            {
                PageListBox.Items.Add(i);
            }

            PageListBox.SelectedItem = currentPage;

            var agentsPage = _filteredAgents
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize).ToList();

            AgentListView.ItemsSource = agentsPage;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
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
            int maxPriority = 0;

            foreach (Agent selectedAgent in AgentListView.SelectedItems)
            {
                if (selectedAgent.Priority > maxPriority)
                    maxPriority = selectedAgent.Priority;
            }

            PriorChangeWindow prior = new PriorChangeWindow(maxPriority);
            prior.Owner = Window.GetWindow(this);
            prior.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            prior.ShowDialog();
            int newPriority = Convert.ToInt32(prior.TBPriority.Text);

            foreach (Agent agent in AgentListView.SelectedItems)
            {
                agent.Priority = newPriority;
            }

            try
            {
                Vakhitova_GlazkiSaveEntities.GetContext().SaveChanges();
                AgentListView.SelectedItems.Clear();
                UpdateAgents();
            }

            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddAgentBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            if (currentPage > 1)
            {
                currentPage--;
                ChangePage();
            }
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                ChangePage();
            }
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedItem is int page && page != currentPage)
            {
                currentPage = page;
                ChangePage();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(AgentListView.SelectedItem as Agent));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                // Получаем статический контекст
                var context = Vakhitova_GlazkiSaveEntities.GetContext();

                // Полная очистка кэша – отсоединяем все сущности
                foreach (var entry in context.ChangeTracker.Entries().ToList())
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                // Принудительно выполняем запрос, чтобы обновить контекст
                context.Agent.FirstOrDefault(); // любой запрос

                // Теперь вызываем UpdateAgents() – он загрузит данные заново
                UpdateAgents();

                // Дополнительно обновляем ListView (на всякий случай)
                AgentListView.Items.Refresh();
            }
        }
    }
}
