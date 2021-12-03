using System;
using System.Threading;

namespace BumbleBee.Code.Application.Services
{
    public class ProgressIndicator : IDisposable
    {
        private bool active;
        private Thread thread;
        private int _delay;

        public ProgressIndicator(int delay = 100)
        {
            thread = new Thread(DisplayStatus);
            _delay = delay;
        }

        public void Start()
        {
            active = true;
            if (!thread.IsAlive)
            {
                thread = new Thread(DisplayStatus);
                thread.Start();
            }
        }

        public void Stop()
        {
            active = false;
            Console.WriteLine();
            Console.SetCursorPosition(0, Console.CursorTop -1);
        }

        private void DisplayStatus()
        {
            Console.Write($"  ");
            while (active)
            {
                Console.Write(".");
                Thread.Sleep(_delay);
                _delay += 100;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}