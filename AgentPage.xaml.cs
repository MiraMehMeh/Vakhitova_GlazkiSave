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

        int CountRecords; //кол-во записей в таблице 
        int CountPage; // общее кол-во страниц
        int CurrentPage = 0; // текущая стр

        List<Agent> CurrentPageList = new List<Agent>();
        List<Agent> TableList;

        //функция отв за разделение listа
        private void ChangePage(int direction, int? selectedPage)
        {
            //direction - направление. 0 - начало, 1 - пред стр
            // selectedPage - при нажатии на стрелочки передается null
            // при выборе опред стр в этой переменной находится номер стр

            CurrentPageList.Clear(); //начальная очистка листа
            CountRecords = TableList.Count;

            // определение кол-во стр
            if (CountRecords % 10 > 0)
            {
                CountPage = CountRecords / 10 + 1;
            }

            else
            {
                CountPage = CountRecords / 10;
            }

            Boolean Ifupdate = true;
            // проверка на правильность если
            //CurrentPage(номер текущ стр) "правильный"

            int min;

            if (selectedPage.HasValue) // проверка на значение не null (тк может быть null)
            {
                if (selectedPage >= 0 && selectedPage <= CountPage)
                {
                    CurrentPage = (int)selectedPage;
                    min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;

                    for (int i = CurrentPage * 10; i < min; i++)
                    {
                        CurrentPageList.Add(TableList[i]);
                    }
                }
            }

            else // если нажата стрелка
            {
                switch (direction)
                {
                    case 1: //нажата кнопка "пред стр"
                        if (CurrentPage > 0) // те кнопка нажата правильно и назад можно идти
                        {
                            CurrentPage--;

                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;

                            for (int i = CurrentPage * 10; i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }

                        else
                        {
                            Ifupdate = false; // в случае если 
                            //CurrentPage попытается выйти из диапзаона внесение данных не произойдет
                        }

                        break;

                    case 2: // нажата кнопка след стр
                        if (CurrentPage < CountPage - 1) // если вперед идти можно
                        {
                            CurrentPage++;

                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;

                            for (int i = CurrentPage * 10; i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }

                        else
                        {
                            Ifupdate = false;
                        }

                        break;
                }

            }

            if (Ifupdate) // если currentPage не вышел из диапзаона, то
            {
                PageListBox.Items.Clear();
                // удаление старых значений из листбокса номеров страниц, нужно чтобы при изменении
                // кол-ва записей кол-во стр динамически изменялось

                for (int i = 1; i <= CountPage; i++)
                {
                    PageListBox.Items.Add(i);
                }

                PageListBox.SelectedIndex = CurrentPage;

                AgentListView.ItemsSource = CurrentPageList;
                //обновить отображение списка агентов
                AgentListView.Items.Refresh();
            }
        }


        private void UpdateAgents()
        {
            // все агенты с типом
            var currentAgents = Vakhitova_GlazkiSaveEntities.GetContext()
                                    .Agent.Include("AgentType")
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

            // заполнение таблицы для постраничного вывода
            TableList = currentAgents;

            // вызов функции отображения кол-ва стр с параметрами:
            // направление 0 - нач загрузка
            // 0 - выбранная стр
            ChangePage(0, 0);
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

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }
    }
}
