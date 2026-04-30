using Dreamine.MVVM.Interfaces.Windows;
using System;
using System.Collections.Concurrent;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// Tracks WPF window open states in memory.
    /// </summary>
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