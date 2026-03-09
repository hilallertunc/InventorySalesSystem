using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Order;
using InventorySales.Domain.Entities.Orders;
using InventorySales.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Result<OrderResponse>> CreateAsync(string userId, OrderCreateRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                return Result<OrderResponse>.Failure("Order items cannot be left blank.");

            await using var tx = await _orderRepository.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Created
                };

                var responseItems = new List<OrderItemResponse>();

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                        return Result<OrderResponse>.Failure("The quantity cannot be 0 or negative.");

                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product is null)
                        return Result<OrderResponse>.Failure($"Product not found. ProductId={item.ProductId}");

                    if (product.Stock < item.Quantity)
                        return Result<OrderResponse>.Failure($"Stock is insufficient. Product={product.Name}, Stock={product.Stock}, Desired={item.Quantity}");

                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });

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

                var orderResponse = new OrderResponse
                {
                    Id = order.Id,
                    Status = order.Status.ToString(),
                    CreatedAtUtc = order.CreatedAtUtc,
                    Total = total,
                    Items = responseItems
                };

                return Result<OrderResponse>.Success(orderResponse, "Order created successfully");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Result<OrderResponse>.Failure($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result> CancelAsync(int orderId)
        {
            await using var tx = await _orderRepository.BeginTransactionAsync();

            try
            {
                var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
                if (order is null)
                    return Result.Failure("Sipariş bulunamadı.");

                if (order.Status == OrderStatus.Cancelled)
                    return Result.Failure("Sipariş zaten iptal.");

                foreach (var item in order.Items)
                {
                    var product = item.Product ?? await _productRepository.GetByIdAsync(item.ProductId);
                    if (product is null)
                        return Result.Failure($"Product bulunamadı. ProductId={item.ProductId}");

                    product.Stock += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }

                order.Status = OrderStatus.Cancelled;
                await _orderRepository.SaveAsync();

                await tx.CommitAsync();
                return Result.Success("Order cancelled");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Result.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}