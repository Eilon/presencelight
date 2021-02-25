﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PresenceLight.Services;
using PresenceLight.Telemetry;


using ModernWpf;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage
    {
        private MainWindowModern parentWindow;
        private DiagnosticsClient _diagClient;
        private MediatR.IMediator _mediator;

        ILogger _logger;
        public SettingsPage()
        {
            _mediator = App.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            _diagClient = App.ServiceProvider.GetRequiredService<DiagnosticsClient>();
            _logger = App.ServiceProvider.GetRequiredService<ILogger<SettingsPage>>();

            parentWindow = System.Windows.Application.Current.Windows.OfType<MainWindowModern>().First();

            InitializeComponent();

         

            switch (SettingsHandlerBase.Config.Theme)
            {
                case "Light":
                    themeLight.IsChecked = true;
                    break;
                case "Dark":
                    themeDark.IsChecked = true;
                    break;
                case "Use system setting":
                    themeDefault.IsChecked = true;
                    break;
                default:
                    themeDefault.IsChecked = true;
                    break;
            }
            if (SettingsHandlerBase.Config.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {
                 White.IsChecked = true;
            }

            switch (SettingsHandlerBase.Config.LightSettings.HoursPassedStatus)
            {
                case "Keep":
                    HourStatusKeep.IsChecked = true;
                    break;
                case "White":
                    HourStatusWhite.IsChecked = true;
                    break;
                case "Off":
                   HourStatusOff.IsChecked = true;
                    break;
                default:
                  HourStatusKeep.IsChecked = true;
                    break;
            }
            if (SettingsHandlerBase.Config.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {
                
                White.IsChecked = true;
            }
        }

        private void OnThemeToggle(object sender, RoutedEventArgs e)
        {
            if (themeDark.IsChecked.Value)
            {
                SettingsHandlerBase.Config.Theme = "Dark";
            }

            if (themeDefault.IsChecked.Value)
            {
                SettingsHandlerBase.Config.Theme = "Use default setting";
            }

            if (themeLight.IsChecked.Value)
            {
                SettingsHandlerBase.Config.Theme = "Light";
            }

            switch (SettingsHandlerBase.Config.Theme)
            {
                case "Light":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    break;
                case "Dark":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    break;
                case "Use system setting":
                    ThemeManager.Current.ApplicationTheme = null;
                    break;
                default:
                    ThemeManager.Current.ApplicationTheme = null;
                    break;
            }
        }


        private async Task LoadSettings()
        {
            try
            {
                bool useWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());
                bool IsInWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());

                if (useWorkingHours)
                {
                    pnlWorkingHours.Visibility = Visibility.Visible;

                    SettingsHandlerBase.SyncOptions();
                }
                else
                {
                    pnlWorkingHours.Visibility = Visibility.Collapsed;
                    SettingsHandlerBase.SyncOptions();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Loading Settings");

                _diagClient.TrackException(e);
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSettings.IsEnabled = false;

                if (Transparent.IsChecked == true)
                {
                    SettingsHandlerBase.Config.IconType = "Transparent";
                }
                else
                {
                    SettingsHandlerBase.Config.IconType = "White";
                }

                if (HourStatusKeep.IsChecked == true)
                {
                    SettingsHandlerBase.Config.LightSettings.HoursPassedStatus = "Keep";
                }

                if (HourStatusOff.IsChecked == true)
                {
                    SettingsHandlerBase.Config.LightSettings.HoursPassedStatus = "Off";
                }

                if (HourStatusWhite.IsChecked == true)
                {
                    SettingsHandlerBase.Config.LightSettings.HoursPassedStatus = "White";
                }
                SettingsHandlerBase.Config.LightSettings.DefaultBrightness = Convert.ToInt32(brightness.Value);

                CheckAAD();


                SetWorkingDays();

                SettingsHandlerBase.SyncOptions();
                await parentWindow._mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

                lblSettingSaved.Visibility = Visibility.Visible;
                btnSettings.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Saving Settings");
                _diagClient.TrackException(ex);

            }
        }

        private void SetWorkingDays()
        {
            List<string> days = new List<string>();

            if (Monday.IsChecked != null && Monday.IsChecked.Value)
            {
                days.Add("Monday");
            }

            if (Tuesday.IsChecked != null && Tuesday.IsChecked.Value)
            {
                days.Add("Tuesday");
            }

            if (Wednesday.IsChecked != null && Wednesday.IsChecked.Value)
            {
                days.Add("Wednesday");
            }

            if (Thursday.IsChecked != null && Thursday.IsChecked.Value)
            {
                days.Add("Thursday");
            }

            if (Friday.IsChecked != null && Friday.IsChecked.Value)
            {
                days.Add("Friday");
            }

            if (Saturday.IsChecked != null && Saturday.IsChecked.Value)
            {
                days.Add("Saturday");
            }

            if (Sunday.IsChecked != null && Sunday.IsChecked.Value)
            {
                days.Add("Sunday");
            }

            SettingsHandlerBase.Config.LightSettings.WorkingDays = string.Join("|", days);
        }

        private async void CheckAAD()
        {
            try
            {
                SettingsHandlerBase.SyncOptions();

                //landingPage.configErrorPanel.Visibility = Visibility.Hidden;

                //if (landingPage.dataPanel.Visibility != Visibility.Visible)
                //{
                //    landingPage.signInPanel.Visibility = Visibility.Visible;
                //}

                if (!await parentWindow._mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
                {
                    await parentWindow._mediator.Send(new Core.GraphServices.InitializeCommand()
                    {
                        Client = parentWindow._graphservice.GetAuthenticatedGraphClient()
                    });

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Checking Azure Active Directory");
                _diagClient.TrackException(e);
            }
        }

        private void PopulateWorkingDays()
        {
            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingDays))
            {
                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Monday", StringComparison.OrdinalIgnoreCase))
                {
                    Monday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Tuesday", StringComparison.OrdinalIgnoreCase))
                {
                    Tuesday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Wednesday", StringComparison.OrdinalIgnoreCase))
                {
                    Wednesday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Thursday", StringComparison.OrdinalIgnoreCase))
                {
                    Thursday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Friday", StringComparison.OrdinalIgnoreCase))
                {
                    Friday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Saturday", StringComparison.OrdinalIgnoreCase))
                {
                    Saturday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Sunday", StringComparison.OrdinalIgnoreCase))
                {
                    Sunday.IsChecked = true;
                }
            }
        }

        private async void cbSyncLights(object sender, RoutedEventArgs e)
        {
            if (!SettingsHandlerBase.Config.LightSettings.SyncLights)
            {
                await _mediator.Send(new Services.SetColorCommand { Color = "Off" }).ConfigureAwait(true);
             
                 var landingPage = System.Windows.Application.Current.Windows.OfType<Pages.ProfilePage>().First();

                landingPage.turnOffButton.Visibility = Visibility.Collapsed;
                landingPage.turnOnButton.Visibility = Visibility.Visible;
            }

            SettingsHandlerBase.SyncOptions();
            await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
            e.Handled = true;
        }

        private async void cbUseDefaultBrightnessChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.UseDefaultBrightness)
            {
                pnlDefaultBrightness.Visibility = Visibility.Visible;
            }
            else
            {
                pnlDefaultBrightness.Visibility = Visibility.Collapsed;
            }

            SettingsHandlerBase.SyncOptions();
            await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
            e.Handled = true;
        }

        private async void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime))
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime))
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }
            bool useWorkingHours = await parentWindow._mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());

            if (useWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
            }

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
             
            if (SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue)
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue)
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }
    }
}
