using Dreamine.MVVM.Interfaces.Windows;
using System;
using System.Collections.Concurrent;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// Tracks WPF window open states in memory.
    /// </summary>
    /// <remarks>
    /// This implementation is designed for single-threaded use on the WPF UI thread.
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> protects individual dictionary operations,
    /// but the write-then-event pattern in <c>SetState</c> is not atomic.
    /// Do not call <see cref="MarkOpened"/> or <see cref="MarkClosed"/> from background threads.
    /// </remarks>
    public sealed class WindowStateService : IWindowStateService
    {
        private readonly ConcurrentDictionary<string, bool> _states = new();

        /// <inheritdoc />
        public event EventHandler<WindowStateChangedEventArgs>? StateChanged;

        /// <inheritdoc />
        public bool IsOpen(string windowKey)
        {
            ValidateWindowKey(windowKey);

            return _states.TryGetValue(windowKey, out bool isOpen) && isOpen;
        }

        /// <inheritdoc />
        public void MarkOpened(string windowKey)
        {
            SetState(windowKey, true);
        }

        /// <inheritdoc />
        public void MarkClosed(string windowKey)
        {
            SetState(windowKey, false);
        }

        private void SetState(string windowKey, bool isOpen)
        {
            ValidateWindowKey(windowKey);

            _states[windowKey] = isOpen;
            StateChanged?.Invoke(this, new WindowStateChangedEventArgs(windowKey, isOpen));
        }

        private static void ValidateWindowKey(string windowKey)
        {
            if (string.IsNullOrWhiteSpace(windowKey))
            {
                throw new ArgumentException("Window key must not be empty.", nameof(windowKey));
            }
        }
    }
}