using System.Linq;
using System.Text;
using System.Windows;
using href.Utils;

namespace Mp3TagsEncodingFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ScanFolder(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ApplySelectedFixes(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }


        private string CorrectStringEncoding(string baseString)
        {
            Encoding[] outgoingEncodings = EncodingTools.DetectOutgoingEncodings(baseString);

            if (!outgoingEncodings.Any(encoding => string.Equals(encoding.EncodingName, Encoding.Default.EncodingName)))
            {
                byte[] bytes = Encoding.Unicode.GetBytes(baseString);

                byte[] convetedBytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, bytes.Where(b => b != 0).ToArray());

                return Encoding.UTF8.GetString(convetedBytes);
            }

            return null;
        }
    }
}
