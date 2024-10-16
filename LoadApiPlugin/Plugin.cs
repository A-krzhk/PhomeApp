using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;

[Author(Name = "Ivan Petrov")]
public class Plugin : IPluggable
{
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
    {
        var employeesList = args.Cast<EmployeesDTO>().ToList();
        logger.Info("Starting new user uploader");
        logger.Info("Type q or quit to exit");
        logger.Info("Available commands: get");

        string command = "";

        while (!command.ToLower().Contains("quit") && !command.ToLower().Contains("q"))
        {
            Console.Write("> ");
            command = Console.ReadLine();

            switch (command.ToLower())
            {
                case "get":
                    logger.Info("Uploading new users...");
                    LoadDataFromApi(employeesList);
                    logger.Info($"{employeesList.Count} users loaded from API and added to the list.");

                    int index = 0;
                    foreach (var employee in employeesList)
                    {
                        Console.WriteLine($"{index} Name: {employee.Name} | Phone: {employee.Phone}");
                        ++index;
                    }
                    break;
            }

            Console.WriteLine("");
        }

        return employeesList.Cast<DataTransferObject>();
    }

    private void LoadDataFromApi(List<EmployeesDTO> employeesList)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync("https://dummyjson.com/users").Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    var apiResponse = JsonConvert.DeserializeObject<UsersApiResponse>(jsonResponse);

                    foreach (var user in apiResponse.Users)
                    {
                        var employee = new EmployeesDTO($"{user.FirstName} {user.LastName}");
                        employee.AddPhone(user.Phone);
                        employeesList.Add(employee);
                    }
                }
                else
                {
                    logger.Error($"Failed to fetch data. Status Code: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error while fetching users: {ex.Message}");
        }
    }

    public class UsersApiResponse
    {
        public List<UserApi> Users { get; set; }
    }

    public class UserApi
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Phone { get; set; }
    }


}

