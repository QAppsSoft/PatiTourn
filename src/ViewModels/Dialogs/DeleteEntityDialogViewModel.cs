using System.Reactive;
using ReactiveUI;

namespace ViewModels.Dialogs
{
    public class DeleteEntityDialogViewModel : ViewModelBase
    {
        public DeleteEntityDialogViewModel()
        {
            OkCommand = ReactiveCommand.Create(() => CanDelete = true);
            CancelCommand = ReactiveCommand.Create(() => CanDelete = false);
        }

        public ReactiveCommand<Unit, bool> OkCommand { get; }
        public ReactiveCommand<Unit, bool> CancelCommand { get; }

        public bool CanDelete { get; set; }
    }
}
