namespace ConfirmMe.Dto
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int PositionId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int PositionId { get; set; }
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public int PositionId { get; set; }
        public bool IsActive { get; set; }
    }
}
