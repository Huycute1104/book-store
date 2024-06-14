using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.UnitOfwork;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BookStore.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoriesController(IUnitOfwork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetCategories(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchCategoryName = " ")
        {
            try
            {
                Expression<Func<Category, bool>> filter = null;

                if (!string.IsNullOrEmpty(searchCategoryName))
                {
                    filter = p => p.CategoryName.Contains(searchCategoryName);
                }

                var categories = _unitOfWork.CategoryRepo.Get(
                    filter: filter,
                    pageIndex: pageIndex,
                    pageSize: pageSize
                );

                var categoryDtos = _mapper.Map<IEnumerable<CategoryMapper>>(categories);

                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetCategoryById(int id)
        {
            try
            {
                Category category = _unitOfWork.CategoryRepo.GetById(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category Not Found" });
                }

                var categoryDto = _mapper.Map<CategoryMapper>(category);

                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult CreateCategory([FromBody] CreateCategoryMapper categoryDto)
        {
            try
            {
                var Cate = _unitOfWork.CategoryRepo.Get().LastOrDefault();
                if (Cate == null)
                {
                    return BadRequest("Category Not Found.");
                }
                var create = _mapper.Map<Category>(categoryDto);
                create.CategoryId = Cate.CategoryId + 1;

                _unitOfWork.CategoryRepo.Add(create);


                var productDto = _mapper.Map<CategoryMapper>(create);

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateResult(int id, CreateCategoryMapper model)
        {
            try
            {
                Category category = _unitOfWork.CategoryRepo.GetById(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category Not Found" });
                }
                category.CategoryName = model.Name;

                _unitOfWork.CategoryRepo.Update(category);

                CategoryMapper updatedProduct = _mapper.Map<CategoryMapper>(category);
                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategoryById(int id)
        {
            try
            {
                Category category = _unitOfWork.CategoryRepo.GetById(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category Not Found" });
                }

                _unitOfWork.CategoryRepo.Delete(category);
                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
    

