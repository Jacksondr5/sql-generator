using System;
using Core;

namespace ConsoleApp
{
    internal class UserInputRepository : IUserInputRepository
    {
        public string GetUserInput(string message)
        {
            Console.WriteLine(message);
            return Console.ReadLine();
        }
    }
}