using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using NetCoreAPI.Models;
using System.Linq;
using RockLib.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace NetCoreAPI.Controllers.v1
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
        public string Post([FromBody] object value)
        {
            Product p = JsonConvert.DeserializeObject<Product>(value.ToString());

            if (p == null)
            {
                return "you posted invalid data: " + value.ToString();
            }

            try
            {
                DbContext.Add(p);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return "add product error: " + ex.InnerException.Message;
            }

            return "add product success: " + p.ToString();
        }

        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] object value)
        {
            Console.WriteLine(value.ToString());
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public string Delete(int id)
        {
            try
            {
                Product prod = DbContext.Products.Where(pd => pd.Id == id).FirstOrDefault();
                if (prod == null)
                {
                    return "The product doesn't exist";
                }

                DbContext.Remove(prod);
                DbContext.SaveChanges();
                return "The product has been deleted";
            }
            catch (Exception e)
            {
                return e.Message;
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
    }
}
