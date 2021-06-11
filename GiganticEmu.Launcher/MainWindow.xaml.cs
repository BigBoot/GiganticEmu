using GiganticEmu.Shared;
using MaterialDesignExtensions.Controls;
using System;
using System.Windows;
using Refit;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace GiganticEmu.Launcher
{
    public partial class MainWindow : MaterialWindow
    {
        private IApi _api;
        private AuthHeaderHandler _auth;

        public MainWindow()
        {
            InitializeComponent();
            _auth = new AuthHeaderHandler();
            _api = RestService.For<IApi>(new HttpClient(_auth)
            {
                BaseAddress = new Uri("http://localhost:3000")
            });
        }

        private void ShowRegisterPage(object sender, RoutedEventArgs e)
        {
            PageContainer.SelectedItem = PageRegister;
        }

        private void ShowLoginPage(object sender, RoutedEventArgs e)
        {
            PageContainer.SelectedItem = PageLogin;
        }

        private async void Login(object sender, RoutedEventArgs e)
        {
            var username = LoginUsername.Text;
            var password = LoginPassword.Password;

            PageContainer.SelectedItem = PageLoading;
            try
            {
                var result = await _api.Login(new Shared.SessionPostRequest(username, password, false));
                if (result.Code == RequestResult.Success)
                {
                    ClearInputs();
                    LoginError.Visibility = Visibility.Hidden;
                    _auth.AuthToken = result.AuthToken;
                    await GetUserInfo();
                }
                else
                {
                    LoginError.Text = result.Message;
                    LoginError.Visibility = Visibility.Visible;
                    PageContainer.SelectedItem = PageLogin;
                }
            }
            catch (Exception ex)
            {
                LoginError.Text = ex.Message;
                LoginError.Visibility = Visibility.Visible;
                PageContainer.SelectedItem = PageLogin;
            }
        }

        private async void Register(object sender, RoutedEventArgs e)
        {
            if (RegisterPassword.Password != RegisterPasswordConfirm.Password)
            {
                RegisterError.Text = "the given passwords don't match";
                return;
            }

            var username = RegisterUsername.Text;
            var password = RegisterPassword.Password;
            var email = RegisterEmail.Text;

            PageContainer.SelectedItem = PageLoading;
            try
            {
                var result = await _api.Register(new UserPostRequest(email, username, password));
                if (result.Code == RequestResult.Success)
                {
                    ClearInputs();
                    RegisterError.Visibility = Visibility.Hidden;
                    _auth.AuthToken = result.AuthToken;

                    await GetUserInfo();
                }
                else
                {
                    RegisterError.Text = string.Join("\n", result.Errors?.Select(err => err.Message) ?? new List<string> { result.Message });
                    RegisterError.Visibility = Visibility.Visible;
                    PageContainer.SelectedItem = PageRegister;
                }
            }
            catch (Exception ex)
            {
                RegisterError.Text = ex.Message;
                RegisterError.Visibility = Visibility.Visible;
                PageContainer.SelectedItem = PageRegister;
            }
        }

        private async void Logout(object sender, RoutedEventArgs e)
        {
            PageContainer.SelectedItem = PageLoading;
            try
            {
                await _api.Logout(new SessionDeleteRequest());
            }
            catch
            {

            }

            PageContainer.SelectedItem = PageLogin;
        }

        private async Task GetUserInfo()
        {
            PageContainer.SelectedItem = PageLoading;
            try
            {
                var result = await _api.GetSession();
                if (result.Code == RequestResult.Success)
                {
                    UserUsername.Text = result.Username;
                    PageContainer.SelectedItem = PageUser;
                }
                else
                {
                    PageContainer.SelectedItem = PageLogin;
                }
            }
            catch (Exception ex)
            {
                LoginError.Text = ex.Message;
                LoginError.Visibility = Visibility.Visible;
                PageContainer.SelectedItem = PageLogin;
            }
        }

        private async void StartGame(object sender, RoutedEventArgs e)
        {
            if(!File.Exists("Binaries/Win64/RxGame-Win64-Test.exe"))
            {
                MessageBox.Show("Binaries/Win64/RxGame-Win64-Test.exe not found!", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userinfo = JsonSerializer.Serialize(new
            {
                nickname = UserUsername.Text,
                username = UserUsername.Text,
                auth_token = _auth.AuthToken
            });

            await File.WriteAllTextAsync("Binaries/Win64/userinfo", userinfo);
            Process.Start("Binaries/Win64/RxGame-Win64-Test.exe");

            Application.Current.Shutdown();
        }

        private void ClearInputs()
        {
            RegisterUsername.Clear();
            RegisterPassword.Clear();
            RegisterPasswordConfirm.Clear();
            RegisterEmail.Clear(); 
            LoginUsername.Clear();
            LoginPassword.Clear();
        }
    }
}
