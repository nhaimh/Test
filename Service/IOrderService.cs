using Test.DTO;
using Test.Models;

namespace Test.Service
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(OrderDto dto);
        Task<List<Order>> GetOrdersByCustomerAsync(int customerID);
        Task UpdateOrderStatusAsync(int orderID, OrderStatus status);
    }
}
