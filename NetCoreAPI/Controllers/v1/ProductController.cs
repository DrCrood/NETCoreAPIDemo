using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using DotNET5API.Models;
using System.Linq;
using RockLib.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DotNET5API.Controllers.v1
{
    [ApiVersion("1.0")]  //router based versioning
    [ApiVersion("5.0")]  //support other versions as well
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _config;  //get settings in appsettings.json
        private readonly ProductDBContext DbContext;
        private readonly AppRunningConfigurations _appConfig;
        public ProductController(ProductDBContext DBContext, IOptions<AppRunningConfigurations> appconfig, IConfiguration config)
        {
            //Get DbContext from dependence injection container
            DbContext = DBContext;
            _appConfig = appconfig.Value;
            _config = config;
            DbContext.Database.EnsureCreated();
        }

        // GET: api/<ProductController>
        [HttpGet]
        [Produces("application/json")]  //speccify return format
        public IEnumerable<Product> Get()
        {
            if(_appConfig.setASetting)
            {
                Console.WriteLine(nameof(_appConfig.setASetting));
            }
            Console.WriteLine(_config["Logging:LogLevel:Default"] + " ->");
                   
            return DbContext.Products.ToList();
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        [Produces("application/json")]
        public ActionResult<Product> Get(int id)
        {
            Product prod = DbContext.Products.Where(pd => pd.Id == id).FirstOrDefault();
            return prod != null ? prod : NotFound();
        }

        // POST api/<ProductController>
        [HttpPost]
        public ActionResult<Product> Post([FromBody] object value)
        {
            Product p = JsonConvert.DeserializeObject<Product>(value.ToString());

            if (p == null)
            {
                return BadRequest(p);
            }

            try
            {
                DbContext.Add(p);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
            }

            return Ok(p);
        }

        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> Put(int id, [FromBody] object value)
        {
            Product p = JsonConvert.DeserializeObject<Product>(value.ToString());

            if (p == null || id != p.Id)
            {
                return BadRequest(p);
            }

            var product = await DbContext.Products.FindAsync(id);

            if (product == null)
            {
                return BadRequest("Id not found");
            }

            product.Name = p.Name;
            product.Price = p.Price;
            product.Inventory = p.Inventory;

            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.Message);
                }
            }

            return Ok(p);
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public ActionResult<Product> Delete(int id)
        {
            try
            {
                Product prod = DbContext.Products.Where(pd => pd.Id == id).FirstOrDefault();
                if (prod == null)
                {
                    return NotFound(prod);
                }

                DbContext.Remove(prod);
                DbContext.SaveChanges();
                return Ok(prod) ;
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.InnerException.Message);
            }
        }

        [HttpGet("about")]
        public ContentResult About()
        {
            return Content("About the Product API service.");
        }

        [HttpGet("version")]
        public string Version()
        {
            return "V1.0";
        }

        private bool ProductExists(int id)
        {
            return DbContext.Products.Any(e => e.Id == id);
        }
    }
}
