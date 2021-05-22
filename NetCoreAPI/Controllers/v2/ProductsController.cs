using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreAPI.Models;
using RockLib.Logging;

//Build model class and DbContext class first and then use following steps to generate the Controller code:
//Right click Controller folder: Add>New Scaffolded Item> API Controller with actions, using Entity Framework

namespace NetCoreAPI.Controllers.v2
{
    [ApiVersion("2.0")]  //router based versioning
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController] //enable some API-specific behaviors.
                    //Can be applied to namespace for all Controllers.
                    //This makes attribute routing a requirement.
                    //makes model validation errors automatically trigger an HTTP 400 response
    public class ProductsController : ControllerBase
    {
        private readonly ProductDBContext DbContext;
        private readonly ILogger _logger;
        public ProductsController(ProductDBContext context, ILogger logger)
        {
            DbContext = context;
            _logger = logger;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IEnumerable<Product>> GetProducts()
        {
            _logger.Info("test logger");
            return await DbContext.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await DbContext.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            //short for  return new ObjectResult(product);
            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            DbContext.Entry(product).State = EntityState.Modified;

            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                DbContext.Products.Add(product);
                await DbContext.SaveChangesAsync();
            }
            catch(Exception e)
            {
                return BadRequest(e.InnerException.Message);
            }
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await DbContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            DbContext.Products.Remove(product);
            await DbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("about")]
        public ActionResult About()
        {
            return Ok("About the Product API service.");
        }

        [HttpGet("version")]
        public ActionResult Version()
        {
            return Ok("API version 2.0");
        }
        private bool ProductExists(int id)
        {
            return DbContext.Products.Any(e => e.Id == id);
        }
    }
}
