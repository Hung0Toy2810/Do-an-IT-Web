// Backend/Model/dto/CustomerAdminDtos/CustomerAdminDto.cs
namespace Backend.Model.dto.CustomerAdminDtos
{
    public class CustomerAdminDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvtURL { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int TotalInvoices { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}