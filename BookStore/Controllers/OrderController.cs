using AutoMapper;
using BookStore.Enum;
using BookStore.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.UnitOfwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BookStore.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderController(IUnitOfwork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("customer/{customerId}")]
        [Authorize(Roles ="Customer")]
        public IActionResult GetOrdersByUserId(int customerId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var ordersQuery = _unitOfWork.OrderRepo.Get(
                filter: o => o.UserId == customerId,
                orderBy: q => q.OrderBy(o => o.OrderId)
            );

            var totalCount = ordersQuery.Count();

            var orders = ordersQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            foreach (var orderDto in orderDtos)
            {
                var orderDetails = _unitOfWork.OrderDetailRepo.FindAll(
                    od => od.OrderId == orderDto.OrderId,
                    includeProperties: "Book.Images"
                );
                orderDto.OrderDetails = _mapper.Map<List<OrderDetailDto>>(orderDetails);
            }

            var pagedResult = new PagedResult<OrderDto>(orderDtos, totalCount, pageIndex, pageSize);

            return Ok(pagedResult);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetOrders([FromQuery] OrderQueryParameters orderParameter)
        {
            Expression<Func<Order, bool>> filter = o =>
                (string.IsNullOrEmpty(orderParameter.CustomerName) || o.CustomerName.Contains(orderParameter.CustomerName)) &&
                (string.IsNullOrEmpty(orderParameter.CustomerPhone) || o.CustomerPhone.Contains(orderParameter.CustomerPhone)) &&
                (!orderParameter.UserId.HasValue || o.UserId == orderParameter.UserId) &&
                (!orderParameter.MinPrice.HasValue || o.Total >= orderParameter.MinPrice) &&
                (!orderParameter.MaxPrice.HasValue || o.Total <= orderParameter.MaxPrice) &&
                (string.IsNullOrEmpty(orderParameter.OrderStatus) || o.OrderStatus.ToLower() == orderParameter.OrderStatus.ToLower()) &&
                (
                    (orderParameter.StartDate.HasValue && !orderParameter.EndDate.HasValue && o.OrderDate.Date == orderParameter.StartDate.Value.Date) ||
                    (!orderParameter.StartDate.HasValue && orderParameter.EndDate.HasValue && o.OrderDate.Date == orderParameter.EndDate.Value.Date) ||
                    (orderParameter.StartDate.HasValue && orderParameter.EndDate.HasValue && o.OrderDate.Date >= orderParameter.StartDate.Value.Date && o.OrderDate.Date <= orderParameter.EndDate.Value.Date) ||
                    (!orderParameter.StartDate.HasValue && !orderParameter.EndDate.HasValue)
                );

            var ordersQuery = _unitOfWork.OrderRepo.Get(
                filter: filter,
                orderBy: q => q.OrderByDescending(o => o.OrderDate),
                includeProperties: "OrderDetails,OrderDetails.Book,OrderDetails.Book.Images"
            );

            var totalCount = ordersQuery.Count();

            var orders = ordersQuery
                .Skip((orderParameter.PageIndex - 1) * orderParameter.PageSize)
                .Take(orderParameter.PageSize)
                .ToList();

            var ordersDto = _mapper.Map<List<OrderDto>>(orders);

            var pagedResult = new PagedResult<OrderDto>(ordersDto, totalCount, orderParameter.PageIndex, orderParameter.PageSize);

            return Ok(pagedResult);
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateOrderStatus(int orderId, [FromQuery] string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                return BadRequest(new { message = "New status must be provided." });
            }

            var validStatuses = new[]
            {
                OrderStatus.Pending.ToString(),
                OrderStatus.Processing.ToString(),
                OrderStatus.Shipped.ToString(),
                OrderStatus.Delivered.ToString(),
                OrderStatus.Cancelled.ToString()
            };

            if (!validStatuses.Contains(newStatus))
            {
                return BadRequest(new { message = "Invalid order status value." });
            }

            var order = _unitOfWork.OrderRepo.GetById(orderId);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {orderId} not found." });
            }

            if (order.OrderStatus == OrderStatus.Cancelled.ToString() || order.OrderStatus == OrderStatus.Delivered.ToString())
            {
                return BadRequest(new { message = "Cannot update the status of cancelled or delivered orders." });
            }

            if (order.OrderStatus == OrderStatus.Processing.ToString() && newStatus == OrderStatus.Pending.ToString())
            {
                return BadRequest(new { message = "Cannot change the status from Processing to Pending." });
            }

            if (order.OrderStatus == OrderStatus.Shipped.ToString() && newStatus == OrderStatus.Pending.ToString())
            {
                return BadRequest(new { message = "Cannot change the status from Shipped to Pending." });
            }

            order.OrderStatus = newStatus;
            _unitOfWork.OrderRepo.Update(order);
            _unitOfWork.Save();

            return Ok(new { message = "Order status updated successfully.", order });
        }



    }
}
