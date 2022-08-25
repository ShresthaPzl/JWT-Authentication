using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationContext _context;
        public AdminController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser(UserModel model)
        {
            if(model != null)
            {
                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                return Ok($"User {model.UserName} has been added successfully.");
            }

            return NoContent();
        }

    }
}
