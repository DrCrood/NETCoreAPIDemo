using DotNET5API.Interfaces;

namespace DotNET5API.Models
{
    public class TradeMark : ITradeMark
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
