using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class CreateCustomer
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [MaxLength(100, ErrorMessage = "Số điện thoại không được vượt quá 100 ký tự.")]
        [RegularExpression(@"^\+?\d{8,15}$", ErrorMessage = "Số điện thoại không hợp lệ (8–15 chữ số).")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, gồm cả chữ và số.")]
        public string Password { get; set; } = string.Empty;
    }
}
