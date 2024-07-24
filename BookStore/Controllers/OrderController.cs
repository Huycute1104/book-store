using AutoMapper;
using BookStore.Enum;
using BookStore.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using Repository.Models;
using Repository.UnitOfwork;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace BookStore.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly PayOS _payOS;

        public OrderController(IUnitOfwork unitOfWork, IMapper mapper, PayOS payOS)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _payOS = payOS;
        }

        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Customer")]
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
                orderDto.OrderDetails = _mapper.Map<List<ViewOrderDetail>>(orderDetails);
            }

            var pagedResult = new PagedResult<OrderDto>(orderDtos, totalCount, pageIndex, pageSize);

            return Ok(pagedResult);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest orderDto)
        {
            if (orderDto == null)
            {
                return BadRequest("Order data is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.Claims.FirstOrDefault(x => x.Type == "userId");
            var userIDPase = int.Parse(userId.Value);
            var orderId = new Random().Next(1, int.MaxValue); // Ensure the ID is unique as per your application logic
            var order = new Order
            {
                OrderId = orderId,
                OrderDate = orderDto.OrderDate,
                Total = orderDto.Total,
                UserId = userIDPase,
                OrderStatus = "Pending",
                CustomerName = orderDto.CustomerName,
                CustomerPhone = orderDto.CustomerPhone,

            };
             _unitOfWork.OrderRepo.Add(order);
            var data = new List<ItemData>();

            foreach (var item in orderDto.orderDetailDtos)
            {
                var book = _unitOfWork.BookRepo.GetById(item.BookId);
                

                var itemDetail = new ItemData(book.BookName, item.Quantity, (int)item.UnitPrice);              
                data.Add(itemDetail);

                var orderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    BookId = item.BookId,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    Discount = 0
                };

                _unitOfWork.OrderDetailRepo.Add(orderDetail);
            }

            PaymentData paymentData = new PaymentData(orderId, (int) orderDto.Total,"Thanh toan hoa don", data, "http://localhost:3000/fail", "http://localhost:3000/success");
            Net.payOS.Types.CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

           

            return Ok( createPayment.checkoutUrl);
        }

        //[HttpPost("create")]
        //public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
        //{
        //    try
        //    {
        //        int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        //        ItemData item = new ItemData(body.productName, 1, body.price);
        //        List<ItemData> items = new List<ItemData>();
        //        items.Add(item);
        //        PaymentData paymentData = new PaymentData(orderCode, body.price, body.description, items, body.cancelUrl, body.returnUrl);

        //        CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

        //        return Ok(new Response(0, "success", createPayment));
        //    }
        //    catch (System.Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //        return Ok(new Response(-1, "fail", null));
        //    }
        //}


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
