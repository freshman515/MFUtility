using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MFUtility.WPF.Bases;

  public class BindableCollection<T> : ObservableCollection<T>
    {
        private readonly SynchronizationContext? _sync;

        public BindableCollection()
        {
            IsNotifying = true;
            _sync = SynchronizationContext.Current;
        }

        public BindableCollection(IEnumerable<T> collection)
            : base(new List<T>(collection))
        {
            IsNotifying = true;
            _sync = SynchronizationContext.Current;
        }

        /// <summary>
        /// 是否通知 UI
        /// </summary>
        public bool IsNotifying { get; set; }

        #region === UI Thread 调用封装 ===

        protected void OnUIThread(Action action)
        {
            if (_sync == null)
            {
                action();
                return;
            }

            _sync.Post(_ => action(), null);
        }

        #endregion

        #region === Override 通知 ===

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!IsNotifying) return;

            if (_sync != null)
            {
                OnUIThread(() => base.OnCollectionChanged(e));
            }
            else
            {
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!IsNotifying) return;

            if (_sync != null)
            {
                OnUIThread(() => base.OnPropertyChanged(e));
            }
            else
            {
                base.OnPropertyChanged(e);
            }
        }

        #endregion

        #region === 基础操作增强 ===

        protected override sealed void InsertItem(int index, T item)
        {
            if (_sync != null)
                OnUIThread(() => InsertItemBase(index, item));
            else
                InsertItemBase(index, item);
        }

        protected virtual void InsertItemBase(int index, T item)
        {
            base.InsertItem(index, item);
        }

        protected override sealed void SetItem(int index, T item)
        {
            if (_sync != null)
                OnUIThread(() => SetItemBase(index, item));
            else
                SetItemBase(index, item);
        }

        protected virtual void SetItemBase(int index, T item)
        {
            base.SetItem(index, item);
        }

        protected override sealed void RemoveItem(int index)
        {
            if (_sync != null)
                OnUIThread(() => RemoveItemBase(index));
            else
                RemoveItemBase(index);
        }

        protected virtual void RemoveItemBase(int index)
        {
            base.RemoveItem(index);
        }

        protected override sealed void ClearItems()
        {
            if (_sync != null)
                OnUIThread(ClearItemsBase);
            else
                ClearItemsBase();
        }

        protected virtual void ClearItemsBase()
        {
            base.ClearItems();
        }

        #endregion

        #region === AddRange / RemoveRange / Refresh ===

        public void AddRange(IEnumerable<T> items)
        {
            void Execute()
            {
                var notify = IsNotifying;
                IsNotifying = false;

                int index = Count;
                foreach (var item in items)
                {
                    InsertItemBase(index++, item);
                }

                IsNotifying = notify;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            OnUIThread(Execute);
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            void Execute()
            {
                var notify = IsNotifying;
                IsNotifying = false;

                foreach (var item in items)
                {
                    int index = IndexOf(item);
                    if (index >= 0)
                        RemoveItemBase(index);
                }

                IsNotifying = notify;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            OnUIThread(Execute);
        }

        public void Refresh()
        {
            OnUIThread(() =>
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        #endregion
    }