using System.Collections.Generic;
using System.Linq;
using DAL.Entities;

namespace DAL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ShoppingOnlineContext _context;

        public CategoryRepository()
        {
            _context = new ShoppingOnlineContext();
        }

        public IEnumerable<Category> GetAll()
        {
            return _context.Categories.ToList();
        }

        public Category? GetById(string id)
        {
            return _context.Categories.FirstOrDefault(c => c.CategoryId == id);
        }

        public void Add(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var entity = _context.Categories.Find(id);
            if (entity != null)
            {
                _context.Categories.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
