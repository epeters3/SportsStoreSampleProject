using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Abstract;

namespace SportsStore.Domain.Concrete
{
    class OrderProcessor
    {
        void ProcessOrder(Cart cart, ShippingDetails shippingDetails)
        {
            EFProductRepository productRepo = new EFProductRepository();
            foreach (var lineItem in cart.Lines)
            {
                // Adjust the inventory quantity of this product,
                // then call SaveProduct(product) in IProductRepository
                if (lineItem.Quantity >= lineItem.Product.InventoryQty)
                {
                    // We have enough inventory for the order. Update the database inventory.
                    lineItem.Product.InventoryQty -= lineItem.Quantity;
                    productRepo.SaveProduct(lineItem.Product);
                } else
                {
                    // We don't have enough inventory for the order.
                    throw new System.InvalidOperationException("There is not enough inventory to cover this item");
                }
                
            }
        }
    }
}
