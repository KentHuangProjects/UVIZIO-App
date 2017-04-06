using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    public static class KnownCharacteristics
    {
        public static Guid RFDUINO_READ         = Guid.ParseExact("aba8a707-f28c-11e6-bc64-92361f002671", "d");
        public static Guid RFDUINO_WRITE        = Guid.ParseExact("aba8a708-f28c-11e6-bc64-92361f002671", "d");
        public static Guid RFDUINO_DISCONNECT   = Guid.ParseExact("aba8a709-f28c-11e6-bc64-92361f002671", "d");

        static KnownCharacteristics()
        {
            //ToDo do we need a lock here?
            LookupTable = Characteristics.ToDictionary(c => c.Id, c => c);
        }


        public static KnownCharacteristic Lookup(Guid id)
        {
            return LookupTable.ContainsKey(id) ? LookupTable[id] : new KnownCharacteristic("Unknown characteristic", Guid.Empty);
        }

        private static readonly Dictionary<Guid, KnownCharacteristic> LookupTable;

        /// <summary>
        /// https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicsHome.aspx
        /// </summary>
        private static readonly List<KnownCharacteristic> Characteristics = new List<KnownCharacteristic>()
        {
            
            new KnownCharacteristic("RFduino Generic Read",         RFDUINO_READ),
            new KnownCharacteristic("RFduino Generic Write",        RFDUINO_WRITE),
            new KnownCharacteristic("RFduino Generic Disconnect",   RFDUINO_DISCONNECT),
        };
    }
}