using ChatTest.Ui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.Drawing;
using OrleansTest.IGrains;
using System.Net;
using Newtonsoft.Json;

namespace ChatTest
{
    public class Program
    {
        [STAThread]
        static int Main()
        {
            Application app = new Application();
            Window win = new Login();
            win.Show();
            app.Run();
            HostManager.StopAsync().Wait();
            return 0;
        }

        public static void RunChatWindow()
        {
            ChatWindow CW = new ChatWindow();
            CW.Show();
        }

        public static class HostManager
        {
            static HostManager()
            {
                IsConnected = false;
            }

            private static IClusterClient _client;
            private static IHost _host;

            public static bool IsConnected { get; private set; }

            public static async Task StartAsync()
            {
                IHostBuilder builder = Host.CreateDefaultBuilder()
                                       .UseOrleansClient(client =>
                                       {
                                           client.UseLocalhostClustering();
                                       })
                                       .ConfigureLogging(logging => logging.AddConsole())
                                       .UseConsoleLifetime();

                _host = builder.Build();
                while (true)
                {
                    try
                    {
                        await _host.StartAsync();
                        break;
                    }
                    catch { }
                }
                IsConnected = true;

                _client = _host.Services.GetRequiredService<IClusterClient>();
            }

            public static async Task StopAsync()
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    await _host.StopAsync();
                }
            }

            public static async ValueTask<int> Login(string username, string password)
            {
                ISessionGrain session = _client.GetGrain<ISessionGrain>(0);
                string strco = await session.Login(username, password);
                if (strco == string.Empty)
                {
                    return 0;
                }
                else
                {
                    UserDataSetting.Default.UserData = strco;
                    UserDataSetting.Default.Save();
                    return 1;
                }
            }

            public static async ValueTask<int> Login(string cookie)
            {
                ISessionGrain session = _client.GetGrain<ISessionGrain>(0);
                string strco = await session.Login(cookie);
                if (strco == string.Empty)
                {
                    return 0;
                }
                else if (strco == cookie)
                {
                    return 2;
                }
                else
                {
                    UserDataSetting.Default.UserData = strco;
                    UserDataSetting.Default.Save();
                    return 1;
                }
            }

            public static async Task Register(string username, string password)
            {
                ISessionGrain session = _client.GetGrain<ISessionGrain>(0);
                await session.Register(username, password);
            }
        }
        public static class AssemblyResources
        {
            public static string _namespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            public static Assembly assembly = Assembly.GetExecutingAssembly();
            public static Stream GetExecutingAssemblyResources(string assemblyPath)
            {
                Stream stream = assembly.GetManifestResourceStream(_namespace + ".Resources." + assemblyPath);
                if (stream != null)
                {
                    return stream;
                }
                else
                {
                    throw new FileLoadException("加载嵌入资源失败");
                }
            }
        }
        public static class Transformat
        {
            public static string DoubleString(string str, int count)
            {
                string ret = string.Empty;
                for (int i = 0; i < count; i++)
                {
                    ret += str;
                }
                return ret;
            }

            public static BitmapImage BitmapImageFromStream(Stream stream)
            {
                BitmapImage bitmap = new BitmapImage() { CacheOption = BitmapCacheOption.OnLoad };
                if (stream != null)
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                return bitmap;
            }
            public static BitmapImage BitmapImageFromPath(string imagePath)
            {
                BitmapImage bitmap = new BitmapImage();

                if (File.Exists(imagePath))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                    {
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }
                return bitmap;
            }

            /// <summary>
            /// 将Bitmap对象转换成bitmapImage对象
            /// </summary>
            /// <param name="bitmap"></param>
            /// <returns></returns>
            public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                return image;
            }

            /// <summary>
            /// 将bitmapImage对象转换成Bitmap对象
            /// </summary>
            /// <param name="bitmapImage"></param>
            /// <returns></returns>
            public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                    enc.Save(outStream);
                    Bitmap bitmap = new Bitmap(outStream);
                    return bitmap;
                }
            }

            public static System.Windows.Media.Color GetColor(string color)
            {
                if (!color.StartsWith('#'))
                {
                    throw new ArgumentException("输入了一个错误的颜色.");
                }
                color = color.ToUpper();
                switch (color.Length)
                {
                    case 4:
                        return System.Windows.Media.Color.FromRgb(
                            byte.Parse(Program.Transformat.DoubleString(color.Substring(1, 1), 2),System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(Program.Transformat.DoubleString(color.Substring(2, 1), 2),System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(Program.Transformat.DoubleString(color.Substring(3, 1), 2), System.Globalization.NumberStyles.HexNumber));
                    case 5:
                        return System.Windows.Media.Color.FromArgb(
                            byte.Parse(Program.Transformat.DoubleString(color.Substring(1, 1), 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(Program.Transformat.DoubleString(color.Substring(2, 1), 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(Program.Transformat.DoubleString(color.Substring(3, 1), 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(Program.Transformat.DoubleString(color.Substring(4, 1), 2), System.Globalization.NumberStyles.HexNumber));
                    case 7:
                        return System.Windows.Media.Color.FromRgb(
                            byte.Parse(color.Substring(1, 2),System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));
                    case 9:
                        return System.Windows.Media.Color.FromArgb(
                            byte.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(color.Substring(7, 2), System.Globalization.NumberStyles.HexNumber));
                    default:
                        throw new ArgumentException("输入了一个错误的颜色.");

                }
            }
        }
    }
}