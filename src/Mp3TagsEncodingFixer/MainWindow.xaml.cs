using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using href.Utils;
using File = TagLib.File;

namespace Mp3TagsEncodingFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string SelectedFolder { get; set; }

        public IEnumerable<Mp3> Mp3s { get; private set; }


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

        // ReSharper disable once InconsistentNaming
        private async void ScanFolderForMp3sWithCorruptedTags(object sender, RoutedEventArgs e)
        {
            string folder = SelectedFolder;

            ScanFolderButton.IsEnabled = false;
            try
            {
                if (Directory.Exists(folder))
                {
                    using (StreamWriter stringWriter = new StreamWriter("Scan.log", false, Encoding.UTF8))
                    {
                        Mp3s = await Task<List<Mp3>>.Factory.StartNew(() => GetMp3sWithCorruptedTags(stringWriter, folder));
                        RaisePropertyChanged(nameof(Mp3s));
                    }
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(this, exception.ToString());
            }
            finally
            {
                ScanFolderButton.IsEnabled = true;
            }
        }

        // ReSharper disable once InconsistentNaming
        private List<Mp3> GetMp3sWithCorruptedTags(TextWriter errors, string folder)
        {
            IEnumerable<string> fileNames = Directory.EnumerateFiles(folder, "*.mp3", SearchOption.AllDirectories);

            List<Mp3> result = new List<Mp3>();

            foreach (string fileName in fileNames)
            {
                try
                {
                    using (File file = File.Create(fileName))
                    {
                        file.Mode = File.AccessMode.Read;

                        Mp3 mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Album), file.Tag.Album);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        //bool result = FillPerformers(file.Tag.Performers, track);
                        //result = FillAlbumName(file.Tag.Album, track) || result;
                        //result = FillAlbumPerformers(file.Tag.AlbumArtists, track) || result;
                        //result = FillTitle(file.Tag.Title, track) || result;
                        //result = FillGenres(file.Tag.Genres, track) || result;
                        //result = FillTrackNumber(file.Tag.Track, track) || result;
                        //result = FillTrackCount(file.Tag.TrackCount, track) || result;
                        //result = FillDiskNumber(file.Tag.Disc, track) || result;
                        //result = FillDiskCount(file.Tag.DiscCount, track) || result;
                        //result = FillComposers(file.Tag.Composers, track) || result;
                        //result = FillYear(file.Tag.Year, track) || result;
                        //result = FillSize(track) || result;
                    }
                }
                catch (Exception e)
                {
                    errors.Write(fileName);
                    errors.WriteLine(":");
                    errors.WriteLine(e.ToString());
                }
            }

            return result;
        }

        private static Mp3 GetMp3IfTagIsCorrupted(string fileName, string tagName, params string[] tagValue)
        {
            (bool isCorrupted, string[] fixedValue) = IsCorrupted(tagValue);
            if (isCorrupted)
            {
                return new Mp3
                    {
                        FileName = fileName,
                        TagName = tagName,
                        TagValue = string.Join(", ", tagValue),
                        TagValueFix = string.Join(", ", fixedValue)
                    };
            }

            return null;
        }

        private static (bool isCorrupted, string[] fixedValue) IsCorrupted(params string[] strings)
        {
            string[] fixedValue = strings
                .Where(item => item != null)
                .Select(CorrectStringEncoding)
                .ToArray();

            bool isCorrupted = false;

            if (fixedValue.Any())
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(strings[i])
                        && !string.IsNullOrWhiteSpace(fixedValue[i])
                        && !string.Equals(strings[i], fixedValue[i]))
                    {
                        isCorrupted = true;
                        break;
                    }
                }
            }

            return (isCorrupted, fixedValue);
        }

        private void ApplySelectedFixes(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }


        private static string CorrectStringEncoding(string baseString)
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
