using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]  // Provides Model.IsValid on all method calls and also tells the method params to use [FromBody] to prevent null references
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            _config = config;
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // validate user

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _authRepository.UserExists(userForRegisterDto.Username))
                return BadRequest("User already exists, please choose another Username");

            var userToCreate = new User()
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = _authRepository.Register(userToCreate, userForRegisterDto.Password);

            // Temp until there is a createdroute
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // Check that the user exists
            var userFromRepository = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepository == null)
                return Unauthorized();

            // Add user details to the claim
            var claims = new[]
            {
                 new Claim(ClaimTypes.NameIdentifier, userFromRepository.Id.ToString()),
                 new Claim(ClaimTypes.Name, userFromRepository.Username)
             };

            // Get the signature key from app settings
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            // Hash the signature
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Build the Payload
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };    

            //Create a new Jwt Handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Create a new Jwt and populate
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // return the token with an 200 status
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }
    }
}