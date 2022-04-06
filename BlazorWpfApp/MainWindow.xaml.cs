using Microsoft.Extensions.DependencyInjection;
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

namespace BlazorWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            services.AddBlazorWebView();
            BlazorView.Services = services.BuildServiceProvider();
            BlazorView.RootComponents.Add(new Microsoft.AspNetCore.Components.WebView.Wpf.RootComponent()
            {
                ComponentType = typeof(Main),
                Selector = "#app"
            });
            BlazorView.HostPage = "wwwroot\\Index.html";
        }
    }
}
