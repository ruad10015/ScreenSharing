using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenSharing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StartListening();
        }

        private void StartListening()
        {
            var ipAddress = IPAddress.Parse("192.168.1.69");
            var port = 27001;

            Task.Run(() =>
            {
                try
                {
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        var ep = new IPEndPoint(ipAddress, port);
                        socket.Bind(ep);
                        socket.Listen(10);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Listening on {socket.LocalEndPoint}", "Listen", MessageBoxButton.OK);
                        });

                        while (true)
                        {
                            var client = socket.Accept();
                            Task.Run(() =>
                            {
                                var clientEndPoint = (IPEndPoint)client.RemoteEndPoint;

                                try
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        int bytesRead;
                                        byte[] buffer = new byte[1024];

                                        while ((bytesRead = client.Receive(buffer)) > 0)
                                        {
                                            ms.Write(buffer, 0, bytesRead);
                                        }

                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            AddImageToControl(ms.ToArray());
                                            
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });
        }

        private void AddImageToControl(byte[] imageData)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.None;
                bitmapImage.StreamSource = new MemoryStream(imageData);
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                userScreen.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}