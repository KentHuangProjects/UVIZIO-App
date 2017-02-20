using SQLite.Net.Attributes;

namespace UVIZIO.BLE.Client.Core.Model
{
    public class BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
    }
}
