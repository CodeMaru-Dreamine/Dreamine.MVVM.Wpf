using Dreamine.MVVM.Interfaces.Windows;
using System;
using System.Collections.Concurrent;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// \if KO
    /// <para>Window State Service 기능과 관련 상태를 캡슐화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Tracks WPF window open states in memory.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>이 멤버의 동작과 사용 시 고려 사항을 설명합니다.</para>
    /// \endif
    /// \if EN
    /// <para>This implementation is designed for single-threaded use on the WPF UI thread. <see cref="ConcurrentDictionary{TKey,TValue}"/> protects individual dictionary operations, but the write-then-event pattern in <c>SetState</c> is not atomic. Do not call <see cref="MarkOpened"/> or <see cref="MarkClosed"/> from background threads.</para>
    /// \endif
    /// </remarks>
    public sealed class WindowStateService : IWindowStateService
    {
        /// <summary>
        /// \if KO
        /// <para>states 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the states value.</para>
        /// \endif
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _states = new();

        /// <summary>
        /// \if KO
        /// <para>State Changed 상황이 발생할 때 알립니다.</para>
        /// \endif
        /// \if EN
        /// <para>Occurs when state changed takes place.</para>
        /// \endif
        /// </summary>
        public event EventHandler<WindowStateChangedEventArgs>? StateChanged;

        /// <summary>
        /// \if KO
        /// <para>Is Open 조건을 확인합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Determines whether is open.</para>
        /// \endif
        /// </summary>
        /// <param name="windowKey">
        /// \if KO
        /// <para>window Key에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for window key.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>Is Open 조건이 충족되면 <see langword="true"/>이고, 그렇지 않으면 <see langword="false"/>입니다.</para>
        /// \endif
        /// \if EN
        /// <para><see langword="true"/> when the is open condition is satisfied; otherwise, <see langword="false"/>.</para>
        /// \endif
        /// </returns>
        public bool IsOpen(string windowKey)
        {
            ValidateWindowKey(windowKey);

            return _states.TryGetValue(windowKey, out bool isOpen) && isOpen;
        }

        /// <summary>
        /// \if KO
        /// <para>Mark Opened 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the mark opened operation.</para>
        /// \endif
        /// </summary>
        /// <param name="windowKey">
        /// \if KO
        /// <para>window Key에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for window key.</para>
        /// \endif
        /// </param>
        public void MarkOpened(string windowKey)
        {
            SetState(windowKey, true);
        }

        /// <summary>
        /// \if KO
        /// <para>Mark Closed 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the mark closed operation.</para>
        /// \endif
        /// </summary>
        /// <param name="windowKey">
        /// \if KO
        /// <para>window Key에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for window key.</para>
        /// \endif
        /// </param>
        public void MarkClosed(string windowKey)
        {
            SetState(windowKey, false);
        }

        /// <summary>
        /// \if KO
        /// <para>State 값을 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Sets the state value.</para>
        /// \endif
        /// </summary>
        /// <param name="windowKey">
        /// \if KO
        /// <para>window Key에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for window key.</para>
        /// \endif
        /// </param>
        /// <param name="isOpen">
        /// \if KO
        /// <para>is Open에 사용할 <see cref="bool"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="bool"/> value used for is open.</para>
        /// \endif
        /// </param>
        private void SetState(string windowKey, bool isOpen)
        {
            ValidateWindowKey(windowKey);

            _states[windowKey] = isOpen;
            StateChanged?.Invoke(this, new WindowStateChangedEventArgs(windowKey, isOpen));
        }

        /// <summary>
        /// \if KO
        /// <para>Window Key 값의 유효성을 검사합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Validates the window key value.</para>
        /// \endif
        /// </summary>
        /// <param name="windowKey">
        /// \if KO
        /// <para>window Key에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for window key.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentException">
        /// \if KO
        /// <para>입력 인자가 유효하지 않은 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when an input argument is invalid.</para>
        /// \endif
        /// </exception>
        private static void ValidateWindowKey(string windowKey)
        {
            if (string.IsNullOrWhiteSpace(windowKey))
            {
                throw new ArgumentException("Window key must not be empty.", nameof(windowKey));
            }
        }
    }
}