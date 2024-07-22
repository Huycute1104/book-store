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
        [Authorize(Roles = "Admin")]
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

                int totalCount = _unitOfWork.UserRepo.Count(filter);
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var pagedResponse = new PagedResponse<UserDto>
                {
                    Data = userDtos,
                    TotalPages = totalPages,
                    TotalCount = totalCount
                };

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Customer")]
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
        [Authorize(Roles = "Admin")]
        public IActionResult ToggleUserStatus(int id)
        {
            try
            {
                User user = _unitOfWork.UserRepo.GetById(id);
                if (user == null)
                {
                    return NotFound(new { message = "User Not Found" });
                }

                user.UserStatus = !user.UserStatus;
                _unitOfWork.UserRepo.Update(user);

                return Ok(new { message = "Successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Customer")]
        public IActionResult UpdateUserInfo(int id, UpdateUserMapper userMapper)
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
