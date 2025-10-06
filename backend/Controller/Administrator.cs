using Backend.Model.dto.Administrator;
using Backend.Model.dto.Customer;
using Backend.Service.AdministratorService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/administrators")]
    public class AdministratorController : ControllerBase
    {
        private readonly IAdministratorService _administratorService;

        public AdministratorController(IAdministratorService administratorService)
        {
            _administratorService = administratorService ?? throw new ArgumentNullException(nameof(administratorService));
        }

        /// <summary>
        /// Creates a new administrator with the provided username and password.
        /// </summary>
        /// <param name="createAdministrator">The administrator data including username and password.</param>
        /// <returns>A response indicating the result of the creation operation.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAdministrator([FromBody] CreateAdministrator createAdministrator)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            await _administratorService.CreateAdministratorAsync(createAdministrator);
            return Created($"api/administrators/{createAdministrator.Username}", new
            {
                Message = "Administrator created successfully.",
                Username = createAdministrator.Username
            });
        }

        /// <summary>
        /// Authenticates an administrator with the provided username and password.
        /// </summary>
        /// <param name="loginAdministrator">The login data including username and password.</param>
        /// <returns>A response containing the JWT token and its expiration time.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginAdministrator loginAdministrator)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid login data.");
            }

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _administratorService.LoginAsync(loginAdministrator, clientIp);
            return Ok(response);
        }

        /// <summary>
        /// Deletes an administrator by setting their status to inactive.
        /// </summary>
        /// <param name="username">The username of the administrator.</param>
        /// <returns>A response indicating the result of the deletion operation.</returns>
        [HttpDelete("{username}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAdministrator(string username)
        {
            await _administratorService.DeleteAdministratorByUsernameAsync(username);
            return Ok(new { Message = "Administrator account deactivated successfully." });
        }

        /// <summary>
        /// Deletes the current administrator account by setting status to inactive.
        /// </summary>
        /// <returns>A response indicating the result of the deletion operation.</returns>
        [HttpDelete("me")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCurrentAdministrator()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid administratorId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }

            await _administratorService.DeleteAdministratorAsync(administratorId);
            return Ok(new { Message = "Your administrator account deactivated successfully." });
        }

        /// <summary>
        /// Changes the password of an administrator.
        /// </summary>
        /// <param name="username">The username of the administrator.</param>
        /// <param name="request">The old and new password data.</param>
        /// <returns>A response indicating the result of the password change operation.</returns>
        [HttpPut("{username}/password")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword(string username, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            await _administratorService.ChangePasswordByUsernameAsync(username, request);
            return Ok(new { Message = "Password changed successfully." });
        }

        /// <summary>
        /// Changes the password of the current administrator.
        /// </summary>
        /// <param name="request">The old and new password data.</param>
        /// <returns>A response indicating the result of the password change operation.</returns>
        [HttpPut("password")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeCurrentPassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid administratorId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }

            await _administratorService.ChangePasswordAsync(administratorId, request);
            return Ok(new { Message = "Your password changed successfully." });
        }

        /// <summary>
        /// Gets the administrator's information by username.
        /// </summary>
        /// <param name="username">The username of the administrator.</param>
        /// <returns>The administrator's information.</returns>
        [HttpGet("{username}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdministratorInfo(string username)
        {
            var administratorInfo = await _administratorService.GetAdministratorInfoByUsernameAsync(username);
            return Ok(administratorInfo);
        }

        /// <summary>
        /// Gets the current administrator's information based on the JWT token.
        /// </summary>
        /// <returns>The current administrator's information.</returns>
        [HttpGet("me")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentAdministratorInfo()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { Message = "Invalid or missing user identifier in token." });
            }
            var administratorInfo = await _administratorService.GetAdministratorInfoByTokenAsync(userIdClaim);
            return Ok(administratorInfo);
        }

        /// <summary>
        /// Gets all administrators' information (for administrators only).
        /// </summary>
        /// <returns>A list of all administrators' information.</returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAdministrators()
        {
            var administrators = await _administratorService.GetAllAdministratorsAsync();
            return Ok(administrators);
        }

        /// <summary>
        /// Gets all customers' information (for administrators only).
        /// </summary>
        /// <returns>A list of all customers' information.</returns>
        [HttpGet("customers")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _administratorService.GetAllCustomersAsync();
            return Ok(customers);
        }
    }
}