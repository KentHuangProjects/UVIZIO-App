using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    // Source: https://developer.bluetooth.org/gatt/services/Pages/ServicesHome.aspx
    public static class KnownServices
    {
        private static readonly Dictionary<Guid, KnownService> LookupTable;

        public static readonly Guid RFDUINO_SERVICE = Guid.ParseExact("aba8a706-f28c-11e6-bc64-92361f002671", "d");

        static KnownServices()
        {
            LookupTable = Services.ToDictionary(s => s.Id, s => s);
        }

        public static KnownService Lookup(Guid id)
        {
            return LookupTable.ContainsKey(id) ? LookupTable[id] : new KnownService("Unknown Service", Guid.Empty);
        }

        private static readonly IList<KnownService> Services = new List<KnownService>()
        {
            

            new KnownService("RFduino Generic Service", RFDUINO_SERVICE),
        };

    }
}