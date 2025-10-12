using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Errors;
using Talabat.Repository.Data;

namespace Talabat.APIs.Controllers
{
    public class BuggyController : ApiBaseController
    {
        private readonly StoreContext _dbcontext;

        public BuggyController(StoreContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        [HttpGet("notfound")]  //Get : api/buggy/notfound
        public ActionResult GetNotFoundRequest()
        {
            var product = _dbcontext.products.Find(100);
            if (product is null) 
                return NotFound(new ApiResponse(404));
            return Ok(product);
        }
        [HttpGet("servererror")]  //Get : api/buggy/servererror
        public ActionResult GetServerError()
        {
            var product = _dbcontext.products.Find(100);
            var productToReturn=product.ToString();//will throw [NullReferenceException]
            return Ok(productToReturn);
        }
        [HttpGet("badrequest")]  //Get : api/buggy/badrequest
        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiResponse(400));
        }
        [HttpGet("badrequest/{id}")]  //Get : api/buggy/badrequest/five
        public ActionResult GetBadRequest(int id) //Validation Error
        {
            return BadRequest();
        }
    }
}
