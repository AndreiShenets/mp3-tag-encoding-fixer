using System.ComponentModel;

namespace Mp3TagsEncodingFixer
{
    public class Mp3 : INotifyPropertyChanged
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        public string FileName { get; set; }
        public string TagName { get; set; }
        public string TagValue { get; set; }
        public string TagValueFix { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}