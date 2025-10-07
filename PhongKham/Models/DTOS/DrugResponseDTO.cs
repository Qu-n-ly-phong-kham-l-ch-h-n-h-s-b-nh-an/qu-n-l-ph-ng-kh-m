namespace PhongKham.API.Models.DTOs
{
    public class DrugResponseDTO
    {
        public int DrugId { get; set; }
        public string DrugName { get; set; } = null!;
        public string? Unit { get; set; }
        public decimal? Price { get; set; }
    }
}
