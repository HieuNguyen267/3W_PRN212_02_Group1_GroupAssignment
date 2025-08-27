using System.Collections.Generic;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepo _repo;

        public CategoryService(ICategoryRepo repo)
        {
            _repo = repo;
        }

        public string GenerateNewCategoryId()
        {
            return (_repo as CategoryRepo)?.GenerateNewCategoryId() ?? "CAT000";
        }

        public List<Category> GetAllCategories()
        {
            return _repo.GetAll();
        }

        public Category? GetById(string id)
        {
            return _repo.GetById(id);
        }

        public void AddCategory(Category category)
        {
            _repo.Add(category);
        }

        public void UpdateCategory(Category category)
        {
            _repo.Update(category);
        }

        public void DeleteCategory(string id)
        {
            _repo.Delete(id);
        }
    }
}
