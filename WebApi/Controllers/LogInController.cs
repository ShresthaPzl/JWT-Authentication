using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _iConfiguration;
        public LogInController(ApplicationContext context,
            IConfiguration iConfiguration)
        {
            _context = context;
            _iConfiguration = iConfiguration;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult LogIn([FromBody] UserLogIn model)
        {
            var user = Authenticate(model);

            if(user != null)
            {
                var token = GenerateToken(user);
                return Ok(token);
            }

            return NotFound($"{model.UserName} Not Found!");
        }


        private UserModel Authenticate(UserLogIn model)
        {
            var currentUser = _context.Users.FirstOrDefault(user => user.UserName.ToUpper() == model.UserName.ToUpper()
            && user.Password == model.Password);

            if(currentUser != null)
            {
                return currentUser;
            }
            return null;
        }
        private string GenerateToken(UserModel model)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iConfiguration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, model.UserName),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.GivenName, model.GivenName),
                new Claim(ClaimTypes.Surname, model.SurName),
                new Claim(ClaimTypes.Role, model.Role)
            };

            var token = new JwtSecurityToken(_iConfiguration["JWT:Issuer"], _iConfiguration["JWT:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
