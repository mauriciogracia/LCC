using Domain.Interfaces;

namespace Infrastructure
{
    public class ConsoleLogger : ILog
    {
        public void error(string message)
        {
            Console.Error.WriteLine(message);
        }

        public void info(string message)
        {
            Console.Out.WriteLine(message);
        }
    }
}
