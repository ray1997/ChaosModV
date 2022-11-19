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
            m_configFile.ReadFile();
            this.PropertyChanged += SettingsMonitor;
        }

        private void SettingsMonitor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UnsavedChanges))
                return;
            else if (e.PropertyName == nameof(IsModUpToDate))
                return;
            else if (e.PropertyName == nameof(ListingTwoRow))
                return;
            else if (e.PropertyName == nameof(ModStatus))
                return;
            else if (e.PropertyName == nameof(ModStatusIcon))
                return;
            else if (e.PropertyName == nameof(AllowToOpenModPage))
                return;
            else if (e.PropertyName == nameof(SecondColumnSize))
                return;
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

        #region Config page
        private OptionsFile m_configFile = new OptionsFile("config.ini");

        private string GetConfigString(string defaultValue = null, [CallerMemberName] string propertyName = null)
        {
            return m_configFile.ReadValue(propertyName, defaultValue);
        }

        private bool GetConfigBool(bool defaultValue = false, [CallerMemberName] string propertyName = null)
        {
            return m_configFile.ReadValueBool(propertyName, defaultValue);
        }

        private int GetConfigInt(int defaultValue = 0, [CallerMemberName] string propertyName = null)
        {
            return m_configFile.ReadValueInt(propertyName, defaultValue);
        }

        private int? newEffectSpawnTime = null;
        public int NewEffectSpawnTime
        {
            get
            {
                if (newEffectSpawnTime is null)
                    newEffectSpawnTime = GetConfigInt(30);
                return newEffectSpawnTime.Value;
            }
            set => SetProperty(ref newEffectSpawnTime, value);
        }

        private int? effectTimedDur = null;
        public int EffectTimedDur
        {
            get
            {
                if (effectTimedDur is null)
                    effectTimedDur = GetConfigInt(90);
                return effectTimedDur.Value;
            }
            set => SetProperty(ref effectTimedDur, value);
        }

        private int? effectTimedShortDur = null;
        public int EffectTimedShortDur
        {
            get
            {
                if (effectTimedShortDur is null)
                    effectTimedShortDur = GetConfigInt(30);
                return effectTimedShortDur.Value;
            }
            set => SetProperty(ref effectTimedShortDur, value);
        }

        private string seed = null;
        public string Seed
        {
            get
            {
                if (seed is null)
                    seed = GetConfigString();
                return seed;
            }
            set => SetProperty(ref seed, value);
        }

        private int? maxParallelRunningEffects = null;
        public int MaxParallelRunningEffects
        {
            get
            {
                if (maxParallelRunningEffects is null)
                    maxParallelRunningEffects = GetConfigInt(99);
                return maxParallelRunningEffects.Value;
            }
            set => SetProperty(ref maxParallelRunningEffects, value);
        }

        private Color? effectTimerColor = null;
        public Color EffectTimerColor
        {
            get
            {
                if (effectTimerColor is null)
                    effectTimerColor = (Color?)ColorConverter.ConvertFromString(GetConfigString("#4040FF"));
                return effectTimerColor.Value;
            }
            set => SetProperty(ref effectTimerColor, value);
        }

        private Color? effectTextColor = null;
        public Color EffectTextColor
        {
            get
            {
                if (effectTextColor is null)
                    effectTextColor = (Color?)ColorConverter.ConvertFromString(GetConfigString("#FFFFFF"));
                return effectTextColor.Value;
            }
            set => SetProperty(ref effectTextColor, value);
        }

        private Color? effectTimedTimerColor = null;
        public Color EffectTimedTimerColor
        {
            get
            {
                if (effectTimedTimerColor is null)
                    effectTimedTimerColor = (Color?)ColorConverter.ConvertFromString(GetConfigString("#B4B4B4"));
                return effectTimedTimerColor.Value;
            }
            set => SetProperty(ref effectTimedTimerColor, value);
        }

        private bool? disableStartup = null;
        public bool DisableStartup
        {
            get
            {
                if (disableStartup is null)
                    disableStartup = GetConfigBool();
                return disableStartup.Value;
            }
            set => SetProperty(ref disableStartup, value);
        }

        private bool? disableTimerBarDraw = null;
        public bool DisableTimerBarDraw
        {
            get
            {
                if (disableTimerBarDraw is null)
                    disableTimerBarDraw = GetConfigBool();
                return disableTimerBarDraw.Value;
            }
            set => SetProperty(ref disableTimerBarDraw, value);
        }

        private bool? disableEffectTextDraw = null;
        public bool DisableEffectTextDraw
        {
            get
            {
                if (disableTimerBarDraw is null)
                    disableEffectTextDraw = GetConfigBool();
                return disableEffectTextDraw.Value;
            }
            set => SetProperty(ref disableEffectTextDraw, value);
        }

        private bool? enableClearEffectsShortcut = null;
        public bool EnableClearEffectsShortcut
        {
            get
            {
                if (enableClearEffectsShortcut is null)
                    enableClearEffectsShortcut = GetConfigBool(true);
                return enableClearEffectsShortcut.Value;
            }
            set => SetProperty(ref enableClearEffectsShortcut, value);
        }

        private bool? enableToggleModShortcut = null;
        public bool EnableToggleModShortcut
        {
            get
            {
                if (enableToggleModShortcut is null)
                    enableToggleModShortcut = GetConfigBool(true);
                return enableToggleModShortcut.Value;
            }
            set => SetProperty(ref enableToggleModShortcut, value);
        }

        private bool? enableDebugMenu = null;
        public bool EnableDebugMenu
        {
            get
            {
                if (enableDebugMenu is null)
                    enableDebugMenu = GetConfigBool();
                return enableDebugMenu.Value;
            }
            set => SetProperty(ref enableDebugMenu, value);
        }

        private bool? enablePauseTimerShortcut = null;
        public bool EnablePauseTimerShortcut
        {
            get
            {
                if (enablePauseTimerShortcut is null)
                    enablePauseTimerShortcut = GetConfigBool();
                return enablePauseTimerShortcut.Value;
            }
            set => SetProperty(ref enablePauseTimerShortcut, value);
        }

        private bool? enableAntiSoftlockShortcut = null;
        public bool EnableAntiSoftlockShortcut
        {
            get
            {
                if (enableAntiSoftlockShortcut is null)
                    enableAntiSoftlockShortcut = GetConfigBool(true);
                return enableAntiSoftlockShortcut.Value;
            }
            set => SetProperty(ref enableAntiSoftlockShortcut, value);
        }

        private bool? enableGroupWeightingAdjustments = null;
        public bool EnableGroupWeightingAdjustments
        {
            get
            {
                if (enableGroupWeightingAdjustments is null)
                    enableGroupWeightingAdjustments = GetConfigBool(true);
                return enableGroupWeightingAdjustments.Value;
            }
            set => SetProperty(ref enableGroupWeightingAdjustments, value);
        }

        private bool? enableFailsafe = null;
        public bool EnableFailsafe
        {
            get
            {
                if (enableFailsafe is null)
                    enableFailsafe = GetConfigBool(true);
                return enableFailsafe.Value;
            }
            set => SetProperty(ref enableFailsafe, value);
        }

        //Meta configs
        private int? newMetaEffectSpawnTime = null;
        public int NewMetaEffectSpawnTime
        {
            get
            {
                if (newMetaEffectSpawnTime is null)
                    newMetaEffectSpawnTime = GetConfigInt(600);
                return newMetaEffectSpawnTime.Value;
            }
            set => SetProperty(ref newMetaEffectSpawnTime, value);
        }

        private int? metaEffectDur = null;
        public int MetaEffectDur
        {
            get
            {
                if (metaEffectDur is null)
                    metaEffectDur = GetConfigInt(95);
                return metaEffectDur.Value;
            }
            set => SetProperty(ref metaEffectDur, value);
        }

        private int? metaShortEffectDur = null;
        public int MetaShortEffectDur
        {
            get
            {
                if (metaShortEffectDur is null)
                    metaShortEffectDur = GetConfigInt(65);
                return metaShortEffectDur.Value;
            }
            set => SetProperty(ref metaShortEffectDur, value);
        }
        #endregion

        #region Effects page

        #endregion
    }
}
