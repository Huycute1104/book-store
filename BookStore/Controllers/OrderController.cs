using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.UnitOfwork;

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

        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var orders = _unitOfWork.OrderRepo.Get();
            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            foreach (var orderDto in orderDtos)
            {
                var orderDetails = _unitOfWork.OrderDetailRepo.FindAll(
                    od => od.OrderId == orderDto.OrderId,
                    includeProperties: "Book.Images"
                );
                orderDto.OrderDetails = _mapper.Map<List<OrderDetailDto>>(orderDetails);
            }

            return Ok(orderDtos);
        }
    }
}
