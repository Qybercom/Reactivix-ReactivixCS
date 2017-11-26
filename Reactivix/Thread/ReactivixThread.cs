using System;
using System.Collections.Generic;

/**
 * https://www.dotnetperls.com/using-alias
 */
using SystemThread = System.Threading.Thread;

/**
 * https://www.youtube.com/watch?v=ja63QO1Imck
 */
namespace Reactivix.Thread
{
    public class ReactivixThread
    {
        public const int TICK = 10; // Milliseconds

        public int Tick { get; set; } = TICK;

        private IReactivixThread _context { get; set; }
        private SystemThread _thread { get; set; }

        private List<Action<IReactivixThread>> _tasksInternal { get; set; }
        private List<Action<object>> _tasksExternal { get; set; }
        private Action<IReactivixThread> _stop { get; set; }

        public ReactivixThread(IReactivixThread context)
        {
            _context = context;
            _thread = new SystemThread(_process);

            _tasksInternal = new List<Action<IReactivixThread>>();
            _tasksExternal = new List<Action<object>>();
        }

        public void Pipe(object context = null)
        {
            while (_tasksExternal.Count > 0)
            {
                _tasksExternal[0](context);
                _tasksExternal.RemoveAt(0);
            }
        }

        private void _process(object sender)
        {
            if (sender != null)
            {
                Action<IReactivixThread> start = sender as Action<IReactivixThread>;
                start(_context);
            }

            _context.ReactivixThreadStart(this);

            var run = true;

            while (run)
            {
                while (_tasksInternal.Count > 0)
                {
                    _tasksInternal[0](_context);
                    _tasksInternal.RemoveAt(0);
                }

                _context.ReactivixThreadPipe(this);

                _stop?.Invoke(_context);

                SystemThread.Sleep(Tick);
            }
        }

        public void Start(Action<IReactivixThread> start = null)
        {
            _thread.Start(start);
        }

        public void Stop(Action<IReactivixThread> stop = null, bool forced = false)
        {
            _stop = stop;

            if (forced) _thread.Abort();
        }

        public void Internal(Action<IReactivixThread> task)
        {
            _tasksInternal.Add(task);
        }

        public void External(Action<object> task)
        {
            _tasksExternal.Add(task);
        }
    }
}