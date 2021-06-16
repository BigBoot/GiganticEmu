using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace GiganticEmu.Launcher
{
    public partial class User
    {
        public User()
        {
            ViewModel = new UserViewModel();
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern(ButtonLogout, nameof(ButtonLogout.Click))
                    .Select(x => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel!.Logout)
                    .DisposeWith(disposables);
            });
        }
    }
}
