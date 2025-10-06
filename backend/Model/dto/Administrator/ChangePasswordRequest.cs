using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required.")]
        [MaxLength(100, ErrorMessage = "Old password cannot exceed 100 characters.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MaxLength(100, ErrorMessage = "New password cannot exceed 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}