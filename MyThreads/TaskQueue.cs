using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MyThreads
{
    class TaskQueue
    {
        private object locker = new object();
        private int threadsCount;
        private Thread[] threads;
        private Queue<TaskDelegate> taskQueue = new Queue<TaskDelegate>();
        private Boolean isInterrupted = false;
        private int tasksCount = 0;
        private int lockedCount = 0;

        public delegate void TaskDelegate();
        public TaskQueue(int threadsCount) {
            if (threadsCount < 1)
                throw new ArgumentOutOfRangeException("Number of threads must be more than 0.");

            this.threadsCount = threadsCount;

            init();
        }

        private void init()
        {
            threads = new Thread[threadsCount];

            for (int i = 0; i < threadsCount; i++)
            {
                threads[i] = new Thread(new ThreadStart(Run));
                threads[i].Start();
            }
        }

        private void Run()
        {
            while (!isInterrupted)
            {
                TaskDelegate task = null;
                lock (locker)
                {
                    if (taskQueue.Count.Equals(0))
                    {
                        lockedCount++;
                        Monitor.Wait(locker);
                        lockedCount--;
                    }
                    else
                    {
                        task = taskQueue.Dequeue();
                        tasksCount++;
                        Monitor.Pulse(locker);
                    }
                }

                if (task != null)
                {
                    task();
                    Interlocked.Decrement(ref tasksCount);
                }

            }

        }

        public void Interrupt()
        {
            while (true)
            {
                Thread.Sleep(100);
                lock (locker)
                {
                    if (taskQueue.Count == 0  && tasksCount == 0)
                        break;
                }
            }

            lock (locker)
            {
                isInterrupted = true;
                Monitor.Pulse(locker);
            }
        }

        public void EnqueueTask(TaskDelegate task) {
            lock (locker)
            {
                taskQueue.Enqueue(task);
                if (lockedCount != 0)
                    Monitor.Pulse(locker);
            }
                
        }

    }
}
