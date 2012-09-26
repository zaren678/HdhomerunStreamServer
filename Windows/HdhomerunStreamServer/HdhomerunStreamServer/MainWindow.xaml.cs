using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfApplication1
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
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            int controlPort;
            try
            {
                controlPort = Int32.Parse(controlPortTextBox.Text);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Control Port must be filled out", "Error", MessageBoxButton.OK);
                return;
            }
            catch (FormatException)
            {
                MessageBox.Show("Control Port must an integer between 4096 and 65535", "Error", MessageBoxButton.OK);
                return;
            }
            catch (OverflowException)
            {
                MessageBox.Show("Control Port must an integer between 4096 and 65535", "Error", MessageBoxButton.OK);
                return;
            }

            RandomPortGenerator.Instance.addControlPort(controlPort);

            streamController = new StreamServerController(controlPort, RandomPortGenerator.Instance.getNextPort(), RandomPortGenerator.Instance.getNextPort());

            streamController.ServerStarted += new StreamServerController.ServerStartedHandler(HandleServerStarted);
            streamController.ServerStopped += new StreamServerController.ServerStoppedHandler(HandleServerStopped);
            streamController.ServerStreaming += new StreamServerController.ServerStreamingHandler(HandleServerStreaming);
            streamController.ServerStreamingStopped += new StreamServerController.ServerStreamingStoppedHandler(HandleServerStreamingStopped);

            try
            {
                streamController.FindViewer();
                streamController.Start();
            }
            catch (ViewerNotFoundException exception)
            {
                MessageBoxResult error = MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK);
            }
            catch (BadVlcVersionException exception)
            {
                MessageBoxResult error = MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK);
            }
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

        }
  
    }
}
