namespace Reactivix.Thread
{
    public interface IReactivixThread
    {
        void ReactivixThreadStart(ReactivixThread context);
        void ReactivixThreadPipe(ReactivixThread context);
        void ReactivixThreadStop(ReactivixThread context);
    }
}