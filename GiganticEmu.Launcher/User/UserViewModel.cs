using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Reactive;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher
{
    public class UserViewModel : ReactiveObject
    {
        [Reactive]
        public bool IsLoading { get; set; } = false;

        [Reactive]
        public UserData? User { get; set; } = null;

        public ReactiveCommand<Unit, Unit> Logout { get; }

        public UserViewModel()
        {
            Logout = ReactiveCommand.CreateFromTask(DoLogout);
        }

        private async Task DoLogout()
        {
            IsLoading = true;
            await Locator.Current.GetService<CredentialStorage>()!.ClearToken();
            User = null;
            IsLoading = false;
        }
    }
}
