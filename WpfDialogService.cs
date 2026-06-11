using Dreamine.MVVM.Interfaces.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// Default WPF implementation of <see cref="IDialogService"/> backed by <see cref="MessageBox"/>.
    /// </summary>
    public sealed class WpfDialogService : IDialogService
    {
        /// <inheritdoc />
        public void ShowMessage(string message, string title = "")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <inheritdoc />
        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <inheritdoc />
        public bool Confirm(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        /// <inheritdoc />
        public Task ShowMessageAsync(string message, string title = "")
        {
            ShowMessage(message, title);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> ConfirmAsync(string message, string title = "Confirm")
        {
            return Task.FromResult(Confirm(message, title));
        }
    }
}
