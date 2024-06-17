namespace BookStore.Model
{
    public class UpdateUserMapper
    {
        public UpdateUserMapper() { }

        public UpdateUserMapper(string phone, string address)
        {
            Phone = phone;
            Address = address;
        }

        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
