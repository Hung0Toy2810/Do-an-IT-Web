// Controllers/BestSellerController.cs
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/bestseller")]
public class BestSellerController : ControllerBase
{
    private readonly BestSellerService _s;
    public BestSellerController(BestSellerService s) => _s = s;

    [HttpGet("top-30")]
    public Task<List<long>> Get() => _s.GetTop30();
}