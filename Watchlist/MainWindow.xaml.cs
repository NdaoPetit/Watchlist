using System;
using System.ComponentModel;
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

namespace Watchlist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WatchlistSource source;
        public MainWindow()
        {
            InitializeComponent();
            source = new WatchlistSource();
            dgWatchlist.ItemsSource = source;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var ans = MessageBox.Show("Do you want to quit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ans == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var ticker = tbxTicker.Text;
            if (!string.IsNullOrEmpty(ticker))
            {
                ticker = ticker.ToUpper();
                var stock = source.Where(s => s.Ticker.Equals(ticker)).FirstOrDefault();
                if (stock != null)
                {
                    MessageBox.Show("Watchlist already contains " + ticker);
                }
                else
                {
                    stock = new Stock(ticker);
                    await stock.Connect();
                    source.Add(stock);
                }
                tbxTicker.Clear();
            }
            else
            {
                MessageBox.Show("Enter stock ticker", "Add stock", MessageBoxButton.OK);
            }
        }
    }
}
