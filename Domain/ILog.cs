namespace Domain
{
    public interface ILog
    {
        void error(string message);

        void info(string message);
    }
}
