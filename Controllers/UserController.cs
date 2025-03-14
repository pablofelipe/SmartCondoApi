using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Dto;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Models;
using SmartCondoApi.Services;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController(SmartCondoContext _context, UserService _userService) : ControllerBase
    {
        // Adicionar um usuário
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddUser([FromBody] UserCreateDTO userCreateDTO)
        {
            try
            {
                var userResponseDTO = await _userService.AddUserAsync(userCreateDTO);
                return Ok(userResponseDTO);
            }
            catch (InvalidCredentialsException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UsersExceedException ex)
            {
                return Unauthorized(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro interno. Mensagem: {ex.Message}");
            }
        }

        // Atualizar um usuário
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UserUpdateDTO updatedUser)
        {
            try
            {
                var userResponseDTO = await _userService.UpdateUserAsync(id, updatedUser);
                return Ok(userResponseDTO);
            }
            catch (InvalidCredentialsException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }

        //Obter todos os usuários
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<User>> Get()
        {
            return await _context.Users.ToListAsync();
        }

        // Obter um usuário por ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        //Deletar um usuário por ID
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
                return BadRequest();
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();

        }
    }
}
