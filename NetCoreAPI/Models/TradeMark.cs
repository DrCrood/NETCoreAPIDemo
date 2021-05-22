using NetCoreAPI.Interfaces;

namespace NetCoreAPI.Models
{
    public class TradeMark : ITradeMark
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
