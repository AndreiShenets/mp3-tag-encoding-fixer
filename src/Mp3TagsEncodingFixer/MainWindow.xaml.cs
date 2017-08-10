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
using System.Windows.Input;
using href.Utils;
using File = TagLib.File;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;

namespace Mp3TagsEncodingFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public string SelectedFolder { get; set; }

        public List<Mp3> Mp3s { get; private set; }


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
                        // ReSharper disable once AccessToDisposedClosure
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
        private List<Mp3> GetMp3sWithCorruptedTags(TextWriter log, string folder)
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

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.AlbumArtists), file.Tag.AlbumArtists);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.AlbumArtistsSort), file.Tag.AlbumArtistsSort);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.AlbumSort), file.Tag.AlbumSort);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Comment), file.Tag.Comment);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Composers), file.Tag.Composers);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.ComposersSort), file.Tag.ComposersSort);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Conductor), file.Tag.Conductor);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Genres), file.Tag.Genres);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Lyrics), file.Tag.Lyrics);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Performers), file.Tag.Performers);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.PerformersSort), file.Tag.PerformersSort);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.Title), file.Tag.Title);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }

                        mp3 = GetMp3IfTagIsCorrupted(fileName, nameof(file.Tag.TitleSort), file.Tag.TitleSort);
                        if (mp3 != null)
                        {
                            result.Add(mp3);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(fileName);
                    log.WriteLine(":");
                    log.WriteLine(e.ToString());
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
                        IsChecked = true,
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

        private async void ApplySelectedFixes(object sender, RoutedEventArgs e)
        {
            List<Mp3> mp3s = Mp3s;

            ApplySelectedFixesButton.IsEnabled = false;
            try
            {
                if (mp3s != null && mp3s.Any(item => item.IsChecked))
                {
                    using (StreamWriter stringWriter = new StreamWriter("ApplyFixes.log", false, Encoding.UTF8))
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        Mp3s = await Task<List<Mp3>>.Factory.StartNew(() => ApplySelectedFixes(stringWriter, mp3s));
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
                ApplySelectedFixesButton.IsEnabled = true;
            }
        }

        private static List<Mp3> ApplySelectedFixes(TextWriter log, List<Mp3> mp3s)
        {
            List<Mp3> appliedFixes = new List<Mp3>();

            IEnumerable<IGrouping<string, Mp3>> mp3sToProcess = mp3s.Where(item => item.IsChecked)
                .GroupBy(item => item.FileName);

            foreach (IGrouping<string, Mp3> mp3Group in mp3sToProcess)
            {
                string fileName = mp3Group.Key;

                try
                {
                    using (File file = File.Create(fileName))
                    {
                        file.Mode = File.AccessMode.Write;

                        foreach (Mp3 mp3 in mp3Group)
                        {
                            switch (mp3.TagName)
                            {
                                case nameof(file.Tag.Album):
                                    file.Tag.Album = CorrectStringEncoding(file.Tag.Album);

                                    break;

                                case nameof(file.Tag.AlbumSort):
                                    file.Tag.AlbumSort = CorrectStringEncoding(file.Tag.AlbumSort);

                                    break;

                                case nameof(file.Tag.AlbumArtists):
                                    file.Tag.AlbumArtists = file.Tag.AlbumArtists
                                        .Select(CorrectStringEncoding)
                                        .ToArray();

                                    break;

                                case nameof(file.Tag.AlbumArtistsSort):
                                    file.Tag.AlbumArtistsSort = file.Tag.AlbumArtistsSort
                                        .Select(CorrectStringEncoding)
                                        .ToArray();

                                    break;

                                case nameof(file.Tag.Comment):
                                    file.Tag.Comment = CorrectStringEncoding(file.Tag.Comment);

                                    break;

                                case nameof(file.Tag.Composers):
                                    file.Tag.Composers = file.Tag.Composers
                                        .Select(CorrectStringEncoding)
                                        .ToArray();

                                    break;

                                case nameof(file.Tag.ComposersSort):
                                    file.Tag.ComposersSort = file.Tag.ComposersSort
                                        .Select(CorrectStringEncoding)
                                        .ToArray();

                                    break;

                                case nameof(file.Tag.Conductor):
                                    file.Tag.Conductor = CorrectStringEncoding(file.Tag.Conductor);

                                    break;

                                case nameof(file.Tag.Genres):
                                    file.Tag.Genres = file.Tag.Genres
                                        .Select(CorrectStringEncoding)
                                        .ToArray();

                                    break;

                                case nameof(file.Tag.Lyrics):
                                    file.Tag.Lyrics = CorrectStringEncoding(file.Tag.Lyrics);

                                    break;

                                case nameof(file.Tag.Performers):
                                    file.Tag.Performers = file.Tag.Performers
                                        .Select(CorrectStringEncoding)
                                        .ToArray();

                                    break;

                                case nameof(file.Tag.PerformersSort):
                                    file.Tag.PerformersSort = file.Tag.PerformersSort
                                        .Select(CorrectStringEncoding)
                                        .ToArray();

                                    break;

                                case nameof(file.Tag.Title):
                                    file.Tag.Title = CorrectStringEncoding(file.Tag.Title);

                                    break;

                                case nameof(file.Tag.TitleSort):
                                    file.Tag.TitleSort = CorrectStringEncoding(file.Tag.TitleSort);

                                    break;
                            }

                            appliedFixes.Add(mp3);
                        }

                        file.Save();
                    }
                }
                catch (Exception e)
                {
                    log.Write(fileName);
                    log.WriteLine(":");
                    log.WriteLine(e.ToString());
                }
            }

            return mp3s.Where(item => !appliedFixes.Contains(item)).ToList();
        }

        private static string CorrectStringEncoding(string baseString)
        {
            if (!string.IsNullOrEmpty(baseString))
            {
                Encoding[] outgoingEncodings = EncodingTools.DetectOutgoingEncodings(baseString);

                if (!outgoingEncodings.Any(encoding => string.Equals(encoding.EncodingName, Encoding.Default.EncodingName)))
                {
                    byte[] bytes = Encoding.Unicode.GetBytes(baseString);

                    byte[] convertedBytes = Encoding.Convert(Encoding.Default, Encoding.UTF8,
                        bytes.Where(b => b != 0).ToArray());

                    return Encoding.UTF8.GetString(convertedBytes);
                }
            }

            return baseString;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space
                && sender is ListBox listBox
                && listBox.SelectedItem is Mp3 mp3)
            {
                mp3.IsChecked = !mp3.IsChecked;
                e.Handled = true;
            }
        }
    }
}
