namespace opengen.threading
{
    public abstract class ThreaderTaskBase : IThreaderTask
    {
        private uint _id;
        private int _priority;
        private bool _complete;
        
        public uint id {get {return _id;}}
        public int priority {get {return _priority;}}
        public bool isComplete {get {return _complete;}}

        public ThreaderTaskBase(int priority = 0)
        {
            _id = Threader.GetId();
            _priority = priority;
            _complete = false;

            Threader.Log($"NEW ThreaderTask! {id}");
        }

        public void ResetPriority(int value)
        {
            _priority = value;
            Threader.Log($"{id} ResetPriority:{value}");
        }

        public virtual void PreExecute()
        {
            //threaded executable code would go here
            Threader.Log($"{id} PreExecute");
        }

        public virtual void Execute()
        {
            //threaded executable code would go here
            Threader.Log($"{id} Execute");
        }

        public virtual void PostExecute()
        {
            //threaded executable code would go here
            Threader.Log($"{id} PostExecute");
        }

        public void OnComplete()
        {
            _complete = true;
            // Debug.Log("BASE OnComplete "+_id);
            Threader.Log($"{id} OnComplete");
        }

        public override string ToString()
        {
            return $"THREADER TASK: id:{_id}, priority:{_priority}, complete:{_complete}";
        }
    }
}