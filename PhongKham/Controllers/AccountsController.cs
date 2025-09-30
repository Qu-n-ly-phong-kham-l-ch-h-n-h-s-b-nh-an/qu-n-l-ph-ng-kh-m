using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountsController(AccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: api/accounts
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_accountService.GetAll());
        }

        // GET: api/accounts/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var acc = _accountService.GetById(id);
            if (acc == null) return NotFound();
            return Ok(acc);
        }

        // POST: api/accounts
        [HttpPost]
        public IActionResult Create(Account account)
        {
            _accountService.Create(account);
            return Ok(account);
        }

        // PUT: api/accounts/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, Account account)
        {
            if (id != account.AccountId) return BadRequest();
            _accountService.Update(account);
            return Ok(account);
        }

        // DELETE: api/accounts/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _accountService.Delete(id);
            return NoContent();
        }
    }
}
