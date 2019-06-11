using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;
using WebApiADO.NetCrud.Dtos.Responses.Shared;
using WebApiADO.NetCrud.Dtos.Responses.Todos;
using WebApiADO.NetCrud.Entities;
using WebApiADO.NetCrud.Enums;
using WebApiADO.NetCrud.Infrastructure.Services;

namespace WebApiADO.NetCrud.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/todos")]
    public class TodosController : ApiController
    {
        private readonly TodoService _todosService;


        public TodosController()
        {
            _todosService = new TodoService();
        }

        [HttpGet]
        public async Task<JsonResult<List<TodoDto>>> GetTodos()
        {
            var todos = await _todosService.FetchMany();
            return Json(TodoListResponse.Build(todos),
                GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
        }


        [HttpGet]
        [Route("pending")]
        public async Task<ResponseMessageResult> GetPending()
        {
            var todos = await _todosService.FetchMany(TodoShow.Pending);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, TodoListResponse.Build(todos),
                GlobalConfiguration.Configuration.Formatters.JsonFormatter));
            // return Ok(TodoListResponse.Build(todos)); <-- also works
        }

        [HttpGet]
        [Route("completed")]
        public async Task<JsonResult<List<TodoDto>>> GetCompleted()
        {
            var todos = await _todosService.FetchMany(TodoShow.Completed);
            return Json(TodoListResponse.Build(todos),
                GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
        }


        [HttpGet]
        [Route("{id}")]
        public async Task<HttpResponseMessage> GetTodoDetails(int id)
        {
            var todo = await _todosService.GetById(id);
            if (todo != null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ObjectContent(typeof(Todo), todo,
                    GlobalConfiguration.Configuration.Formatters.JsonFormatter);
                return response;
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.Content = new ObjectContent(typeof(ErrorDtoResponse), new ErrorDtoResponse("Not found"),
                    GlobalConfiguration.Configuration.Formatters.JsonFormatter);
                return response;
            }
        }


        [HttpPost]
        public async Task<CreatedNegotiatedContentResult<Todo>> CreateTodo([FromBody] Todo todo)
        {
            await _todosService.CreateTodo(todo);
            return Created("/api/todos/" + todo.Id, todo);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> UpdateTodo(int id, [FromBody] Todo todoFromUser)
        {
            var todoFromDb = await _todosService.GetProxyById(id);
            if (todoFromDb != null)
                return Ok(await _todosService.Update(todoFromDb, todoFromUser));
            return new ResponseMessageResult(new HttpResponseMessage
            {
                Content = new ObjectContent(typeof(object), new
                {
                    Success = false,
                    FullMessages = new[] {"Not Found"}
                }, GlobalConfiguration.Configuration.Formatters.JsonFormatter),
                StatusCode = HttpStatusCode.NotFound
            });
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<HttpResponseMessage> DeleteTodo(int id)
        {
            var todo = await _todosService.GetProxyById(id);
            if (todo != null)
            {
                await _todosService.Delete(id);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return Request.CreateResponse(HttpStatusCode.NotFound, new ErrorDtoResponse("Todo not Found"),
                GlobalConfiguration.Configuration.Formatters.JsonFormatter);
        }


        [HttpDelete]
        public async Task<ResponseMessageResult> DeleteAll()
        {
            await _todosService.DeleteAll();
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}