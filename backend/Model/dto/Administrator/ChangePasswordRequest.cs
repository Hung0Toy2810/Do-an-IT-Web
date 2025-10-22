using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ.")]
        [MaxLength(100, ErrorMessage = "Mật khẩu cũ không được vượt quá 100 ký tự.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MaxLength(100, ErrorMessage = "Mật khẩu mới không được vượt quá 100 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
