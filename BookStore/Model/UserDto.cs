using CloudinaryDotNet.Actions;

namespace BookStore.Model
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool UserStatus { get; set; }
        public RoleDto Role { get; set; }
    }
}
