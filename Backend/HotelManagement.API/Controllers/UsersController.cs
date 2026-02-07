using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.User;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager,Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role = null)
        {
            try
            {
                IEnumerable<User> users;
                
                if (!string.IsNullOrEmpty(role))
                {
                    users = await _userService.GetUsersByRoleAsync(role);
                }
                else
                {
                    users = await _userService.GetAllUsersAsync();
                }

                return Ok(new ApiResponse<IEnumerable<User>>
                {
                    Success = true,
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving users"
                });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(long id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"User with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user"
                });
            }
        }

        /// <summary>
        /// Create new user (Manager can create Receptionist/Housekeeping, Admin can create all)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                // Manager can only create Receptionist and Housekeeping staff
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (currentUserRole == "Manager" && (dto.Role == "Admin" || dto.Role == "Manager"))
                {
                    return Forbid();
                }

                var user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    Role = dto.Role
                };

                var createdUser = await _userService.CreateUserAsync(user, dto.Password);
                
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, new ApiResponse<User>
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = createdUser
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = ex.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating user"
                });
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = new User
                {
                    Email = dto.Email ?? string.Empty,
                    Role = dto.Role ?? "Customer"
                };

                var updatedUser = await _userService.UpdateUserAsync(id, user);
                
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = updatedUser
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = ex.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating user"
                });
            }
        }

        /// <summary>
        /// Deactivate user
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(long id)
        {
            try
            {
                var success = await _userService.DeactivateUserAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "User deactivated successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to deactivate user"
                    });
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deactivating user"
                });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "User deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to delete user"
                    });
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting user"
                });
            }
        }
    }
}
