using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.DTO;
using Test.Models;
using Test.Service;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderDto dto)
        {
            if (dto == null || dto.OrderDetails == null || !dto.OrderDetails.Any())
            {
                return BadRequest(new { error = "Invalid order data" });
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetOrdersByCustomer), new { customerID = order.CustomerID }, order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating the order", message = ex.Message });
            }
        }

        [HttpGet("customer/{customerID}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(int customerID)
        {
            if (customerID <= 0)
            {
                return BadRequest(new { error = "Invalid customer ID" });
            }

            try
            {
                var orders = await _orderService.GetOrdersByCustomerAsync(customerID);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching orders", message = ex.Message });
            }
        }

        [HttpPut("{orderID}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderID, [FromBody] OrderStatus status)
        {
            if (!Enum.IsDefined(typeof(OrderStatus), status))
            {
                return BadRequest(new { error = "Invalid order status" });
            }

            try
            {
                await _orderService.UpdateOrderStatusAsync(orderID, status);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating order status", message = ex.Message });
            }
        }
    }
}
