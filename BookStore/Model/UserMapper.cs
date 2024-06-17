namespace BookStore.Model
{
    public class UserMapper
    {
        public UserMapper() { }

        public UserMapper(int id, string email, string phone, string address, bool userStatus, int roleId)
        {
            Id = id;
            Email = email;
            Phone = phone;
            Address = address;
            UserStatus = userStatus;
            RoleId = roleId;
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool UserStatus { get; set; }
        public int RoleId { get; set; }

    }
}
