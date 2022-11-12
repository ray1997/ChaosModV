﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Shared; 

using static ConfigApp.Effects;

namespace ConfigApp
{
    public partial class MainWindow : Window
    {
        private bool m_initializedTitle = false;
        
        private OptionsFile m_effectsFile = new OptionsFile("effects.ini");

        private Dictionary<string, TreeMenuItem> m_treeMenuItemsMap;
        private Dictionary<string, EffectData> m_effectDataMap;

        public MainWindow()
        {
            Init();
        }

        private void Init()
        {
            InitializeComponent();

            if (!m_initializedTitle)
            {
                m_initializedTitle = true;

                Title += " (v" + Info.VERSION + ")";
            }

            //ParseConfigFile();
            //ParseTwitchFile();

            InitEffectsTreeView();

            ParseEffectsFile();

            //InitTwitchTab();

            // Check write permissions
            try
            {
                if (!File.Exists(".writetest"))
                {
                    using (File.Create(".writetest"))
                    {
                    
                    }

                    File.Delete(".writetest");
                }
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is FileNotFoundException)
            {
                MessageBox.Show("No permissions to write in the current directory. Try to either run the program with admin privileges or allow write access to the current directory.",
                    "No Write Access", MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
            }

            Closing += UnsavedChangesWarning;
            twitch_user_channel_oauth.Password = ViewModel.TwitchChannelOAuth;
        }

        private void UnsavedChangesWarning(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ViewModel.UnsavedChanges)
            {
                e.Cancel = false;
                return;
            }

            if (ViewModel.UnsavedChanges)
            {
                var result = MessageBox.Show("Do you want to save changes that have been made before exit?",
                    "There is unsaved changes!", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    //TODO:Save changes
                }
            }
        }

        private EffectData GetEffectData(string effectId)
        {
            // Create EffectData in case effect wasn't saved yet
            if (!m_effectDataMap.TryGetValue(effectId, out EffectData effectData))
            {
                effectData = new EffectData(EffectsMap[effectId].IsShort ? EffectTimedType.TimedShort : EffectTimedType.TimedNormal, -1, 5, false, false, null, 0);

                m_effectDataMap.Add(effectId, effectData);
            }

            return effectData;
        }

        //private void ParseConfigFile()
        //{
        //    m_configFile.ReadFile();

        //    misc_user_effects_spawn_dur.Text = m_configFile.ReadValue("NewEffectSpawnTime", "30");
        //    misc_user_effects_timed_dur.Text = m_configFile.ReadValue("EffectTimedDur", "90");
        //    misc_user_effects_random_seed.Text = m_configFile.ReadValue("Seed");
        //    misc_user_effects_timed_short_dur.Text = m_configFile.ReadValue("EffectTimedShortDur", "30");
        //    misc_user_effects_clear_enable.IsOn = m_configFile.ReadValueBool("EnableClearEffectsShortcut", true);
        //    misc_user_effects_drawtimer_disable.IsOn = m_configFile.ReadValueBool("DisableTimerBarDraw", false);
        //    misc_user_effects_drawtext_disable.IsOn = m_configFile.ReadValueBool("DisableEffectTextDraw", false);
        //    misc_user_toggle_mod_shortcut.IsOn = m_configFile.ReadValueBool("EnableToggleModShortcut", true);
        //    misc_user_effects_menu_enable.IsOn = m_configFile.ReadValueBool("EnableDebugMenu", false);
        //    misc_user_effects_timer_pause_shortcut_enable.IsOn = m_configFile.ReadValueBool("EnablePauseTimerShortcut", false);
        //    misc_user_effects_max_running_effects.Text = m_configFile.ReadValue("MaxParallelRunningEffects", "99");
        //    if (m_configFile.HasKey("EffectTimerColor"))
        //    {
        //        misc_user_effects_timer_color.SelectedColor = (Color)ColorConverter.ConvertFromString(m_configFile.ReadValue("EffectTimerColor"));
        //    }
        //    if (m_configFile.HasKey("EffectTextColor"))
        //    {
        //        misc_user_effects_text_color.SelectedColor = (Color)ColorConverter.ConvertFromString(m_configFile.ReadValue("EffectTextColor"));
        //    }
        //    if (m_configFile.HasKey("EffectTimedTimerColor"))
        //    {
        //        misc_user_effects_effect_timer_color.SelectedColor = (Color)ColorConverter.ConvertFromString(m_configFile.ReadValue("EffectTimedTimerColor"));
        //    }
        //    misc_user_effects_disable_startup.IsOn = m_configFile.ReadValueBool("DisableStartup", false);
        //    misc_user_effects_enable_group_weighting.IsOn = m_configFile.ReadValueBool("EnableGroupWeightingAdjustments", true);
        //    misc_user_effects_enable_failsafe.IsOn = m_configFile.ReadValueBool("EnableFailsafe", true);
        //    misc_user_anti_softlock_shortcut.IsOn = m_configFile.ReadValueBool("EnableAntiSoftlockShortcut", true);

        //    // Meta Effects
        //    meta_effects_spawn_dur.Text = m_configFile.ReadValue("NewMetaEffectSpawnTime", "600");
        //    meta_effects_timed_dur.Text = m_configFile.ReadValue("MetaEffectDur", "95");
        //    meta_effects_short_timed_dur.Text = m_configFile.ReadValue("MetaShortEffectDur", "65");
        //}

        //private void WriteConfigFile()
        //{
        //    m_configFile.WriteValue("NewEffectSpawnTime", misc_user_effects_spawn_dur.Text);
        //    m_configFile.WriteValue("EffectTimedDur", misc_user_effects_timed_dur.Text);
        //    m_configFile.WriteValue("Seed", misc_user_effects_random_seed.Text);
        //    m_configFile.WriteValue("EffectTimedShortDur", misc_user_effects_timed_short_dur.Text);
        //    m_configFile.WriteValue("EnableClearEffectsShortcut", misc_user_effects_clear_enable.IsOn);
        //    m_configFile.WriteValue("DisableTimerBarDraw", misc_user_effects_drawtimer_disable.IsOn);
        //    m_configFile.WriteValue("DisableEffectTextDraw", misc_user_effects_drawtext_disable.IsOn);
        //    m_configFile.WriteValue("EnableToggleModShortcut", misc_user_toggle_mod_shortcut.IsOn);
        //    m_configFile.WriteValue("EnableDebugMenu", misc_user_effects_menu_enable.IsOn);
        //    m_configFile.WriteValue("EnablePauseTimerShortcut", misc_user_effects_timer_pause_shortcut_enable.IsOn);
        //    m_configFile.WriteValue("EffectTimerColor", misc_user_effects_timer_color.SelectedColor.ToString());
        //    m_configFile.WriteValue("EffectTextColor", misc_user_effects_text_color.SelectedColor.ToString());
        //    m_configFile.WriteValue("EffectTimedTimerColor", misc_user_effects_effect_timer_color.SelectedColor.ToString());
        //    m_configFile.WriteValue("DisableStartup", misc_user_effects_disable_startup.IsOn);
        //    m_configFile.WriteValue("EnableGroupWeightingAdjustments", misc_user_effects_enable_group_weighting.IsOn);
        //    m_configFile.WriteValue("EnableFailsafe", misc_user_effects_enable_failsafe.IsOn);
        //    int runningEffects;
        //    if (int.TryParse(misc_user_effects_max_running_effects.Text, out runningEffects) && runningEffects > 0)
        //    {
        //        m_configFile.WriteValue("MaxParallelRunningEffects", misc_user_effects_max_running_effects.Text);
        //    }
        //    m_configFile.WriteValue("EnableAntiSoftlockShortcut", misc_user_anti_softlock_shortcut.IsOn);

        //    // Meta Effects
        //    m_configFile.WriteValue("NewMetaEffectSpawnTime", meta_effects_spawn_dur.Text);
        //    m_configFile.WriteValue("MetaEffectDur", meta_effects_timed_dur.Text);
        //    m_configFile.WriteValue("MetaShortEffectDur", meta_effects_short_timed_dur.Text);
            
        //    m_configFile.WriteFile();
        //}

        //private void ParseTwitchFile()
        //{
        //    m_twitchFile.ReadFile();

        //    twitch_user_agreed.IsChecked = m_twitchFile.ReadValueBool("EnableTwitchVoting", false);
        //    twitch_user_channel_name.Text = m_twitchFile.ReadValue("TwitchChannelName");
        //    twitch_user_user_name.Text = m_twitchFile.ReadValue("TwitchUserName");
        //    twitch_user_channel_oauth.Password = m_twitchFile.ReadValue("TwitchChannelOAuth");
        //    twitch_user_effects_secs_before_chat_voting.Text = m_twitchFile.ReadValue("TwitchVotingSecsBeforeVoting", "0");
        //    twitch_user_overlay_mode.SelectedIndex = m_twitchFile.ReadValueInt("TwitchVotingOverlayMode", 0);
        //    twitch_user_chance_system_enable.IsChecked = m_twitchFile.ReadValueBool("TwitchVotingChanceSystem", false);
        //    twitch_user_chance_system_retain_chance_enable.IsChecked = m_twitchFile.ReadValueBool("TwitchVotingChanceSystemRetainChance", true);
        //    twitch_user_random_voteable_enable.IsOn = m_twitchFile.ReadValueBool("TwitchRandomEffectVoteableEnable", true);
        //    twitch_permitted_usernames.Text = m_twitchFile.ReadValue("TwitchPermittedUsernames");
        //}

        //private void WriteTwitchFile()
        //{
        //    m_twitchFile.WriteValue("EnableTwitchVoting", twitch_user_agreed.IsChecked.Value);
        //    m_twitchFile.WriteValue("TwitchChannelName", twitch_user_channel_name.Text);
        //    m_twitchFile.WriteValue("TwitchUserName", twitch_user_user_name.Text);
        //    m_twitchFile.WriteValue("TwitchChannelOAuth", twitch_user_channel_oauth.Password);
        //    m_twitchFile.WriteValue("TwitchVotingSecsBeforeVoting", twitch_user_effects_secs_before_chat_voting.Text);
        //    m_twitchFile.WriteValue("TwitchVotingOverlayMode", twitch_user_overlay_mode.SelectedIndex);
        //    m_twitchFile.WriteValue("TwitchVotingChanceSystem", twitch_user_chance_system_enable.IsChecked.Value);
        //    m_twitchFile.WriteValue("TwitchVotingChanceSystemRetainChance", twitch_user_chance_system_retain_chance_enable.IsChecked.Value);
        //    m_twitchFile.WriteValue("TwitchRandomEffectVoteableEnable", twitch_user_random_voteable_enable.IsOn);
        //    m_twitchFile.WriteValue("TwitchPermittedUsernames", twitch_permitted_usernames.Text);

        //    m_twitchFile.WriteFile();
        //}

        private void ParseEffectsFile()
        {
            m_effectsFile.ReadFile();

            foreach (string key in m_effectsFile.GetKeys())
            {
                string value = m_effectsFile.ReadValue(key);

                // Split by comma, ignoring commas in between quotation marks
                string[] values = Regex.Split(value, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                if (!EffectsMap.TryGetValue(key, out EffectInfo effectInfo))
                {
                    continue;
                }

                EffectTimedType effectTimedType = effectInfo.IsShort ? EffectTimedType.TimedShort : EffectTimedType.TimedNormal;
                int effectTimedTime = -1;
                int effectWeight = 5;
                bool effectPermanent = false;
                bool effectExcludedFromVoting = false;
                string effectCustomName = null;
                int effectShortcut = 0;

                // Compatibility checks, previous versions had less options
                if (values.Length >= 4)
                {
                    Enum.TryParse(values[1], out effectTimedType);
                    int.TryParse(values[2], out effectTimedTime);
                    int.TryParse(values[3], out effectWeight);

                    if (values.Length >= 5)
                    {
                        int tmp;

                        int.TryParse(values[4], out tmp);
                        effectPermanent = tmp != 0;

                        if (values.Length >= 6)
                        {
                            int.TryParse(values[5], out tmp);
                            effectExcludedFromVoting = tmp != 0;

                            if (values.Length >= 7)
                            {
                                effectCustomName = values[6] == "0" ? null : values[6].Trim('\"');
                                if (values.Length >= 8)
                                {
                                    int.TryParse(values[7], out effectShortcut);
                                }
                            }
                        }
                    }
                }

                int.TryParse(values[0], out int enabled);
                m_treeMenuItemsMap[key].IsChecked = enabled == 0 ? false : true;

                m_effectDataMap.Add(key, new EffectData(effectTimedType, effectTimedTime, effectWeight, effectPermanent,
                    effectExcludedFromVoting, effectCustomName, effectShortcut));
            }
        }

        private void WriteEffectsFile()
        {
            foreach (var pair in EffectsMap)
            {
                EffectData effectData = GetEffectData(pair.Key);

                m_effectsFile.WriteValue(pair.Key, $"{(m_treeMenuItemsMap[pair.Key].IsChecked ? 1 : 0)}"
                    + $",{(effectData.TimedType == EffectTimedType.TimedNormal ? 0 : 1)}"
                    + $",{effectData.CustomTime},{effectData.WeightMult},{(effectData.Permanent ? 1 : 0)},{(effectData.ExcludedFromVoting ? 1 : 0)}"
                    + $",\"{(string.IsNullOrEmpty(effectData.CustomName) ? "" : effectData.CustomName)}\""
                    + $",{(effectData.Shortcut)}");
            }

            m_effectsFile.WriteFile();
        }

        private void InitEffectsTreeView()
        {
            m_treeMenuItemsMap = new Dictionary<string, TreeMenuItem>();
            m_effectDataMap = new Dictionary<string, EffectData>();

            TreeMenuItem playerParentItem = new TreeMenuItem("Player");
            TreeMenuItem vehicleParentItem = new TreeMenuItem("Vehicle");
            TreeMenuItem pedsParentItem = new TreeMenuItem("Peds");
            TreeMenuItem screenParentItem = new TreeMenuItem("Screen");
            TreeMenuItem timeParentItem = new TreeMenuItem("Time");
            TreeMenuItem weatherParentItem = new TreeMenuItem("Weather");
            TreeMenuItem miscParentItem = new TreeMenuItem("Misc");
            TreeMenuItem metaParentItem = new TreeMenuItem("Meta");

            var sortedEffects = new SortedDictionary<string, Tuple<string, EffectCategory>>();

            foreach (var pair in EffectsMap)
            {
                sortedEffects.Add(pair.Value.Name, new Tuple<string, EffectCategory>(pair.Key, pair.Value.EffectCategory));
            }
            
            foreach (var effect in sortedEffects)
            {
                var effectTuple = effect.Value;

                TreeMenuItem menuItem = new TreeMenuItem(effect.Key);
                m_treeMenuItemsMap.Add(effectTuple.Item1, menuItem);

                switch (effectTuple.Item2)
                {
                    case EffectCategory.Player:
                        playerParentItem.AddChild(menuItem);
                        break;
                    case EffectCategory.Vehicle:
                        vehicleParentItem.AddChild(menuItem);
                        break;
                    case EffectCategory.Peds:
                        pedsParentItem.AddChild(menuItem);
                        break;
                    case EffectCategory.Screen:
                        screenParentItem.AddChild(menuItem);
                        break;
                    case EffectCategory.Time:
                        timeParentItem.AddChild(menuItem);
                        break;
                    case EffectCategory.Weather:
                        weatherParentItem.AddChild(menuItem);
                        break;
                    case EffectCategory.Misc:
                        miscParentItem.AddChild(menuItem);
                        break;
                    case EffectCategory.Meta:
                        metaParentItem.AddChild(menuItem);
                        break;
                }
            }

            effects_user_effects_tree_view.Items.Clear();
            effects_user_effects_tree_view.Items.Add(playerParentItem);
            effects_user_effects_tree_view.Items.Add(vehicleParentItem);
            effects_user_effects_tree_view.Items.Add(pedsParentItem);
            effects_user_effects_tree_view.Items.Add(screenParentItem);
            effects_user_effects_tree_view.Items.Add(timeParentItem);
            effects_user_effects_tree_view.Items.Add(weatherParentItem);
            effects_user_effects_tree_view.Items.Add(miscParentItem);

            meta_effects_tree_view.Items.Clear();
            meta_effects_tree_view.Items.Add(metaParentItem);
            
        }

        //void InitTwitchTab()
        //{
        //    TwitchTabHandleAgreed();
        //}

        //void TwitchTabHandleAgreed()
        //{
        //    bool agreed = twitch_user_agreed.IsOn.GetValueOrDefault();

        //    twitch_user_channel_name.IsEnabled = agreed;
        //    twitch_user_channel_oauth.IsEnabled = agreed;
        //    twitch_user_user_name.IsEnabled = agreed;
        //    twitch_user_effects_secs_before_chat_voting.IsEnabled = agreed;
        //    twitch_user_overlay_mode.IsEnabled = agreed;
        //    twitch_user_chance_system_enable_label.IsEnabled = agreed;
        //    twitch_user_chance_system_enable.IsEnabled = agreed;
        //    twitch_user_chance_system_retain_chance_enable_label.IsEnabled = agreed;
        //    twitch_user_chance_system_retain_chance_enable.IsEnabled = agreed;
        //    twitch_user_random_voteable_enable.IsEnabled = agreed;
        //    twitch_permitted_usernames.IsEnabled = agreed;
        //}

        private void OnlyNumbersPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utils.HandleOnlyNumbersPreviewTextInput(e);
        }

        private void NoSpacePreviewKeyDown(object sender, KeyEventArgs e)
        {
            Utils.HandleNoSpacePreviewKeyDown(e);
        }

        private void NoCopyPastePreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Utils.HandleNoCopyPastePreviewExecuted(e);
        }

        private void user_save_Click(object sender, RoutedEventArgs e)
        {
            //TODO:Turn this to relaycommand
            //WriteConfigFile();
            //WriteTwitchFile();
            WriteEffectsFile();

            // Reload saved config to show the "new" (saved) settings
            //ParseConfigFile();
            //ParseTwitchFile();

            //TODO:Turn this into in-app notification somehow
            MessageBox.Show("Saved config!\nMake sure to press CTRL + L in-game twice if mod is already running to reload the config.", "ChaosModV", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void user_reset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to reset your config?", "ChaosModV",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                //TODO:Reset function relaycommand
                //m_configFile.ResetFile();

                m_effectsFile.ResetFile();

                result = MessageBox.Show("Do you want to reset your Twitch settings too?", "ChaosModV",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    //m_twitchFile.ResetFile();
                    //ParseTwitchFile();

                    // Ensure all options are disabled in twitch tab again
                    //TwitchTabHandleAgreed();
                }

                Init();

                MessageBox.Show("Config has been reverted to default settings!", "ChaosModV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void twitch_user_agreed_Clicked(object sender, RoutedEventArgs e)
        {
            //TwitchTabHandleAgreed();
        }

        private void effect_user_config_Click(object sender, RoutedEventArgs e)
        {
            TreeMenuItem curTreeMenuItem = (TreeMenuItem)((TreeViewItem)((Grid)((Border)((ContentPresenter)((StackPanel)((Button)sender).Parent).TemplatedParent).Parent).Parent).TemplatedParent).DataContext;

            string effectId = null;
            foreach (var pair in m_treeMenuItemsMap)
            {
                if (pair.Value == curTreeMenuItem)
                {
                    effectId = pair.Key;

                    break;
                }
            }

            if (effectId != null)
            {
                var effectInfo = EffectsMap[effectId];
                var effectData = GetEffectData(effectId);

                var effectConfig = new EffectConfig(effectId, effectData, effectInfo);
                effectConfig.ShowDialog();

                if (effectConfig.IsSaved)
                {
                    effectData.TimedType = effectConfig.effectconf_timer_type_enable.IsChecked.Value ? (EffectTimedType)effectConfig.effectconf_timer_type.SelectedIndex
                        : effectInfo.IsShort ? EffectTimedType.TimedShort : EffectTimedType.TimedNormal;
                    effectData.CustomTime = effectConfig.effectconf_timer_time_enable.IsChecked.Value
                        ? effectConfig.effectconf_timer_time.Text.Length > 0 ? int.Parse(effectConfig.effectconf_timer_time.Text) : -1 : -1;
                    effectData.Permanent = effectConfig.effectconf_timer_permanent_enable.IsChecked.Value;
                    effectData.WeightMult = effectConfig.effectconf_effect_weight_mult.SelectedIndex + 1;
                    effectData.ExcludedFromVoting = effectConfig.effectconf_exclude_voting_enable.IsChecked.Value;
                    effectData.CustomName = effectConfig.effectconf_effect_custom_name.Text.Trim();
                    Key shortcut = (Key)effectConfig.effectconf_effect_shortcut_combo.SelectedItem;
                    effectData.Shortcut = KeyInterop.VirtualKeyFromKey(shortcut);
                }
            }
        }

        private void contribute_modpage_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.gta5-mods.com/scripts/chaos-mod-v-beta");
        }

        private void contribute_github_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/gta-chaos-mod/ChaosModV");
        }

        private void contribute_donate_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://paypal.me/EmrCue");
        }

        private void contribute_discord_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/w2tDeKVaF9");
        }

        private void RowListingUpdate(object sender, SizeChangedEventArgs e)
        {
            ViewModel.ListingTwoRow = e.NewSize.Width > 720;
            //Scroll to top on size chage
            if (ViewModel.ListingTwoRow)
            {
                (sender as ScrollViewer).ScrollToTop();
            }
        }

        private void twitch_user_channel_oauth_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is null)
                return;
            ViewModel.TwitchChannelOAuth = (sender as PasswordBox).Password;
        }
    }
}