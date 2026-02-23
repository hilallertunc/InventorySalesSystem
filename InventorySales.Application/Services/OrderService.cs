using InventorySales.Application.DTOs.Order;
using InventorySales.Domain.Entities.Orders;
using InventorySales.Infrastructure.Repositories;

namespace InventorySales.Application.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;

        public OrderService(OrderRepository orderRepository, ProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<OrderResponse> CreateAsync(string userId, OrderCreateRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new ArgumentException("Order items cannot be left blank.");

            await using var tx = await _orderRepository.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Created,
                    CreatedAtUtc = DateTime.UtcNow
                };

                // product name
                var responseItems = new List<OrderItemResponse>();

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                        throw new ArgumentException("The quantity cannot be 0 or negative.");

                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product is null)
                        throw new ArgumentException($"Product not found. ProductId={item.ProductId}");

                    if (product.Stock < item.Quantity)
                        throw new ArgumentException(
                            $"Stock is insufficient. Product={product.Name}, Stock={product.Stock}, Desired={item.Quantity}"
                        );

                    // stock drop
                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    // order line (db)
                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });

                    // response line(user)
                    responseItems.Add(new OrderItemResponse
                    {
                        ProductId = product.Id,
                        ProductName = product.Name, 
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }

                await _orderRepository.AddAsync(order);
                await tx.CommitAsync();

                var total = responseItems.Sum(i => i.UnitPrice * i.Quantity);

                return new OrderResponse
                {
                    Id = order.Id,
                    Status = order.Status.ToString(),
                    CreatedAtUtc = order.CreatedAtUtc,
                    Total = total,
                    Items = responseItems
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task CancelAsync(int orderId)
        {
            await using var tx = await _orderRepository.BeginTransactionAsync();

            try
            {
                var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
                if (order is null)
                    throw new ArgumentException("Sipariş bulunamadı.");

                if (order.Status == OrderStatus.Cancelled)
                    throw new ArgumentException("Sipariş zaten iptal.");

                // add stock
                foreach (var item in order.Items)
                {
                    
                    var product = item.Product ?? await _productRepository.GetByIdAsync(item.ProductId);
                    if (product is null)
                        throw new ArgumentException($"Product bulunamadı. ProductId={item.ProductId}");

                    product.Stock += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }

                order.Status = OrderStatus.Cancelled;
                await _orderRepository.SaveAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}