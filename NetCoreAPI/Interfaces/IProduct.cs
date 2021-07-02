using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNET5API.Interfaces
{
    public interface IProduct
    {
        //Properties in interface are not auto-properties as in classes. NO backing private fields here.
        public int Id { get; set; }
        string Name { get; set; }
        public int Inventory { get; set; }
        public Double Price { get; set; }
        ITradeMark tradeMark { get; set; }
        bool NeedReplenishment(string name);
        Task<int> GetInventoryAsync();
        int GetDiscountInPercentage(int quantity);
        bool TryParse(string value, out IProduct product);
        int AddInventory(int number);
    }
}
