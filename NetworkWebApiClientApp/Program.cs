using System.Net.Http.Json;

namespace NetworkWebApiClientApp
{
    class Error
    {
        public string? Message { set; get; }
    }
    class User
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public int Age { set; get; }

        public override string ToString()
        {
            return $"User id: {Id}, name: {Name}, age: {Age}";
        }
    }
    internal class Program
    {
        static HttpClient httpClient = new HttpClient();
        static string url = @"https://localhost:7100/";
        static async Task Main(string[] args)
        {


            string answer;

            do
            {
                Console.WriteLine("1: list users");
                Console.WriteLine("2: list user");
                Console.WriteLine("3: add user");
                Console.WriteLine("4: update user");
                Console.WriteLine("5: delete user");
                Console.WriteLine("6: send form");
                Console.WriteLine("7: upload file");
                Console.WriteLine("8: upload files");
                Console.WriteLine("0: exit");

                answer = Console.ReadLine();

                if (answer == "0") break;

                switch(answer)
                {
                    case "1":
                        List<User>? users = await httpClient.GetFromJsonAsync<List<User>>($"{url}api/users");
                        if(users is not null)
                        {
                            Console.WriteLine("Users list:");
                            foreach(var u in users)
                                Console.WriteLine($"\t{u}");
                        }
                        break;
                    case "2":
                        Console.Write("Input id: ");
                        int id = Int32.Parse(Console.ReadLine()!);
                        using (var response = await httpClient.GetAsync($"{url}api/users/{id}"))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                Error? error = await response.Content.ReadFromJsonAsync<Error>();
                                Console.WriteLine(error?.Message);
                            }
                            else if(response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                User? u = await response.Content.ReadFromJsonAsync<User>();
                                Console.WriteLine($"\tUser: {u}");
                            }
                        }
                        
                        break;
                    case "3":
                        Console.Write("Input name: ");
                        string? name = Console.ReadLine();
                        Console.Write("Input age: ");
                        int age = Int32.Parse(Console.ReadLine()!);
                        User user = new User() { Name = name!, Age = age };
                        using (var response = await httpClient.PostAsJsonAsync($"{url}api/users/", user))
                        {
                            User? u = await response.Content.ReadFromJsonAsync<User>();
                            Console.WriteLine($"Addeded user: {u}");
                        }
                        break;
                    case "4":
                        Console.Write("Input id: ");
                        id = Int32.Parse(Console.ReadLine()!);
                        using (var response = await httpClient.GetAsync($"{url}api/users/{id}"))
                        {
                            User? u = await response.Content.ReadFromJsonAsync<User>();
                            Console.Write($"Name: {u.Name}, Input new name: ");
                            name = Console.ReadLine();
                            Console.Write($"Age: {u.Age}, Input new age: ");
                            age = Int32.Parse(Console.ReadLine()!);
                            user = new User() { Id = u.Id,  Name = name!, Age = age };
                            using (var resp = await httpClient.PutAsJsonAsync($"{url}api/users/", user))
                            {
                                if(resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                                {
                                    Error? error = await resp.Content.ReadFromJsonAsync<Error>();
                                    Console.WriteLine(error?.Message);
                                }
                                else if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    u = await resp.Content.ReadFromJsonAsync<User>();
                                    Console.WriteLine($"\tUpdated user: {u}");
                                }
                            }
                        }
                        break;
                    case "5":
                        Console.Write("Input id: ");
                        id = Int32.Parse(Console.ReadLine()!);

                        user = await httpClient.DeleteFromJsonAsync<User>($"{url}api/users/{id}");
                        Console.WriteLine($"\tDeleted user: {user}");
                        break;
                    case "6":
                        Dictionary<string, string> dataForm = new()
                        {
                            ["name"] = "Bobby",
                            ["age"] = "38"
                        };
                        HttpContent formContent = new FormUrlEncodedContent( dataForm );

                        using (var response = await httpClient.PostAsync($"{url}data", formContent))
                        {
                            string text = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(text);
                        }
                        break;
                    case "7":
                        string filePath = @"D:/img01.png";
                        using(var fileContent = new MultipartFormDataContent())
                        {
                            //var fstreamContent = new StreamContent(File.OpenRead(filePath));
                            //fstreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                            var contentBytes = new ByteArrayContent(fileBytes);
                            contentBytes.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

                            fileContent.Add(/*fstreamContent*/ contentBytes, name: "file", fileName: "img01.png");

                            using (var response = await httpClient.PostAsync($"{url}uploadfile", fileContent))
                            {
                                var respText = await response.Content.ReadAsStringAsync();
                                Console.WriteLine(respText);
                            }
                        }
                        break;
                    case "8":
                        string[] filesPath = new[] { @"D:/appstart.png", @"D:/img01-min.png" };
                        using (var fileContent = new MultipartFormDataContent())
                        {
                            foreach(var file in filesPath)
                            {
                                var fileName = Path.GetFileName(file);
                                var fstreamContent = new StreamContent(File.OpenRead(file));
                                fstreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                                fileContent.Add(fstreamContent, name: "file", fileName: fileName);
                            }

                            using (var response = await httpClient.PostAsync($"{url}uploadfile", fileContent))
                            {
                                var respText = await response.Content.ReadAsStringAsync();
                                Console.WriteLine(respText);
                            }
                        }
                        break;
                }

            } while (true);

        }
    }
}