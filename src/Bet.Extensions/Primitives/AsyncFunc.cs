using System.Threading.Tasks;

namespace System.Threading
{
    public class AsyncFunc<T>
    {
        private readonly Func<T>? _func;
        private readonly Func<Task<T>>? _funcTask;
        private readonly bool _isFuncTask;

        public AsyncFunc(Func<T> func)
        {
            _isFuncTask = false;
            _func = func;
        }

        public AsyncFunc(Func<Task<T>> funcTask)
        {
            _isFuncTask = true;
            _funcTask = funcTask;
        }

        public async Task<T> Invoke()
        {
            if (_isFuncTask
                && _funcTask != null)
            {
                return await _funcTask();
            }
            else if (!_isFuncTask
                && _func != null)
            {
                return await Task.FromResult(_func());
            }

            return default!;
        }
    }
}
