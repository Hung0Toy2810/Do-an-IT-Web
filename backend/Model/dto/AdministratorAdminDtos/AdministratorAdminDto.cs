// Backend/Model/dto/AdministratorAdminDtos/AdministratorAdminDto.cs
namespace Backend.Model.dto.AdministratorAdminDtos
{
    public class AdministratorAdminDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; } // nếu bạn thêm field này sau
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