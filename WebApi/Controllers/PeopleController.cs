using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public PeopleController(ApplicationContext context)
        {
            _context = context;
        }


        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminsEndpoints()
        {
            var currentUser = GetCurrentUser();
            return Ok($"Hello {currentUser.GivenName}, You are an {currentUser.Role}");
        }

        [HttpGet("get")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.Persons.ToListAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> Add(Person request)
        {

            var person = new Person
            {
                Id = request.Id,
                Address = request.Address,
                Age = request.Age,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName
            };

            _context.Add(person);
            await _context.SaveChangesAsync();

            return Ok("Person Added Successfully!");
        }

        [Authorize(Roles = "User")]
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if(person != null)
            {
                try
                {
                    return Ok(person);

                }catch(Exception ex)
                {
                    return NotFound(ex.Message);
                }
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if(person != null)
            {
                try
                {
                    _context.Persons.Remove(person);
                    await _context.SaveChangesAsync();

                    return Ok("Deleted Successfully!");

                }catch(Exception ex)
                {
                    return NotFound(ex.Message);
                }
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, Models.Person request)
        {
            var person = await _context.Persons.FindAsync(id);
            if(person != null)
            {
                try
                {
                    person.Address = request.Address;
                    person.Age = request.Age;
                    person.Email = request.Email;
                    person.FirstName = request.FirstName;
                    person.LastName = request.LastName;
                    person.MiddleName = request.MiddleName;
                     

                    _context.Persons.Update(person);
                    await _context.SaveChangesAsync();

                    return Ok(person);
                    
                }catch(Exception ex)
                {
                    return NotFound(ex.Message);
                }
            }

            return NotFound();
        }


        private UserModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if(identity != null)
            {
                var userClaims = identity.Claims;

                return new UserModel
                {
                    UserName = userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                    Email = userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value,
                    GivenName = userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value,
                    SurName = userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.Surname)?.Value,
                    Role = userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value
                };
            }

            return null;
        }
    }
}
