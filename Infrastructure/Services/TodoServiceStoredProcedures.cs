using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Configuration;
using WebApiADO.NetCrud.Entities;
using WebApiADO.NetCrud.Enums;

namespace WebApiADO.NetCrud.Infrastructure.Services
{
    public class TodoServiceStoredProcedures : ITodoService
    {
        private readonly string _connectionString;


        public TodoServiceStoredProcedures()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["MsSql"].ConnectionString;
        }


        public async Task<List<Todo>> FetchMany(TodoShow show = TodoShow.All)
        {
            List<Todo> todos = new List<Todo>();


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command;

                if (show == TodoShow.All)
                {
                    command = new SqlCommand("GetAllTodos", connection);
                }
                else
                {
                    command = show == TodoShow.Pending
                        ? new SqlCommand("GetPending", connection)
                        : new SqlCommand("GetCompleted", connection);
                }

                command.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    while (dataReader.Read())
                    {
                        Todo todo = new Todo();
                        todo.Id = Convert.ToInt32(dataReader["Id"]);
                        todo.Title = Convert.ToString(dataReader["Title"]);
                        todo.Completed = Convert.ToBoolean(dataReader["Completed"]);
                        todo.CreatedAt = Convert.ToDateTime(dataReader["CreatedAt"]);
                        todo.UpdatedAt = Convert.ToDateTime(dataReader["UpdatedAt"]);

                        todos.Add(todo);
                    }
                }

                connection.Close();
            }

            return todos;
        }

        public async Task<Todo> GetById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand("GetTodoById", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);
                
                using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    if (dataReader.Read())
                    {
                        Todo todo = new Todo();
                        todo.Id = Convert.ToInt32(dataReader["Id"]);
                        todo.Title = Convert.ToString(dataReader["Title"]);
                        todo.Description = Convert.ToString(dataReader["Description"]);
                        todo.Completed = Convert.ToBoolean(dataReader["Completed"]);
                        todo.CreatedAt = Convert.ToDateTime(dataReader["CreatedAt"]);
                        todo.UpdatedAt = Convert.ToDateTime(dataReader["UpdatedAt"]);

                        return todo;
                    }
                }

                connection.Close();
            }

            return null;
        }

        public async Task<Todo> GetProxyById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand("GetTodoProxyById", connection);

                command.CommandType = CommandType.StoredProcedure;
                SqlParameter parameter = command.Parameters.Add("@Id", SqlDbType.Int);
                parameter.Value = id;

                using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    if (dataReader.Read())
                    {
                        Todo todo = new Todo();
                        todo.Id = Convert.ToInt32(dataReader["Id"]);
                        return todo;
                    }
                }

                connection.Close();
            }

            return null;
        }

        public async Task CreateTodo(Todo todo)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("CreateTodo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Title", todo.Title);
                    command.Parameters.AddWithValue("@Description", todo.Description);
                    command.Parameters.AddWithValue("@Completed", todo.Completed);

                    await connection.OpenAsync();
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        todo.Id = int.Parse(result.ToString());
                    }

                    connection.Close();
                }
            }
        }

        public async Task<Todo> Update(Todo currentTodo, Todo todoFromUser)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("UpdateTodo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var now = DateTime.UtcNow;
                    command.Parameters.AddWithValue("@Id", currentTodo.Id);
                    command.Parameters.AddWithValue("@Title", todoFromUser.Title);
                    command.Parameters.AddWithValue("@Description", todoFromUser.Description);
                    command.Parameters.AddWithValue("@Completed", todoFromUser.Completed);
                    command.Parameters.AddWithValue("@UpdatedAt", now);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    todoFromUser.Id = currentTodo.Id;
                    todoFromUser.UpdatedAt = now;
                }
            }

            return todoFromUser;
        }


        /// <summary>  
        /// Deletes a To do
        /// </summary>  
        /// <param name="todoId"></param>  
        /// <returns></returns> 
        public async Task Delete(int todoId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("DeleteTodo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@Id", todoId);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAll()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("DeleteAllTodos", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    int affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}