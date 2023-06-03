using System.Xml.Linq;

namespace NetworkWebApiApp
{
    class User
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public int Age { set; get; }
    }
    public class Program
    {
        
        public static void Main(string[] args)
        {
            int id = 1;

            List<User> users = new List<User>()
            {
            new(){ Id = id++, Name = "Sam", Age = 27 },
            new(){ Id = id++, Name = "Joe", Age = 34 },
            new(){ Id = id++, Name = "Ben", Age = 21 },
            new(){ Id = id++, Name = "Leo", Age = 29 },
            };
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/api/users", () => users);
            app.MapGet("/api/users/{id}", (int id) =>
            {
                User? user = users.FirstOrDefault(u => u.Id == id);
                if (user is not null)
                    return Results.Json(user);
                else
                    return Results.NotFound(new { Message = "User not found" });
            });

            app.MapPost("/api/users", (User user) => 
            {
                user.Id = id++;
                users.Add(user);

                return user;
            });

            app.MapPut("/api/users", (User user) =>
            {
                User? userOld = users.FirstOrDefault(u => u.Id == user.Id);
                if (userOld is null) return Results.NotFound(new { Message = "User not found" });

                userOld.Name = user.Name;
                userOld.Age = user.Age;

                return Results.Json(userOld);
            });

            app.MapDelete("/api/users/{id}", (int id) =>
            {
                User? user = users.FirstOrDefault(u => u.Id == id);
                if(user is null) return Results.NotFound(new { Message = "User not found" });

                users.Remove(user);

                return Results.Json(user);
            });

            app.MapPost("/data", async (HttpContext context) =>
            {
                var form = context.Request.Form;
                string? name = form["name"];
                string? age = form["age"];
                await context.Response.WriteAsync($"User info from form: {name} {age}");
            });

            app.MapPost("/uploadfile", async (HttpContext context) =>
            {
                IFormFileCollection files = context.Request.Form.Files;
                var uploadFilesPath = $"{Directory.GetCurrentDirectory()}/uploads";
                Directory.CreateDirectory(uploadFilesPath);
                foreach(var file in files)
                {
                    string filePath = $"{uploadFilesPath}/{file.FileName}";
                    using(var fstream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fstream);
                    }
                }
            });

            app.Run();
        }
    }
}