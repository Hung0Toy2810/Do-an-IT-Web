using Backend.Model.dto.Customer;
using Backend.Model.dto.Administrator;
using Backend.Service.CustomerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        /// <summary>
        /// Creates a new customer with the provided phone number and password.
        /// </summary>
        /// <param name="createCustomer">The customer data including phone number and password.</param>
        /// <returns>A response indicating the result of the creation operation.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomer createCustomer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Thay vì throw Exception
            }

            await _customerService.CreateCustomerAsync(createCustomer);
            return Created($"api/customers/{createCustomer.PhoneNumber}", new
            {
                Message = "Customer created successfully.",
                PhoneNumber = createCustomer.PhoneNumber
            });
        }

        /// <summary>
        /// Authenticates a customer with the provided phone number and password.
        /// </summary>
        /// <param name="loginCustomer">The login data including phone number and password.</param>
        /// <returns>A response containing the JWT token and its expiration time.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginCustomer loginCustomer)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid login data.");
            }

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _customerService.LoginAsync(loginCustomer, clientIp);
            return Ok(response);
        }

        /// <summary>
        /// Updates the customer's avatar.
        /// </summary>
        /// <returns>The public URL of the updated avatar.</returns>
        [HttpPut("avatar")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Invalid file data.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid customerId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }

            var avatarUrl = await _customerService.UpdateAvatarAsync(customerId, file);
            return Ok(new { Message = "Avatar updated successfully.", AvatarUrl = avatarUrl });
        }

        /// <summary>
        /// Updates customer attributes (CustomerName, DeliveryAddress, Email).
        /// </summary>
        /// <param name="request">The customer data to update.</param>
        /// <returns>A response indicating the result of the update operation.</returns>
        [HttpPut]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid customerId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }

            await _customerService.UpdateCustomerAsync(customerId, request);
            return Ok(new { Message = "Customer updated successfully." });
        }

        /// <summary>
        /// Deletes a customer by setting their status to inactive.
        /// </summary>
        /// <returns>A response indicating the result of the deletion operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCustomer()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid customerId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }

            await _customerService.DeleteCustomerAsync(customerId);
            return Ok(new { Message = "Customer account deactivated successfully." });
        }

        /// <summary>
        /// Changes the password of a customer.
        /// </summary>
        /// <param name="request">The old and new password data.</param>
        /// <returns>A response indicating the result of the password change operation.</returns>
        [HttpPut("password")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid customerId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }

            await _customerService.ChangePasswordAsync(customerId, request);
            return Ok(new { Message = "Password changed successfully." });
        }

        /// <summary>
        /// Gets the current customer's information based on the JWT token.
        /// </summary>
        /// <returns>The current customer's information.</returns>
        [HttpGet("me")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentCustomerInfo()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            var customerInfo = await _customerService.GetCustomerInfoByTokenAsync(userIdClaim);
            return Ok(customerInfo);
        }

        /// <summary>
        /// Gets all customers' information (for administrators only).
        /// </summary>
        /// <returns>A list of all customers' information.</returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
    }
}