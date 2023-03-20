using System;
using UnityEngine;

namespace opengen.threading
{
    public class ThreaderThread
    {
        protected readonly System.Threading.Thread _thread;
        protected IThreaderTask _task = null;
        protected bool _abort;
        protected const int THREAD_SLEEP_VALUE = 5;

        public delegate void OnTaskCompleteEventHandler(ThreaderThread thread);
        public event OnTaskCompleteEventHandler OnTaskCompleteEvent;
        
        public IThreaderTask task {get {return _task;}}

        public ThreaderThread()
        {
            //Debug.Log("THREADER: NEW ThreaderThread");
            _thread = new System.Threading.Thread(() => ThreadWork());
            _thread.Start();
        }

        public void StartTask(IThreaderTask newTask)
        {
            newTask.PreExecute();
            _task = newTask;
        }

        protected void ThreadWork()
        {
            while(!_abort)
            {
                if(_task != null)
                {
                    if(!_task.isComplete)
                    {
                        try
                        {
                            _task.Execute();
                        }
                        catch(Exception e)
                        {
                            Threader.LogError("Threader Error: " + e);
                        }
                        finally
                        {
                            if (_abort)
                            {
                                //skip
                            }
                            else
                            {
                                _task.OnComplete();
                                OnTaskCompleteEvent?.Invoke(this);
                            }
                        }
                    }
                }
                //System.Threading.Thread.Sleep(THREAD_SLEEP_VALUE);
            }

            Threader.Log("Threader Thread ENDED");
        }

        public void Stop()
        {
            _abort = true;
        }

        public override string ToString()
        {
            return $"THREADER THREAD: {_task}";
        }
    }
}