using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Administrator
{
    public class CreateAdministrator
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không được vượt quá 100 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, gồm cả chữ và số.")]
        public string Password { get; set; } = string.Empty;
    }
}
