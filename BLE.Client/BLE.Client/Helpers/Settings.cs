// Helpers/Settings.cs
using BLE.Client.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace BLE.Client.Helpers
{
  /// <summary>
  /// This is the Settings static class that can be used in your Core solution or in any
  /// of your client applications. All settings are laid out the same exact way with getters
  /// and setters. 
  /// </summary>
  public static class Settings
  {

    public static IDevice DEVICE = null;
    public static byte BRIGHTNESS = 255;
    public static byte SPEED = 255;
    public static Mode MODE = null;


    private static ISettings AppSettings
    {
      get
      {
        return CrossSettings.Current;
      }
    }

    #region Setting Constants

    private const string SettingsKey = "settings_key";
    private static readonly string SettingsDefault = string.Empty;

    #endregion


    

    public static string GeneralSettings
    {
      get
      {
        return AppSettings.GetValueOrDefault<string>(SettingsKey, SettingsDefault);
      }
      set
      {
        AppSettings.AddOrUpdateValue<string>(SettingsKey, value);
      }
    }

  }
}