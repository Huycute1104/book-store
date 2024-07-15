using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Repository.Models;
using Repository.UnitOfwork;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BookStore.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize(Policy = "Admin")]
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
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var categories = _unitOfWork.CategoryRepo.Get(
                    pageIndex: pageIndex,
                    pageSize: pageSize
                );

                var categoryDtos = _mapper.Map<IEnumerable<CategoryMapper>>(categories);

                int totalCount = _unitOfWork.CategoryRepo.Count();
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var response = new
                {
                    TotalPages = totalPages,
                    Categories = categoryDtos
                };

                return Ok(response);
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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingCategory = _unitOfWork.CategoryRepo
                    .Get(c => c.CategoryName.ToLower() == categoryDto.Name.ToLower())
                    .FirstOrDefault();

                if (existingCategory != null)
                {
                    return BadRequest(new { message = "Category already exists" });
                }

                var newCategory = _mapper.Map<Category>(categoryDto);
                var lastCate = _unitOfWork.CategoryRepo.Get().LastOrDefault();
                int cateID;
                if (lastCate == null)
                {
                    cateID =0;
                }
                else
                {
                    cateID = lastCate.CategoryId;
                }
                newCategory.CategoryId = cateID+1;

                _unitOfWork.CategoryRepo.Add(newCategory);
                _unitOfWork.Save();

                var categoryDtoResponse = _mapper.Map<CategoryMapper>(newCategory);

                return CreatedAtAction(nameof(GetCategoryById), new { id = categoryDtoResponse.Id }, categoryDtoResponse);
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

                CategoryMapper updatedCategory = _mapper.Map<CategoryMapper>(category);
                return Ok(new { message = "Category updated successfully", category = updatedCategory });
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
    

