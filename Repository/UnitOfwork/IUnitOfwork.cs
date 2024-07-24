using Repository.GenericRepository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.UnitOfwork
{
    public interface IUnitOfwork : IDisposable
    {
        IGenericRepository<Role> RoleRepo { get; set; }
        IGenericRepository<Book> BookRepo { get; set; }
        IGenericRepository<Category> CategoryRepo { get; set; }
        IGenericRepository<User> UserRepo { get; set; }
        IGenericRepository<Order> OrderRepo { get; set; }
        IGenericRepository<OrderDetail> OrderDetailRepo { get; set; }
        IGenericRepository<Cart> CartRepo { get; set; }
        IGenericRepository<Image> ImageRepo { get; set; }
        void Save();
    }
}
