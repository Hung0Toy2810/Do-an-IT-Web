using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class UpdateAvatarRequest
    {
        [Required(ErrorMessage = "File is required.")]
        public IFormFile File { get; set; } = null!;
    }
}