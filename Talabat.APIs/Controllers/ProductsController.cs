using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Repositories;
using Talabat.Core.Specifiactions;

namespace Talabat.APIs.Controllers
{
    public class ProductsController : ApiBaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        ///private readonly IGenericRepository<Product> productRepo;
        ///private readonly IGenericRepository<ProductBrand> productBrandRepo;
        ///private readonly IGenericRepository<ProductType> productTypeRepo;
        private readonly IMapper mapper;

        //inject
        public ProductsController(
            ///IGenericRepository<Product> _productRepo,
            ///IGenericRepository<ProductBrand> _productBrandRepo,
            ///IGenericRepository<ProductType> _productTypeRepo,
            IUnitOfWork unitOfWork,
            IMapper _mapper)//register IMapper
        {
            _unitOfWork = unitOfWork;
            ///productRepo = _productRepo;
            ///productBrandRepo = _productBrandRepo;
            ///productTypeRepo = _productTypeRepo;
            mapper = _mapper;
        }
        [CachedAttribute(600)]
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            //var products = await productRepo.GetAllAsync();
            //return Ok(products);
            var spec=new ProductWithBrandAndTypeSpecifications(specParams);
            var products = await /*productRepo*/_unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
            var data = mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);
            //count
            var countSpec = new ProductWithFilterationForCountSpecification(specParams);
            var count=await _unitOfWork.Repository<Product>().GetCountWithSpecAsync(countSpec);
            var counOfData = await _unitOfWork.Repository<Product>().GetCountWithSpecAsync(spec);
            return Ok(new Pagination<ProductToReturnDto>(specParams.PageIndex,specParams.PageSize, count,counOfData, data));
            //return Ok(products);
        }
        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            //var product = await productRepo.GetByIdAsync(id);
            //return Ok(product);
            var spec = new ProductWithBrandAndTypeSpecifications(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);
            if(product is null) return NotFound(new ApiResponse(404));
            return Ok(mapper.Map<Product,ProductToReturnDto>(product));
        }

        [HttpGet("brands")] //Get: api/product/brands
        public async Task<ActionResult<IEnumerable<ProductBrand>>> GetBrands()
        {
            var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
            return Ok(brands);
        }
        [HttpGet("types")] //Get: api/product/types
        public async Task<ActionResult<IEnumerable<ProductType>>> GetTypes()
        {
            var types = await _unitOfWork.Repository<ProductType>().GetAllAsync();
            return Ok(types);
        }

    }
}
