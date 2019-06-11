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
    public class TodoService : ITodoService
    {
        private readonly string _connectionString;


        public TodoService()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["MsSql"].ConnectionString;
        }

        public async Task<List<Todo>> FetchManyVulnerable(TodoShow show = TodoShow.All)
        {
            List<Todo> todos = new List<Todo>();


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql;
                if (show == TodoShow.All)
                {
                    sql = "Select * From Todo";
                }
                else
                {
                    sql = $"Select * From Todo Where Completed={(int) show}";
                }

                SqlCommand command = new SqlCommand(sql, connection);

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Todo todo = new Todo();
                        todo.Id = Convert.ToInt32(dataReader["Id"]);
                        todo.Title = Convert.ToString(dataReader["Title"]);
                        todo.Description = Convert.ToString(dataReader["Description"]);
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


        public async Task<List<Todo>> FetchMany(TodoShow show = TodoShow.All)
        {
            List<Todo> todos = new List<Todo>();


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql;
                SqlCommand command;

                if (show == TodoShow.All)
                {
                    command = new SqlCommand("Select Id, Title, Completed, CreatedAt, UpdatedAt From Todo", connection);
                }
                else
                {
                    command = new SqlCommand(
                        "Select Id, Title, Completed, CreatedAt, UpdatedAt From Todo Where Completed = @Completed",
                        connection);

                    command.Parameters.Add(new SqlParameter("@Completed", show == TodoShow.Completed ? true : false));
                }


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

        public async Task CreateTodoVulnerable(Todo todo)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = $"Insert Into Todo (Title, Description, Completed) Values " +
                             $"('{todo.Title}', '{todo.Description}','{todo.Completed}'); Select SCOPE_IDENTITY()";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

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

        public async Task CreateTodo(Todo todo)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = $"Insert Into Todo (Title, Description, Completed) Values " +
                             $"(@Title, @Description, @Completed); Select SCOPE_IDENTITY()";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    command.Parameters.AddWithValue("Title", todo.Title);
                    command.Parameters.AddWithValue("Description", todo.Description);
                    command.Parameters.AddWithValue("Completed", todo.Completed);

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

        public async Task UpdateVulnerable(int id, Todo todoFromUser)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql =
                    $"Update Todo SET Title='{todoFromUser.Title}', Description='{todoFromUser.Description}', Completed='{todoFromUser.Completed}' " +
                    $"Where Id='{id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
            }
        }

        public async Task<Todo> UpdateVulnerable(Todo currentTodo, Todo todoFromUser)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql =
                    $"Update Todo SET Title='{todoFromUser.Title}', Description='{todoFromUser.Description}', Completed='{todoFromUser.Completed}' " +
                    $"Where Id='{currentTodo.Id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    todoFromUser.Id = currentTodo.Id;
                }
            }

            return todoFromUser;
        }

        public async Task<Todo> Update(Todo currentTodo, Todo todoFromUser)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql =
                    $"Update Todo SET Title=@Title, Description=@Description, Completed=@Completed, UpdatedAt= @UpdatedAt " +
                    "Where Id= @Id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    var now = DateTime.UtcNow;
                    command.Parameters.AddWithValue("Id", currentTodo.Id);
                    command.Parameters.AddWithValue("Title", todoFromUser.Title);
                    command.Parameters.AddWithValue("Description", todoFromUser.Description);
                    command.Parameters.AddWithValue("Completed", todoFromUser.Completed);
                    command.Parameters.AddWithValue("UpdatedAt", now);

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
        public async Task DeleteVulnerable(int todoId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = $"Delete From Todo Where Id='{todoId}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    try
                    {
                        int affectedRows = command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                    }

                    connection.Close();
                }
            }
        }

        public async Task Delete(int todoId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("Delete FROM Todo WHERE Id = @todoId", connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("todoId", todoId);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAll()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("Delete from Todo", connection))
                {
                    command.CommandType = CommandType.Text;
                    int affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Todo> GetByIdVulnerable(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql;

                sql = $"Select * From Todo Where Id={id}";

                SqlCommand command = new SqlCommand(sql, connection);

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    if (await dataReader.ReadAsync())
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


        public async Task<Todo> GetById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand("Select * From Todo Where Id= @Id", connection);

                SqlParameter parameter = command.Parameters.Add("Id", SqlDbType.Int);
                parameter.Value = id;

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

                SqlCommand command = new SqlCommand("Select id From Todo Where Id= @Id", connection);

                SqlParameter parameter = command.Parameters.Add("Id", SqlDbType.Int);
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
    }
}