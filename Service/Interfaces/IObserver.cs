namespace Service.Interfaces
{
    public interface IObserver
    {
        IContextContainer ContextContainer { get; }
        void RunDlls();
    }
}
