using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class UpdateAvatarRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn tệp.")]
        public IFormFile File { get; set; } = null!;
    }
}
