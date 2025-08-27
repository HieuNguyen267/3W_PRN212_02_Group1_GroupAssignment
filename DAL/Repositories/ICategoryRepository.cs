using System.Collections.Generic;
using DAL.Entities;

namespace DAL.Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        Category? GetById(string id);
        void Add(Category category);
        void Update(Category category);
        void Delete(string id);
    }
}
