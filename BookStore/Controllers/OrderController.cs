using AutoMapper;
using BookStore.Model;
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

        public class OrderQueryParameters
        {
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;

            public int? UserId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public string? CustomerPhone { get; set; }
            public string? CustomerName { get; set; }
        }

        [HttpGet]
        public IActionResult GetOrders([FromQuery] OrderQueryParameters orderParameter)
        {
            Expression<Func<Order, bool>> filter = null;

            if (!string.IsNullOrEmpty(orderParameter.CustomerName) ||
                orderParameter.StartDate.HasValue ||
                orderParameter.EndDate.HasValue)
            {
                filter = o =>
                    (string.IsNullOrEmpty(orderParameter.CustomerName) || o.CustomerName.Contains(orderParameter.CustomerName)) &&
                    (!orderParameter.StartDate.HasValue || o.OrderDate >= orderParameter.StartDate.Value) &&
                    (!orderParameter.EndDate.HasValue || o.OrderDate <= orderParameter.EndDate.Value);
            }

            var orders = _unitOfWork.OrderRepo.Get(
                filter: filter,
                orderBy: q => q.OrderByDescending(o => o.OrderDate),
                includeProperties: "OrderDetails,OrderDetails.Book",
                pageIndex: orderParameter.PageIndex,
                pageSize: orderParameter.PageSize
            );

            var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);

            return Ok(ordersDto);
        }
    }

    public class PagedResult<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }

        public PagedResult(List<T> items, int count, int pageIndex, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }
    }
}
