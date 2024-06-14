using BookStore.Model;
using BookStore.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.UnitOfwork;


namespace BookStore.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(IUnitOfwork unitOfWork, IJwtTokenService jwtTokenService)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _unitOfWork.UserRepo.Get()
                               .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var userInfo = new
            {
                user.UserId,
                user.Email,
                user.Address,
                user.Phone,
                user.UserStatus,
            };
            if(userInfo.UserStatus == false)
            {
                return Unauthorized(new { message = "User is Ban" });
            }
            else
            {
                var token = _jwtTokenService.GenerateToken(user);
                return Ok(new { Token = token, User = userInfo });
            }

            
        }
    }
}
