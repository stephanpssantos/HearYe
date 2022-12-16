using HearYe.Shared;
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync
using Microsoft.AspNetCore.Mvc;

namespace HearYe.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HearYeContext db;

        public UsersController(HearYeContext db) 
        { 
            this.db = db;
        }

        [HttpGet]
        public async Task<List<User>> GetUsersAsync()
        {
            return await db.Users!.ToListAsync();
        }
    }
}
