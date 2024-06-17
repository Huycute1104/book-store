using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.UnitOfwork;
using System.Linq.Expressions;

namespace BookStore.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(IUnitOfwork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetUsers(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchEmail = null)
        {
            try
            {
                Expression<Func<User, bool>> filter = p => true; // Include all users by default

                if (!string.IsNullOrEmpty(searchEmail) && !string.IsNullOrWhiteSpace(searchEmail))
                {
                    filter = p => p.Email.Contains(searchEmail);
                }

                var users = _unitOfWork.UserRepo.Get(
                    filter: filter,
                    includeProperties: "Role",
                    pageIndex: pageIndex,
                    pageSize: pageSize
                );

                var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("toggle/{id}")]
        public IActionResult toggleUserStatus(int id)
        {
            try
            {
                User user = _unitOfWork.UserRepo.GetById(id);
                if (user == null)
                {
                    return NotFound(new { message = "User Not Found" });
                }
                if (user.UserStatus == true)
                {
                    user.UserStatus = false;
                }
                else
                {
                    user.UserStatus = true;
                }
                

                _unitOfWork.UserRepo.Update(user);

                return Ok(new { message = "Successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
