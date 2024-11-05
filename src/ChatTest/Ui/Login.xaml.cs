using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

namespace ChatTest.Ui
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            LoadWindow();
            HostStart();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            GC.Collect();
        }

        protected void LoadWindow()
        {
            LoginWindowSetting.Default.Save();
            {
                try
                {
                    Resources["ActionColor"] = new SolidColorBrush(Program.Transformat.GetColor(LoginWindowSetting.Default.ActionColor));
                }
                catch
                {
                    Resources["ActionColor"] = new SolidColorBrush(Program.Transformat.GetColor("#FFF34B4B"));
                }
                try
                {
                    if (!File.Exists(".\\Resources\\HarmonyOS_Sans_Medium.ttf"))
                    {
                        if (!Directory.Exists(".\\Resources"))
                        {
                            Directory.CreateDirectory(".\\Resources");
                        }
                        using (FileStream FS = new(".\\Resources\\HarmonyOS_Sans_Medium.ttf", FileMode.Create, FileAccess.Write))
                        {
                            using (MemoryStream MS = (MemoryStream)Program.AssemblyResources.GetExecutingAssemblyResources("HarmonyOS_Sans_Medium.ttf"))
                            {
                                FS.Write(MS.ToArray());
                                FS.Flush();
                            }
                        }
                    }
                    Resources["HarmonyOS_Sans_Medium"] = new FontFamily(new Uri(".\\Resources\\HarmonyOS_Sans_Medium.ttf", UriKind.Relative), "HarmonyOS Sans Medium");
                }
                catch { }
            }
            {
                Brush BGbrush;
                string BGinfo = LoginWindowSetting.Default.Background;
                if (BGinfo.StartsWith('#'))
                {
                    BGbrush = new SolidColorBrush(Program.Transformat.GetColor(BGinfo));
                }
                else if (BGinfo == "Default")
                {
                    BGbrush = new SolidColorBrush(Colors.LightGray);
                }
                else if (BGinfo == "Light")
                {
                    BGbrush = new SolidColorBrush(Colors.LightGray);
                }
                else if (BGinfo == "Dark")
                {
                    BGbrush = new SolidColorBrush(Colors.DarkGray);
                }
                else
                {
                    try
                    {
                        Stream stream = Program.AssemblyResources.GetExecutingAssemblyResources(BGinfo);
                        BGbrush = new ImageBrush(Program.Transformat.BitmapImageFromStream(stream)) { Stretch = Stretch.UniformToFill };
                    }
                    catch
                    {
                        BGbrush = new SolidColorBrush(Colors.LightGray);
                    }
                }
                LoginWindow.Background = BGbrush;
            }
            {
                string theme = LoginWindowSetting.Default.Theme;
                switch (theme)
                {
                    case "Light":
                        LoginWindow.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#eee1ca"));
                        WindowHead.Background = new SolidColorBrush(Program.Transformat.GetColor("#eadcd6"));
                        BtnClose.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#edebe0"));
                        BtnMinest.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#edebe0"));
                        BtnClose.OpacityMask = new SolidColorBrush(Colors.Red);
                        BtnMinest.OpacityMask = new SolidColorBrush(Program.Transformat.GetColor("#edeadc"));
                        break;
                    case "Dark":
                        LoginWindow.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#43454a"));
                        WindowHead.Background = new SolidColorBrush(Program.Transformat.GetColor("#363532"));
                        BtnClose.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#a8a6b9"));
                        BtnMinest.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#a8a6b9"));
                        BtnClose.OpacityMask = new SolidColorBrush(Colors.Red);
                        BtnMinest.OpacityMask = new SolidColorBrush(Program.Transformat.GetColor("#cad0d3"));
                        break;
                    case "Other":
                        try
                        {
                            LoginWindow.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor(LoginWindowSetting.Default.Theme));
                            WindowHead.Background = new SolidColorBrush(Program.Transformat.GetColor(LoginWindowSetting.Default.HeadBackGroundColor));
                            BtnClose.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor(LoginWindowSetting.Default.HeadBtnOverColor));
                            BtnMinest.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor(LoginWindowSetting.Default.HeadBtnOverColor));
                            BtnClose.OpacityMask = new SolidColorBrush(Program.Transformat.GetColor(LoginWindowSetting.Default.HeadBtnPressCC));
                            BtnMinest.OpacityMask = new SolidColorBrush(Program.Transformat.GetColor(LoginWindowSetting.Default.HeadBtnPressMC));
                        }
                        catch
                        {
                            LoginWindow.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#eee1ca"));
                            WindowHead.Background = new SolidColorBrush(Program.Transformat.GetColor("#eadcd6"));
                            BtnClose.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#edebe0"));
                            BtnMinest.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#edebe0"));
                            BtnClose.OpacityMask = new SolidColorBrush(Colors.Red);
                            BtnMinest.OpacityMask = new SolidColorBrush(Program.Transformat.GetColor("#edeadc"));
                        }
                        break;
                    default:
                        LoginWindow.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#eee1ca"));
                        WindowHead.Background = new SolidColorBrush(Program.Transformat.GetColor("#eadcd6"));
                        BtnClose.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#edebe0"));
                        BtnMinest.BorderBrush = new SolidColorBrush(Program.Transformat.GetColor("#edebe0"));
                        BtnClose.OpacityMask = new SolidColorBrush(Colors.Red);
                        BtnMinest.OpacityMask = new SolidColorBrush(Program.Transformat.GetColor("#edeadc"));
                        break;
                }
            }
            {
                LabHeadTitle.Content = "登录";
                if (LoginWindowSetting.Default.WindowIcon != "None")
                {
                    try
                    {
                        ImgHeadIcon.Source = Program.Transformat.BitmapImageFromStream(Program.AssemblyResources.GetExecutingAssemblyResources(LoginWindowSetting.Default.WindowIcon));
                        ImgHeadDock.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        ImgHeadDock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            {
                ImgUserIcon.Source = Program.Transformat.BitmapImageFromPath("D:\\Dev\\OrleansTest\\ChatTest\\Resources\\QianXI.png");
            }
        }

        protected async void HostStart()
        {
            Program.HostManager.StartAsync().Wait();
            var _cookie = UserDataSetting.Default.UserData;
            if (JsonConvert.DeserializeObject<Cookie>(_cookie) == null)
                return;
            var ret = await Program.HostManager.Login(_cookie);
            switch (ret)
            {
                case 1:
                    this.Dispatcher.Invoke(() =>
                    {
                        Program.RunChatWindow();
                        this.Close();
                    });
                    break;
                case 2:
                    MessageBox.Show("登录过期，请重新登录");
                    break;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void BtnMinest_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void LoginWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (TbxUserName.Text.Length <= 0)
            {
                MessageBox.Show("未输入用户名.");
                return;
            }
            if (TbxPasswd.Password.Length <= 0)
            {
                MessageBox.Show("未输入密码.");
                return;
            }
            if (!Program.HostManager.IsConnected)
            {
                MessageBox.Show("无法连接服务器.");
                return;
            }
            await Program.HostManager.Register(TbxUserName.Text, TbxPasswd.Password);
            TbxPasswd.Password = string.Empty;
        }

        private async void BtnLogIn_Click(object sender, RoutedEventArgs e)
        {
            if (TbxUserName.Text.Length <= 0)
            {
                MessageBox.Show("未输入用户名.");
                return;
            }
            if (TbxPasswd.Password.Length <= 0)
            {
                MessageBox.Show("未输入密码.");
                return;
            }
            if (!Program.HostManager.IsConnected)
            {
                MessageBox.Show("无法连接服务器.");
                return;
            }
            var ret = await Program.HostManager.Login(TbxUserName.Text, TbxPasswd.Password);
            switch (ret)
            {
                case 1:
                    this.Dispatcher.Invoke(() =>
                    {
                        Program.RunChatWindow();
                        this.Close();
                    });
                    break;
                case 0:
                    MessageBox.Show($"登录失败.");
                    break;
                default:
                    MessageBox.Show($"其他异常.");
                    break;
            }
        }
    }
}