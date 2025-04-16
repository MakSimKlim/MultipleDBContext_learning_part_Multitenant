using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Multitenant.Api.Controllers
{
    //контроллер, который использует DI IProductService на уровне контейнера
    //и выставляет 3 конечные точки
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // 1 . Возможность добавить конечную точку для использования метода
        // GetAllService
        private readonly IProductService _service;
        public ProductsController(IProductService service)
        {
            _service = service;
        }
        // 2. Get By Id
        [HttpGet]
        public async Task<IActionResult> GetAsync(int id)
        {
            var productDetails = await _service.GetByIdAsync(id);
            return Ok(productDetails);
        }
        // 3. Create a new Product
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateProductRequest request)
        {
            return Ok(await _service.CreateAsync(request.Name, request.Description, request.Rate));
        }
    }
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Rate { get; set; }
    }
}
