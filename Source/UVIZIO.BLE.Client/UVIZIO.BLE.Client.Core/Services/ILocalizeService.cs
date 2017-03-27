using System.Globalization;

namespace UVIZIO.BLE.Client.Core.Services
{
    public interface ILocalizeService
    {
        CultureInfo GetCurrentCultureInfo();
    }
}
