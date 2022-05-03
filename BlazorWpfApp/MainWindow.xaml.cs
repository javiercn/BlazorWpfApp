using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
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
            services.AddWpfBlazorWebView();
            services.AddAuthorizationCore();

            // Option 1a
            services.AddScoped<AuthenticationStateProvider, ExternalAuthenticationStateProvider>();
            services.AddSingleton<ExternalAuthService>();

            // Option 1b
            services.AddScoped<AuthenticationStateProvider, CurrentThreadUserAuthenticationStateProvider>();
            BlazorView.Services = services.BuildServiceProvider();
            BlazorView.RootComponents.Add(new Microsoft.AspNetCore.Components.WebView.Wpf.RootComponent()
            {
                ComponentType = typeof(Main),
                Selector = "#app"
            });

            // Option 2
            services.AddScoped<AuthenticationStateProvider, ExternalUserAuthenticationStateProvider>();

            BlazorView.HostPage = "wwwroot\\Index.html";
        }
    }

    // Option 1a, the app logs in with whatever mechanism and raises an event
    public class ExternalAuthenticationStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _currentUser;

        public ExternalAuthenticationStateProvider(ExternalAuthService service)
        {
            _currentUser = new AuthenticationState(service.CurrentUser);

            service.UserChanged += (newUser) =>
            {
                _currentUser = new AuthenticationState(newUser);
                NotifyAuthenticationStateChanged(Task.FromResult(_currentUser));
            };
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(_currentUser);
    }

    public class ExternalAuthService
    {
        public event Action<ClaimsPrincipal> UserChanged;
        private ClaimsPrincipal _currentUser;

        public ClaimsPrincipal CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; UserChanged(_currentUser); }
        }
    }

    // Option 1b, the app logs in with whatever mechanism and sets the principal on the UI thread
    public class CurrentThreadUserAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(
                new AuthenticationState(
                    Thread.CurrentPrincipal as ClaimsPrincipal ?? new ClaimsPrincipal(new ClaimsIdentity())));
    }

    // Option 2, where the login/logout is managed within the webview, the auth provider is augmented with
    // additional methods to perform the log in/log out workflows and redirect.
    public class ExternalUserAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new AuthenticationState(_currentUser));

        public Task LogInAsync()
        {
            var loginTask = LogInAsyncCore();
            base.NotifyAuthenticationStateChanged(loginTask);

            return loginTask;

            async Task<AuthenticationState> LogInAsyncCore()
            {
                var user = await LoginWithExternalProviderAsync();
                _currentUser = user;
                return new AuthenticationState(_currentUser);
            }
        }

        private Task<ClaimsPrincipal> LoginWithExternalProviderAsync()
        {
            // Here is where you write your OpenID/MSAL code to authenticate the user.
            return Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
}
