namespace MetaPlApi.Models.DTOs.Responses
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        public string? Role { get; set; }
        public int RoleId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
