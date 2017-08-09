using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using href.Utils;

namespace Mp3TagsEncodingFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string SelectedFolder { get; private set; }


        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }


        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedFolder = dialog.SelectedPath;
                    RaisePropertyChanged(nameof(SelectedFolder));
                }
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
