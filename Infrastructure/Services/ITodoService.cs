using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiADO.NetCrud.Entities;
using WebApiADO.NetCrud.Enums;

namespace WebApiADO.NetCrud.Infrastructure.Services
{
    public interface ITodoService
    {
        Task<List<Todo>> FetchMany(TodoShow show = TodoShow.All);
        Task CreateTodo(Todo todo);
        Task<Todo> Update(Todo currentTodo, Todo todoFromUser);
        Task Delete(int todoId);
        Task DeleteAll();
        Task<Todo> GetById(int id);
        Task<Todo> GetProxyById(int id);
    }
}