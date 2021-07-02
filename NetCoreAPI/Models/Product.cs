using System;
using DotNET5API.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace DotNET5API.Models
{
    public class Product: IProduct
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Inventory { get; set; }
        [Required]
        public Double Price { get; set; }
        [NotMapped]
        public ITradeMark tradeMark { get; set; }
        public int BulkBuyNumber { get; set; }

        public Product()
        {
            BulkBuyNumber = 10;
        }
        public int AddInventory(int number)
        {
            if(number > 0)
            {
                this.Inventory += number;
                return this.Inventory;
            }
            else
            {
                return this.Inventory;
            }
        }

        public int GetDiscountInPercentage(int quantity)
        {
            if(BulkBuyNumber > 1 && quantity >= BulkBuyNumber)
            {
                return 10;
            }
            return 0;
        }

        public Task<int> GetInventoryAsync()
        {
            throw new NotImplementedException();
        }

        public bool NeedReplenishment(string name)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Id + "  " + Name + "  " + Inventory + "  " + Price;
        }

        public bool TryParse(string value, out IProduct product)
        {
            throw new NotImplementedException();
        }
    }
}
