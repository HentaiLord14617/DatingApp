
using API.Controllers;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;
        public BuggyController(DataContext context)
        {
            _context = context;

        }

        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return Unauthorized("Unauthorized");
        }
        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var thing = _context.Users.Find(-1);
            if (thing == null)
            {
                return NotFound("Not Found");
            }
            return Ok(thing);
        }
        [HttpGet("server-error")]
        public ActionResult<string> GetSeverError()
        {
            // try
            // {

            // }
            // catch (Exception ex)
            // {
            //     return StatusCode(500, "Computer says no!");

            // }
            var thing = _context.Users.Find(-1);
            var thingToReturn = thing.ToString();
            return thingToReturn;
        }
        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("Bad Request");
        }



    }
}