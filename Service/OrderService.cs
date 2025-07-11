using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.DTO;
using Test.Models;

namespace Test.Service
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(OrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    CustomerID = dto.CustomerID,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    OrderDetails = new List<OrderDetail>()
                };

                foreach (var detail in dto.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductID);
                    if (product == null)
                        throw new Exception($"Product {detail.ProductID} not found");

                    if (product.StockQtty < detail.Quantity)
                        throw new Exception($"Insufficient stock for product {detail.ProductID}");

                    product.StockQtty -= detail.Quantity;

                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductID = detail.ProductID,
                        Quantity = detail.Quantity,
                        UnitPrice = product.Price
                    });

                    order.TotalAmount += product.Price * detail.Quantity;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersByCustomerAsync(int customerID)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.CustomerID == customerID)
                .ToListAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderID, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderID);
            if (order == null)
                throw new Exception("Order not found");

            order.Status = status;
            await _context.SaveChangesAsync();
        }
    }

}
