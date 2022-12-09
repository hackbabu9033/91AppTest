using _91AppTest.Access;
using _91AppTest.FileSystemProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace _91AppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();
            //新增 service connection
            var serviceCollection = new ServiceCollection()
           .AddSingleton(config)
           .AddScoped<FileSystemAccess>()
           .AddScoped<FileSystemService>()
           .BuildServiceProvider();
        }
    }
}
