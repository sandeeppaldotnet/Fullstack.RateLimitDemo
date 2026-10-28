using Fullstack.RateLimitDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fullstack.RateLimitDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ProductsController : ControllerBase
    {
        private static readonly List<Product> Products =
        [
            new Product
        {
            Id = 1,
            Name = "Laptop",
            Category = "Electronics",
            Price = 75000,
            InStock = true
        },
        new Product
        {
            Id = 2,
            Name = "Smartphone",
            Category = "Electronics",
            Price = 35000,
            InStock = true
        },
        new Product
        {
            Id = 3,
            Name = "Headphones",
            Category = "Accessories",
            Price = 2500,
            InStock = true
        },
        new Product
        {
            Id = 4,
            Name = "Keyboard",
            Category = "Accessories",
            Price = 1800,
            InStock = false
        },
        new Product
        {
            Id = 5,
            Name = "Monitor",
            Category = "Electronics",
            Price = 15000,
            InStock = true
        }
        ];


        /// <summary>
        /// Return all products
        /// </summary>
        /// <returns></returns>
        [EnableRateLimiting("products")]
        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAllProducts()
        {
            return Ok(Products);
        }

        /// <summary>
        /// return products by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("{id:int}")]
        public ActionResult<Product> GetProductById(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product is null)
            {
                return NotFound(new
                {
                    message = $"Product with ID {id} was not found."
                });
            }

            return Ok(product);
        }
        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>


        [HttpGet("category/{category}")]
        public ActionResult<IEnumerable<Product>> GetProductsByCategory(
            string category)
        {
            var products = Products
                .Where(p => p.Category.Equals(
                    category,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(products);
        }
    }

}
