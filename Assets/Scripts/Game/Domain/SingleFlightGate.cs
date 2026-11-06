using System;
using System.Threading.Tasks;

namespace Game.Domain
{
    // Runs one operation at a time; calls made while one is running are dropped.
    public class SingleFlightGate
    {
        private Task _running = Task.CompletedTask;

        public Task RunIfIdle(Func<Task> operation)
        {
            if (!_running.IsCompleted) return Task.CompletedTask;

            _running = operation();
            return _running;
        }
    }
}
