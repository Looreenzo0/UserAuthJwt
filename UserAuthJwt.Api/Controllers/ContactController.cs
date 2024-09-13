using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserAuthJwt.Infrastructure.Services;

namespace UserAuthJwt.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactDapperService _contactDapperService;
        public ContactController(IContactDapperService contactDapperService)
        {
            _contactDapperService = contactDapperService;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            try
            {
                var contacts = await _contactDapperService.GetAllContacts();
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
