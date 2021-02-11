using System;

namespace RequestManager
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test API -> https://jsonplaceholder.typicode.com/todos/
            string endPoint = "https://jsonplaceholder.typicode.com/todos/";
            string query = $"1";

            RequestManager requestManager = new RequestManager(endPoint);

            var result = requestManager.SendRequest(query, null, method: "GET");
        }
    }
}
