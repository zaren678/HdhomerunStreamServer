using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HdhrStreamServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Delegates for thread safe ui updating
        delegate void SetTextBlockDelegate(TextBlock textBlock, String s);

        StreamServerController streamController;

        public MainWindow()
        {
            InitializeComponent();

            string theName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            string theVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            this.Title = theName + " " + theVersion;

            try
            {
                vlcPathTextBox.Text = FindViewer();
                Properties.Settings.Default.vlcPath = vlcPathTextBox.Text;                
            }
            catch (ViewerNotFoundException)
            {
            }
            catch (BadVlcVersionException exception)
            {
                MessageBoxResult error = System.Windows.MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK);
            }

            controlPortTextBox.Text = Properties.Settings.Default.controlPort.ToString();
            vlcPathTextBox.Text = Properties.Settings.Default.vlcPath;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            int controlPort;
            try
            {
                 controlPort = Int32.Parse(controlPortTextBox.Text);

                 if (controlPort < 4096 || controlPort > 65535)
                 {
                     System.Windows.MessageBox.Show("Control Port must an integer between 4096 and 65535", "Error", MessageBoxButton.OK);
                     return;
                 }
            }
            catch (ArgumentNullException)
            {
                System.Windows.MessageBox.Show("Control Port must be filled out", "Error", MessageBoxButton.OK);
                return;
            }
            catch (FormatException)
            {
                System.Windows.MessageBox.Show("Control Port must an integer between 4096 and 65535", "Error", MessageBoxButton.OK);
                return;
            }
            catch (OverflowException)
            {
                System.Windows.MessageBox.Show("Control Port must an integer between 4096 and 65535", "Error", MessageBoxButton.OK);
                return;
            }

            if (String.IsNullOrEmpty(vlcPathTextBox.Text))
            {
                System.Windows.MessageBox.Show("VLC path must be set", "Error", MessageBoxButton.OK);
                return;
            }

            RandomPortGenerator.Instance.addControlPort(controlPort);

            streamController = new StreamServerController(controlPort, RandomPortGenerator.Instance.getNextPort(), RandomPortGenerator.Instance.getNextPort());

            streamController.ServerStarted += new StreamServerController.ServerStartedHandler(HandleServerStarted);
            streamController.ServerStopped += new StreamServerController.ServerStoppedHandler(HandleServerStopped);
            streamController.ServerStreaming += new StreamServerController.ServerStreamingHandler(HandleServerStreaming);
            streamController.ServerStreamingStopped += new StreamServerController.ServerStreamingStoppedHandler(HandleServerStreamingStopped);
            
            streamController.setViewer( vlcPathTextBox.Text );
            streamController.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (streamController != null)
            {
                streamController.Stop();
                RandomPortGenerator.Instance.deletePort(streamController.VideoInputPort);
                RandomPortGenerator.Instance.deletePort(streamController.VideoOutputPort);
                RandomPortGenerator.Instance.deletePort(streamController.ControlPort);
            }
        }

        private void HandleServerStarted()
        {
            UpdateTextBlock(StatusText, "Listening");
        }

        private void HandleServerStopped()
        {
            UpdateTextBlock(StatusText, "Not Running");
        }

        private void HandleServerStreaming()
        {
            UpdateTextBlock(StatusText, "Streaming");
        }

        private void HandleServerStreamingStopped()
        {
            UpdateTextBlock(StatusText, "Listening");
        }

        private void UpdateTextBlock(TextBlock textBlock, String stringVal)
        {
            if (textBlock != null)
            {
                // Checking if this thread has access to the object.
                if (textBlock.Dispatcher.CheckAccess())
                {
                    // This thread has access so it can update the UI thread.
                    UpdateTextBlockUI(textBlock, stringVal);
                }
                else
                {
                    // This thread does not have access to the UI thread.
                    // Place the update method on the Dispatcher of the UI thread.
                    textBlock.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new SetTextBlockDelegate(UpdateTextBlockUI), textBlock, stringVal);
                }
            }
        }

        private void UpdateTextBlockUI(TextBlock textBlock, String stringVal)
        {
            if (textBlock.Text != stringVal)
            {
                textBlock.Text = stringVal;
            }
        }

        private void controlPortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int controlPort;
            try
            {
                controlPort = Int32.Parse(controlPortTextBox.Text);
                Properties.Settings.Default.controlPort = controlPort;
            }
            catch (ArgumentNullException)
            {
                
            }
            catch (FormatException)
            {
                
            }
            catch (OverflowException)
            {
                
            }
        }

        public string FindViewer()
        {
            try
            {
                RegistryKey vlcKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\VideoLAN\\VLC");

                string vlcExe = getRegistryStringKey("SOFTWARE\\VideoLAN\\VLC", "");

                if (!System.IO.File.Exists(vlcExe))
                {
                    throw new ViewerNotFoundException("Viewer not found");
                }

                System.Console.WriteLine("VLC: " + vlcExe);

                String vlcVersion = getRegistryStringKey("SOFTWARE\\VideoLAN\\VLC", "Version");

                System.Console.WriteLine("VLC version: " + vlcVersion);

                if (vlcVersion.Equals("2.0.1"))
                {
                    throw new BadVlcVersionException("VLC version 2.0.1 had an issue with H.264 encoding.\nPlease install a different version of VLC");
                }

                return vlcExe;
            }
            catch (RegistryKeyException e)
            {
                throw new ViewerNotFoundException(e.Message);
            }
        }

        private String getRegistryStringKey(String regKey, String key)
        {
            RegistryKey openedkey = Registry.LocalMachine.OpenSubKey(regKey);

            if (openedkey == null)
            {
                throw new RegistryKeyException("VLC Not found error: Registry Key Not found");
            }

            RegistryValueKind versionType = openedkey.GetValueKind("Version");

            if (versionType != RegistryValueKind.String)
            {
                throw new RegistryKeyException("VLC Not found error: Registry key type not a string");
            }

            Object value = openedkey.GetValue(key);

            if (value == null)
            {
                throw new RegistryKeyException("VLC Not found error: Registry key value was empty");
            }

            return (String)value;
        }

        private void browseVlcButton_Click(object sender, RoutedEventArgs e)
        {            
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
            if( file.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                vlcPathTextBox.Text = file.FileName;
                Properties.Settings.Default.vlcPath = file.FileName;                
            }
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }
  
    }
}
