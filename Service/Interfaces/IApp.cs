namespace Service.Interfaces
{
    public interface IApp
    {
        IContextContainer ContextContainer { get; }
        void RunDlls();
    }
}
