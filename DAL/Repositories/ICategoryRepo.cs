using System.Collections.Generic;
using DAL.Entities;

namespace DAL.Repositories
{
    public interface ICategoryRepo
    {
        void Add(Category category);
        void Update(Category category);
        void Delete(string id);
        Category GetById(string id);
        List<Category> GetAll();
    }
}
