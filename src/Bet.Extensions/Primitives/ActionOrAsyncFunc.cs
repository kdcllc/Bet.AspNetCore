using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>
    /// Provides a way to create a executable task or section of code either way.
    /// </summary>
    public class ActionOrAsyncFunc
    {
        private readonly Action? _action;
        private readonly Func<Task>? _asyncAction;
        private readonly bool _isAsync;

        public ActionOrAsyncFunc(Action action)
        {
            _isAsync = false;
            _action = action;
        }

        public ActionOrAsyncFunc(Func<Task> asyncAction)
        {
            _isAsync = true;
            _asyncAction = asyncAction;
        }

        public async Task Invoke()
        {
            if (_isAsync
                && _asyncAction != null)
            {
                await _asyncAction();
            }
            else if (!_isAsync
                && _action != null)
            {
                _action();
            }
        }
    }
}
