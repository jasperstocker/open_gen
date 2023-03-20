using System.Collections.Generic;
using UnityEngine;

namespace opengen.threading
{
    public class Threader : MonoBSingleton<Threader>
    {
        public static bool UNITY_LOGGING = false;
        protected int _threadCount = 2;
        protected ThreaderThread[] _threads = new ThreaderThread[0];
        protected List<IThreaderTask> _taskQueue = new List<IThreaderTask>();

        public delegate void OnThreadTaskCompleteEventHandler(IThreaderTask task);
        public OnThreadTaskCompleteEventHandler OnThreadTaskCompleteEvent;

        public int threadCount
        {
            get
            {
                return _threadCount;
            }
            set
            {
                _threadCount = value;
            }
        }

        public bool prioritised { get; set; }

        private void OnDisable()
        {
            Abort();
        }
        
        public void Add(IThreaderTask newTask)
        {
            AddInternal(newTask);
            StartThreads();
        }

        public void Add(IList<IThreaderTask> newTasks)
        {
            for(int i = 0; i < newTasks.Count; i++)
            {
                AddInternal(newTasks[i]);
            }

            StartThreads();
        }

        public void PrioritiseThreads()
        {
            prioritised = true;
        }

        public void Continue()
        {
            Threader.Log("CONTINUE");
            StartThreads();
        }

        public void Pause()
        {
            Threader.Log("PAUSE");
            StopThreads(true);
        }

        public void Abort()
        {
            Threader.Log("END");
            StopThreads();
            ClearThreadQueue();
        }

        private void AddInternal(IThreaderTask newTask)
        {
            if(!prioritised)
            {
                prioritised = newTask.priority != 0;
            }

            _taskQueue.Add(newTask);
        }

        private void StartThreads()
        {
            if(_taskQueue.Count == 0)
            {
                return;
            }

            int currentThreadCount = _threads.Length;
            if (currentThreadCount == 0)
            {
                Threader.Log("create thread array");
                _threads = new ThreaderThread[_threadCount];
            }

            for (int i = 0; i < _threadCount; i++)
            {
                if (_threads[i] == null)
                {
                    IThreaderTask task = GetNextTask();
                    if (task != null) //more tasks to complete
                    {
                        Threader.Log($" {i} new task thread");
                        _threads[i] = new ThreaderThread();
                        _threads[i].OnTaskCompleteEvent += OnThreadComplete;
                        _threads[i].StartTask(task);
                    }
                }
            }
        }

        private void StopThreads(bool keepTasks = false)
        {
            int currentThreadCount = _threads.Length;
            for(int c = 0; c < currentThreadCount; c++)
            {
                if(_threads[c] == null)
                {
                    continue;
                }

                if (keepTasks)
                {
                    IThreaderTask task = _threads[c].task;
                    if(task != null)
                    {
                        _taskQueue.Insert(0, task);
                    }
                }
                _threads[c].OnTaskCompleteEvent -= OnThreadComplete;
                _threads[c].Stop();
                _threads[c] = null;
            }

            _threads = new ThreaderThread[0];
        }

        private void ClearThreadQueue()
        {
            _taskQueue.Clear();
        }

        private void OnThreadComplete(ThreaderThread thread)
        {
            Threader.Log("on thread complete");
            IThreaderTask completedTask = thread.task;
            completedTask.PostExecute();
            if(OnThreadTaskCompleteEvent != null)
            {
                OnThreadTaskCompleteEvent(completedTask);
            }

            IThreaderTask nextTask = GetNextTask();
            if (nextTask != null)
            {
                Threader.Log("new task");
                thread.StartTask(nextTask);
            }
            else
            {
                Threader.Log("close thread");
                thread.OnTaskCompleteEvent -= OnThreadComplete;
                Threader.Log("thread deafened");
                thread.Stop();
                Threader.Log("thread stopped");
                
                int activeThreadCount = 0;
                for (int i = 0; i < _threadCount; i++)
                {
                    if (_threads[i] == thread)
                    {
                        _threads[i] = null;
                    }
                    else if (_threads[i] != null)
                    {
                        activeThreadCount++;
                    }
                }

                Threader.Log($"active threads {activeThreadCount}");
                if (activeThreadCount == 0)
                {
                    Threader.Log("tasks complete");
                    _threads = new ThreaderThread[0];
                }
            }
        }

        private IThreaderTask GetNextTask()
        {
            return prioritised ? GetNextPriorityTask() : GetNextQueuedTask();
        }
        
        private IThreaderTask GetNextQueuedTask()
        {
            if (_taskQueue.Count > 0)
            { 
                IThreaderTask output = _taskQueue[0];
                _taskQueue.RemoveAt(0);
                return output;
            }
            return null;
        }

        private IThreaderTask GetNextPriorityTask()
        {
            int queueSize = _taskQueue.Count;
            int highestPrio = int.MinValue;
            int candidate = -1;
            for(int q = 0; q < queueSize; q++)
            {
                IThreaderTask candidateItem = _taskQueue[q];
                if(candidateItem.priority > highestPrio)
                {
                    highestPrio = candidateItem.priority;
                    candidate = q;
                }
            }

            if(candidate != -1)
            {
                IThreaderTask output = _taskQueue[candidate];
                // Threader.Log("THREADER GetNextPriorityTask candidate "+output.id+" "+output.isComplete+" "+output.ready);
                _taskQueue.RemoveAt(candidate);
                return output;
            }
            return null;
        }

        #region ID Generation

        private static uint ID_COUNT = 0;
        public static uint GetId()
        {
            if(ID_COUNT < uint.MaxValue)
            {
                ID_COUNT++;
            }
            else
            {
                ID_COUNT = 0;
            }

            // Threader.Log("GetId "+ID_COUNT);
            return ID_COUNT;
        }

        #endregion

        #region Progress

        private int totalTasks = 0;
        public void SetTaskCount(int value)
        {
            totalTasks = value;
        }

        public float Percent()
        {
            if(totalTasks == 0)
            {
                return 0f;
            }

            if(_taskQueue.Count == 0)
            {
                return 1f;
            }

            return totalTasks / (float)_taskQueue.Count;
        }

        public int pendingTaskCount => _taskQueue.Count;

        public void ClearTotal()
        {
            totalTasks = 0;
        }
        
        #endregion

        #region Debug
        public static void Log(string message)
        {
            if(UNITY_LOGGING)
            {
                Debug.Log(message);
            }
        }

        public static void Log(object obj)
        {
            if(UNITY_LOGGING)
            {
                Debug.Log(obj);
            }
        }

        public static void LogError(string message)
        {
            if(UNITY_LOGGING)
            {
                Debug.LogError(message);
            }
        }
        #endregion
    }
}