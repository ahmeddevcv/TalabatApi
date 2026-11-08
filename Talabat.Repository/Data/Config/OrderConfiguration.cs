using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Repository.Data.Config
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(O => O.ShippingAddress, s => s.WithOwner()); // 1 to 1
            builder.Property(O => O.Status).HasConversion(
                OStatus => OStatus.ToString(),
                OStatus => (OrderStatus)Enum.Parse(typeof(OrderStatus), OStatus)
                );
            builder.Property(O=>O.Subtotal).HasColumnType("decimal(18,2)");

            ///builder.HasMany(o => o.Items)
            ///    .WithOne()
            ///    .OnDelete(DeleteBehavior.Cascade);
            ////with eng aliaa
            builder.HasOne(o => o.DeliveryMethod)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            //Add-Migration SetOnDeleteCascadeForOrderAndOrderItemRelatioship -context StoreContext to allow null
        }
    }
}
