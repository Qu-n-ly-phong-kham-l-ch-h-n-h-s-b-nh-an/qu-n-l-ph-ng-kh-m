using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrugStocksController : ControllerBase
    {
        private readonly DrugStockService _drugStockService;

        public DrugStocksController(DrugStockService drugStockService)
        {
            _drugStockService = drugStockService;
        }

        // GET: api/drugstocks
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_drugStockService.GetAll());
        }

        // GET: api/drugstocks/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var stock = _drugStockService.GetById(id);
            if (stock == null) return NotFound();
            return Ok(stock);
        }

        // POST: api/drugstocks
        [HttpPost]
        public IActionResult Create(DrugStock stock)
        {
            _drugStockService.Create(stock);
            return Ok(stock);
        }

        // PUT: api/drugstocks/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, DrugStock stock)
        {
            if (id != stock.StockId) return BadRequest();
            _drugStockService.Update(stock);
            return Ok(stock);
        }

        // DELETE: api/drugstocks/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _drugStockService.Delete(id);
            return NoContent();
        }
    }
}
