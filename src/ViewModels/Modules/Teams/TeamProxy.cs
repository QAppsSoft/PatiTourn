using System;
using DataModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace ViewModels.Modules.Teams
{
    public class TeamProxy : ValidatableViewModelBase
    {
        public TeamProxy(Team team)
        {
            Team = team ?? throw new ArgumentNullException(nameof(team));

            InitializeProperties(team);

            InitializeValidations();

            UpdateSkaterOnValueChange();
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

        private void UpdateSkaterOnValueChange()
        {
            this.WhenAnyValue(vm => vm.Name, vm => vm.Description)
                .Subscribe(value =>
                {
                    var (name, description) = value;
                    Team.Name = name;
                    Team.Description = description;
                });
        }

        private void InitializeProperties(Team team)
        {
            Name = team.Name;
            Description = team.Description;
        }

        public Team Team { get; }

        [Reactive] public string Name { get; set; } = null!;

        [Reactive] public string Description { get; set; } = null!;

        public static implicit operator Team(TeamProxy teamProxy)
        {
            return teamProxy.Team;
        }

        public static explicit operator TeamProxy(Team team)
        {
            return new TeamProxy(team);
        }
    }
}
