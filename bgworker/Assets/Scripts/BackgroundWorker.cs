using UnityEngine;
using System.Collections;
using System.Threading;

namespace com.youvisio
{
    public delegate void DoWorkEventHandler(object sender, DoWorkEventArgs e);
    public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);

    public class BackgroundWorker
    {
        public event DoWorkEventHandler DoWork;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        private Thread _thread;
        private DoWorkEventArgs _doWorkArgs;
        private bool _isCanceled;

        public bool IsBusy
        {
            get { return _thread != null; }
        }

        public void RunWorkerAsync()
        {
            RunWorkerAsync(null);
        }

        public void RunWorkerAsync(object argument)
        {
            if (_thread != null)
            {
                throw new System.InvalidOperationException("Background Worker is already running");
            }

            _thread = new Thread(OnBackgroundWork);
            _thread.Name = "tBgW"+_thread.ManagedThreadId;
            _thread.IsBackground = true;
            _thread.Start(argument);
        }



        public void CancelAsync()
        {
            if (_thread == null)
            {
                return;
            }

            _isCanceled = true;
        }

        public void Update()
        {
            if (_thread != null && _doWorkArgs != null)
            {
                if (RunWorkerCompleted != null)
                {
                    RunWorkerCompleted(this, new RunWorkerCompletedEventArgs(_doWorkArgs.Result, _doWorkArgs.Error, _doWorkArgs.IsCanceled));
                }
                _thread = null;
                _doWorkArgs = null;
                _isCanceled = false;
            }
        }

        private void OnBackgroundWork(object arg)
        {
            if (DoWork != null)
            {
                var args = new DoWorkEventArgs(arg, () => _isCanceled);

                if (!_isCanceled)
                {
                    try
                    {
                        DoWork(this, args);
                    }
                    catch (System.Exception ex)
                    {
                        args.Error = ex;
                    }
                }
                
                _doWorkArgs = args;
            }
        }
    }

    public class DoWorkEventArgs : System.EventArgs
    {
        private System.Func<bool> _canceled;

        public DoWorkEventArgs(object argument, System.Func<bool> canceled)
        {
            Argument = argument;
            _canceled = (canceled == null) ? () => false : canceled;
        }
        public bool IsCanceled { get { return _canceled(); } }
        public object Argument { get; private set; }
        public object Result { get; set; }
        public System.Exception Error { get; set; }

    }

    public class RunWorkerCompletedEventArgs : System.EventArgs
    {
        public RunWorkerCompletedEventArgs(object result, System.Exception error, bool canceled)
        {
            Result = result;
            Error = error;
            IsCanceled = canceled;
        }
        public bool IsCanceled { get; private set; }
        public object Result { get; private set; }
        public System.Exception Error { get; private set; }
    }
}