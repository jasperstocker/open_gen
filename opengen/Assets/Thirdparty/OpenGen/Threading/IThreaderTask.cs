namespace opengen.threading
{
    public interface IThreaderTask
    {
        uint id {get;}
        int priority {get;}
        bool isComplete {get;}
        void PreExecute();
        void Execute();
        void PostExecute();
        void OnComplete();

    }
}