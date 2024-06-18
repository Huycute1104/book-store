using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.UnitOfwork;

namespace BookStore.Controllers
{
    [Route("api/carts")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly IMapper _mapper;

        public CartController(IUnitOfwork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItemDto cartItemDto)
        {
            if (cartItemDto == null)
            {
                return BadRequest("Invalid cart item data.");
            }

            var book = _unitOfWork.BookRepo.GetById(cartItemDto.BookId);
            if (book == null)
            {
                return NotFound("Book not found.");
            }
/*
            if (book. < cartItemDto.Quantity)
            {
                return BadRequest("Not enough stock available.");
            }*/

            var user = _unitOfWork.UserRepo.GetById(cartItemDto.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            if(user.RoleId == 1)
            {
                return NotFound("User not Customer.");
            }
            int CartID;
            var cart = _unitOfWork.CartRepo.Get().LastOrDefault();
            if (cart == null)
            {
                CartID = 0;
            }
            else
            {
                CartID =cart.CartId;
            }
            var existingCartItem = _unitOfWork.CartRepo.Find(c => c.BookId == cartItemDto.BookId && c.UsersId == cartItemDto.UserId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
                _unitOfWork.CartRepo.Update(existingCartItem);
            }
            else
            {
                var cartItem = new Cart
                {
                    CartId = CartID +1,
                    BookId = cartItemDto.BookId,
                    UsersId = cartItemDto.UserId,
                    Quantity = 1
                };
                _unitOfWork.CartRepo.Add(cartItem);
            }
            _unitOfWork.Save();

            return Ok("Item added to cart successfully.");
        }
    }
}
