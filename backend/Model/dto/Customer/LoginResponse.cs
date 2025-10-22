using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class LoginResponse
    {
        [Required(ErrorMessage = "Vui lòng cung cấp token.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng cung cấp thời gian hết hạn.")]
        public DateTime Expiration { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
