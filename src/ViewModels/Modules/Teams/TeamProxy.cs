using System;
using System.Reactive.Disposables;
using DataModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace ViewModels.Modules.Teams
{
    public sealed class TeamProxy : ValidatableViewModelBase, IEntityProxy<Team>, IDisposable
    {
        private readonly CompositeDisposable _cleanup = new();

        public TeamProxy(Team team)
        {
            ArgumentNullException.ThrowIfNull(team);

            Entity = team;

            InitializeProperties(team);

            InitializeValidations();

            UpdateOnValueChange();
        }

        private void InitializeValidations()
        {
            // Name
            this.ValidationRule(viewModel => viewModel.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "You must specify a valid name");

            // Description
            this.ValidationRule(viewModel => viewModel.Description,
                name => !string.IsNullOrWhiteSpace(name),
                "You must specify a valid description");
        }

        private void UpdateOnValueChange()
        {
            this.WhenAnyValue(vm => vm.Name)
                .Subscribe(name => Entity.Name = name)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.Description)
                .Subscribe(description => Entity.Description = description)
                .DisposeWith(_cleanup);
        }

        private void InitializeProperties(Team team)
        {
            Name = team.Name;
            Description = team.Description;
        }

        public Team Entity { get; }

        [Reactive] public string Name { get; set; } = null!;

        [Reactive] public string Description { get; set; } = null!;

        public static implicit operator Team(TeamProxy teamProxy)
        {
            return teamProxy.Entity;
        }

        public static explicit operator TeamProxy(Team team)
        {
            return new TeamProxy(team);
        }

        public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}
