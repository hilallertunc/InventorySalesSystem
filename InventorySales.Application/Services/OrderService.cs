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

        public async Task<Result<int>> CreateAsync(string userId, OrderCreateRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                return Result<int>.Failure("Order items cannot be left blank.");

            await using var tx = await _orderRepository.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Created
                };

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                        return Result<int>.Failure("The quantity cannot be 0 or negative.");

                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product is null)
                        return Result<int>.Failure($"Product not found. ProductId={item.ProductId}");

                    if (product.Stock < item.Quantity)
                        return Result<int>.Failure($"Stock is insufficient. Product={product.Name}, Stock={product.Stock}, Desired={item.Quantity}");

                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }

                await _orderRepository.AddAsync(order);
                await tx.CommitAsync();

                return Result<int>.Success(order.Id, "Order created successfully.");
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; // for middleware
            }
        }

        public async Task<Result> CancelAsync(int orderId)
        {
            await using var tx = await _orderRepository.BeginTransactionAsync();

            try
            {
                var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
                if (order is null)
                    return Result.Failure("Order not found.");

                if (order.Status == OrderStatus.Cancelled)
                    return Result.Failure("The order has already been cancelled.");

                foreach (var item in order.Items)
                {
                    var product = item.Product ?? await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                await _orderRepository.SaveAsync();
                await tx.CommitAsync();

                return Result.Success("Order cancelled successfully.");
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw; 
            }
        }
    }
}