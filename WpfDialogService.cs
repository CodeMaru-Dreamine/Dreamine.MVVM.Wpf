using Dreamine.MVVM.Interfaces.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// \if KO
    /// <para>Wpf Dialog Service 기능과 관련 상태를 캡슐화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Default WPF implementation of <see cref="IDialogService"/> backed by <see cref="MessageBox"/>.</para>
    /// \endif
    /// </summary>
    public sealed class WpfDialogService : IDialogService
    {
        /// <summary>
        /// \if KO
        /// <para>Show Message 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show message operation.</para>
        /// \endif
        /// </summary>
        /// <param name="message">
        /// \if KO
        /// <para>처리할 메시지입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The message to process.</para>
        /// \endif
        /// </param>
        /// <param name="title">
        /// \if KO
        /// <para>title에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for title.</para>
        /// \endif
        /// </param>
        public void ShowMessage(string message, string title = "")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// \if KO
        /// <para>Show Error 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show error operation.</para>
        /// \endif
        /// </summary>
        /// <param name="message">
        /// \if KO
        /// <para>처리할 메시지입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The message to process.</para>
        /// \endif
        /// </param>
        /// <param name="title">
        /// \if KO
        /// <para>title에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for title.</para>
        /// \endif
        /// </param>
        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// \if KO
        /// <para>Confirm 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the confirm operation.</para>
        /// \endif
        /// </summary>
        /// <param name="message">
        /// \if KO
        /// <para>처리할 메시지입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The message to process.</para>
        /// \endif
        /// </param>
        /// <param name="title">
        /// \if KO
        /// <para>title에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for title.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>Confirm 조건이 충족되면 <see langword="true"/>이고, 그렇지 않으면 <see langword="false"/>입니다.</para>
        /// \endif
        /// \if EN
        /// <para><see langword="true"/> when the confirm condition is satisfied; otherwise, <see langword="false"/>.</para>
        /// \endif
        /// </returns>
        public bool Confirm(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// \if KO
        /// <para>Show Message Async 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show message async operation.</para>
        /// \endif
        /// </summary>
        /// <param name="message">
        /// \if KO
        /// <para>처리할 메시지입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The message to process.</para>
        /// \endif
        /// </param>
        /// <param name="title">
        /// \if KO
        /// <para>title에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for title.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>Show Message Async 작업에서 생성한 <see cref="Task"/> 결과입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Task"/> result produced by the show message async operation.</para>
        /// \endif
        /// </returns>
        public Task ShowMessageAsync(string message, string title = "")
        {
            ShowMessage(message, title);
            return Task.CompletedTask;
        }

        /// <summary>
        /// \if KO
        /// <para>Confirm Async 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the confirm async operation.</para>
        /// \endif
        /// </summary>
        /// <param name="message">
        /// \if KO
        /// <para>처리할 메시지입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The message to process.</para>
        /// \endif
        /// </param>
        /// <param name="title">
        /// \if KO
        /// <para>title에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for title.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>Confirm Async 작업에서 생성한 <c>Task&lt;bool&gt;</c> 결과입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <c>Task&lt;bool&gt;</c> result produced by the confirm async operation.</para>
        /// \endif
        /// </returns>
        public Task<bool> ConfirmAsync(string message, string title = "Confirm")
        {
            return Task.FromResult(Confirm(message, title));
        }
    }
}
