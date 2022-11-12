using CommunityToolkit.Mvvm.ComponentModel;
using ModernWpf.Controls;
using Shared;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ConfigApp
{
    public partial class MainWindowVM : ObservableObject
    {
        public MainWindowVM()
        {
            CheckForUpdates();

            m_twitchFile.ReadFile();
            this.PropertyChanged += SettingsMonitor;
        }

        private void SettingsMonitor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("Twitch"))
                UnsavedChanges = true;
        }

        private bool unsavedChanges = false;
        public bool UnsavedChanges
        {
            get => unsavedChanges;
            set => SetProperty(ref unsavedChanges, value);
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

        #region Twitch voting page
        private OptionsFile m_twitchFile = new OptionsFile("twitch.ini");

        private string GetTwitchConfigString(string defaultValue = null, [CallerMemberName]string propertyName = null)
        {
            return m_twitchFile.ReadValue(propertyName, defaultValue);
        }

        private bool GetTwitchConfigBool(bool defaultValue = false, [CallerMemberName]string propertyName = null)
        {
            return m_twitchFile.ReadValueBool(propertyName, defaultValue);
        }

        private int GetTwitchConfigInt(int defaultValue = 0, [CallerMemberName]string propertyName = null)
        {
            return m_twitchFile.ReadValueInt(propertyName, defaultValue);
        }

        private bool? enableTwitchVoting = null;
        public bool EnableTwitchVoting
        {
            get
            {
                if (enableTwitchVoting is null)
                    enableTwitchVoting = GetTwitchConfigBool();
                return enableTwitchVoting.Value;
            }
            set => SetProperty(ref enableTwitchVoting, value);
        }

        private string twitchChannelName = null;
        public string TwitchChannelName
        {
            get
            {
                if (twitchChannelName is null)
                    twitchChannelName = GetTwitchConfigString();
                return twitchChannelName;
            }
            set => SetProperty(ref twitchChannelName, value);
        }

        private string twitchUserName = null;
        public string TwitchUserName
        {
            get
            {
                if (twitchUserName is null)
                    twitchUserName = GetTwitchConfigString();
                return twitchUserName;
            }
            set => SetProperty(ref twitchUserName, value);
        }

        private string twitchChannelOAuth = null;
        public string TwitchChannelOAuth
        {
            get
            {
                if (twitchChannelOAuth is null)
                    twitchChannelOAuth = GetTwitchConfigString();
                return twitchChannelOAuth;
            }
            set => SetProperty(ref twitchChannelOAuth, value);
        }

        private string twitchVotingSecsBeforeVoting = null;
        public string TwitchVotingSecsBeforeVoting
        {
            get
            {
                if (twitchVotingSecsBeforeVoting is null)
                    twitchVotingSecsBeforeVoting = GetTwitchConfigString("0");
                return twitchVotingSecsBeforeVoting;
            }
            set => SetProperty(ref twitchVotingSecsBeforeVoting, value);
        }

        private int? twitchVotingOverlayMode = null;
        public int TwitchVotingOverlayMode
        {
            get
            {
                if (twitchVotingOverlayMode is null)
                    twitchVotingOverlayMode = GetTwitchConfigInt();
                return twitchVotingOverlayMode.Value;
            }
            set => SetProperty(ref twitchVotingOverlayMode, value);
        }

        private bool? twitchRandomEffectVoteableEnable = null;
        public bool TwitchRandomEffectVoteableEnable
        {
            get
            {
                if (twitchRandomEffectVoteableEnable is null)
                    twitchRandomEffectVoteableEnable = GetTwitchConfigBool(true);
                return twitchRandomEffectVoteableEnable.Value;
            }
            set => SetProperty(ref twitchRandomEffectVoteableEnable, value);
        }

        private string twitchPermittedUsernames = null;
        public string TwitchPermittedUsernames
        {
            get
            {
                if (twitchPermittedUsernames is null)
                    twitchPermittedUsernames = GetTwitchConfigString();
                return twitchPermittedUsernames;
            }
            set => SetProperty(ref twitchPermittedUsernames, value);
        }

        private bool? twitchVotingChanceSystem = null;
        public bool TwitchVotingChanceSystem
        {
            get
            {
                if (twitchVotingChanceSystem is null)
                    twitchVotingChanceSystem = GetTwitchConfigBool();
                return twitchVotingChanceSystem.Value;
            }
            set => SetProperty(ref twitchVotingChanceSystem, value);
        }

        private bool? twitchVotingChanceSystemRetainChance = null;
        public bool TwitchVotingChanceSystemRetainChance
        {
            get
            {
                if (twitchVotingChanceSystemRetainChance is null)
                    twitchVotingChanceSystemRetainChance = GetTwitchConfigBool();
                return twitchVotingChanceSystemRetainChance.Value;
            }
            set => SetProperty(ref twitchVotingChanceSystemRetainChance, value);
        }
        #endregion
    }
}
