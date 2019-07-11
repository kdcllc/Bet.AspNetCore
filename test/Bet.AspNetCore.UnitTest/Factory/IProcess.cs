namespace Bet.AspNetCore.UnitTest.Factory
{
    public interface IProcess<T>
    {
        void Run();
    }

    public interface IProcess
    {
        void Run();
    }
}
