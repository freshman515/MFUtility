using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MFUtility.Bases {
	#region RelayCommand（同步命令）

	public class RelayCommand : ICommand {
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

		public RelayCommand(Action execute)
			: this(execute, null) {
		}

		public RelayCommand(Action execute, Func<bool> canExecute) {
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged {
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter) {
			return _canExecute == null || _canExecute();
		}

		public void Execute(object parameter) {
			_execute();
		}
	}


	public class RelayCommand<T> : ICommand {
		private readonly Action<T> _execute;
		private readonly Func<T, bool> _canExecute;

		public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null) {
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}


		public event EventHandler CanExecuteChanged {
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public bool CanExecute(object parameter) {
			if (parameter is T value)
				return _canExecute?.Invoke(value) ?? true;

			if (parameter == null && (!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null))
				return _canExecute?.Invoke(default) ?? true;

			return false;
		}

		public void Execute(object parameter) {
			if (parameter is Expression<Func<T>> expr) {
				var compiled = expr.Compile();
				_execute(compiled());
			} else if (parameter is T value) {
				_execute(value);
			} else if (parameter == null && (!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null)) {
				_execute(default);
			} else {
				throw new InvalidCastException($"无法将 {parameter?.GetType()?.FullName ?? "null"} 转换为 {typeof(T)}");
			}
		}

		public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
	}

	#endregion

	#region AsyncRelayCommand（异步命令）

	public class AsyncRelayCommand : ICommand {
		private readonly Func<Task> _execute;
		private readonly Func<bool> _canExecute;
		private bool _isExecuting;

		public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null) {
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged {
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public bool CanExecute(object parameter) {
			return !_isExecuting && (_canExecute == null || _canExecute());
		}

		public async void Execute(object parameter) {
			if (!CanExecute(parameter)) return;

			try {
				_isExecuting = true;
				RaiseCanExecuteChanged();
				await _execute();
			} finally {
				_isExecuting = false;
				RaiseCanExecuteChanged();
			}
		}

		public void RaiseCanExecuteChanged() {
			CommandManager.InvalidateRequerySuggested();
		}
	}

	public class AsyncRelayCommand<T> : ICommand {
		private readonly Func<T, CancellationToken, Task> _execute;
		private readonly Predicate<T> _canExecute;
		private CancellationTokenSource _cts;
		private bool _isExecuting;

		public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
			: this((arg, _) => execute(arg), canExecute) {
		}

		public AsyncRelayCommand(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null) {
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}


		public event EventHandler CanExecuteChanged {
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public bool CanExecute(object parameter) {
			if (_isExecuting)
				return false;

			if (parameter is T value)
				return _canExecute?.Invoke(value) ?? true;

			if (parameter == null && (!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null))
				return _canExecute?.Invoke(default) ?? true;

			return false;
		}

		public async void Execute(object parameter) {
			T arg = default;
			if (parameter is T v)
				arg = v;

			_isExecuting = true;
			_cts = new CancellationTokenSource();
			RaiseCanExecuteChanged();

			try {
				await _execute(arg, _cts.Token);
			} finally {
				_isExecuting = false;
				_cts.Dispose();
				_cts = null;
				RaiseCanExecuteChanged();
			}
		}

		public void Cancel() {
			if (_isExecuting)
				_cts?.Cancel();
		}

		public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
	}

	#endregion
}