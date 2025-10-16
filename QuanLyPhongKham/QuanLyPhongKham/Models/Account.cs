namespace QuanLyPhongKhamApi.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
