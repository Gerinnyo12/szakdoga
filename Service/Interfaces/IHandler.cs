namespace Service.Interfaces
{
    public interface IHandler
    {
        IContextContainer ContextContainer { get; }
        void RunDlls();
    }
}
