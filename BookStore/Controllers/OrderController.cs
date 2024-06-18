using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.UnitOfwork;
using System.Linq;

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

        public class OrderQueryParameters
        {
            public int pageIndex { get; set; } = 1;
            public int pageSize { get; set; } = 10;

            public int? UserId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public string? CustomerPhone { get; set; }
            public string? CustomerName { get; set; }
        }

        [HttpGet]
        public IActionResult GetAllOrders([FromQuery] OrderQueryParameters queryParameters)
        {
            var ordersQuery = _unitOfWork.OrderRepo.Get(
                filter: o => (!queryParameters.UserId.HasValue || o.UserId == queryParameters.UserId) &&
                             (!queryParameters.StartDate.HasValue || o.OrderDate >= queryParameters.StartDate) &&
                             (!queryParameters.EndDate.HasValue || o.OrderDate <= queryParameters.EndDate) &&
                             (!queryParameters.MinPrice.HasValue || o.Total >= queryParameters.MinPrice) &&
                             (!queryParameters.MaxPrice.HasValue || o.Total <= queryParameters.MaxPrice) &&
                             (string.IsNullOrEmpty(queryParameters.CustomerPhone) || o.CustomerPhone.Contains(queryParameters.CustomerPhone)) &&
                             (string.IsNullOrEmpty(queryParameters.CustomerName) || o.CustomerName.Contains(queryParameters.CustomerName)),
                orderBy: q => q.OrderBy(o => o.OrderId)
            );

            var totalCount = ordersQuery.Count();

            var orders = ordersQuery
                .Skip((queryParameters.pageIndex - 1) * queryParameters.pageSize)
                .Take(queryParameters.pageSize)
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

            var pagedResult = new PagedResult<OrderDto>(orderDtos, totalCount, queryParameters.pageIndex, queryParameters.pageSize);

            return Ok(pagedResult);
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
