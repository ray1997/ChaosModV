using CommunityToolkit.Mvvm.ComponentModel;
using ModernWpf.Controls;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;

namespace ConfigApp
{
    public partial class MainWindowVM : ObservableObject
    {
        public MainWindowVM()
        {
            CheckForUpdates();
        }

        #region Update check
        private bool? isModUpToDate;
        public bool? IsModUpToDate
        {
            get => isModUpToDate;
            set
            {
                if (SetProperty(ref isModUpToDate, value))
                {
                    OnPropertyChanged(nameof(ModStatus));
                    OnPropertyChanged(nameof(ModStatusIcon));
                    OnPropertyChanged(nameof(AllowToOpenModPage));
                }
            }
        }

        public string ModStatus
        {
            get
            {
                if (IsModUpToDate is null)
                    return "No connection!";
                return IsModUpToDate.Value ?
                    "Mod updated!" :
                    "Mod outdated!";
            }
        }

        public bool AllowToOpenModPage
        {
            get
            {
                if (IsModUpToDate is null)
                    return false;
                return !IsModUpToDate.Value;
            }
        }

        public FontIcon ModStatusIcon
        {
            get
            {
                if (IsModUpToDate == null)
                {
                    return new FontIcon 
                    {
                        Glyph = "\uF384", Foreground = new SolidColorBrush(Colors.OrangeRed)
                    };
                }
                return IsModUpToDate.Value ?
                    new FontIcon
                    {
                        Glyph = "\uE930",
                        Foreground = new SolidColorBrush(Colors.Green)
                    } :
                    new FontIcon
                    {
                        Glyph = "\uE946",
                        Foreground = new SolidColorBrush(Colors.Yellow)
                    };

            }
        }

        private async void CheckForUpdates()
        {
            HttpClient httpClient = new HttpClient();

            try
            {
                string newVersion = await httpClient.GetStringAsync("https://gopong.dev/chaos/version.txt");
                IsModUpToDate = Equals(Info.VERSION, newVersion);
            }
            catch (HttpRequestException)
            {
                IsModUpToDate = null;
            }
        }
        #endregion

        #region Misc page sizing
        private bool listingTwoRow = false;
        public bool ListingTwoRow
        {
            get => listingTwoRow;
            set
            {
                if (SetProperty(ref listingTwoRow, value))
                {
                    OnPropertyChanged(nameof(SecondColumnSize));
                }
            }
        }

        public GridLength SecondColumnSize
        {
            get
            {
                if (ListingTwoRow)
                {
                    return new GridLength(1, GridUnitType.Star);
                }
                return new GridLength(0, GridUnitType.Pixel);
            }
        }
        #endregion
    }
}
