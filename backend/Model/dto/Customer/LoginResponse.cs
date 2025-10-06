using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class LoginResponse
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiration time is required.")]
        public DateTime Expiration { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}