using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.UnitOfwork;
using System.Linq.Expressions;

namespace BookStore.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
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
        [Authorize(Policy = "Admin")]
        public IActionResult GetUsers([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                Expression<Func<User, bool>> filter = p => p.Role.RoleName == "CUSTOMER"; 

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


        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                User user = _unitOfWork.UserRepo.GetById(id, includeProperties: "Role");
                if (user == null)
                {
                    return NotFound(new { message = "User Not Found" });
                }

                var userDto = _mapper.Map<UserDto>(user);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("toggle/{id}")]
        [Authorize(Policy = "Admin")]
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
        [HttpPut("{id}")]
        [Authorize(Policy = "Customer")]
        public IActionResult UpdateUserInfo(int id,UpdateUserMapper userMapper)
        {
            try
            {
                User user = _unitOfWork.UserRepo.GetById(id);
                if (user == null)
                {
                    return NotFound(new { message = "User Not Found" });
                }
                
                user.Address = userMapper.Address;
                user.Phone = userMapper.Phone;


                _unitOfWork.UserRepo.Update(user);

                var userDto = _mapper.Map<UserMapper>(user);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
