using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Common;
using DataModel;
using DataModel.Enums;
using Domain.Services.Interfaces;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ViewModels.Extensions;

namespace ViewModels.Modules.Skaters
{
    public sealed class SkaterProxy : ValidatableViewModelBase, IEntityProxy<Skater>, IDisposable
    {
        private readonly CompositeDisposable _cleanup = new();

        public SkaterProxy(Skater skater, IEntityProvider<Team> teamsProvider, ISchedulerProvider schedulerProvider)
        {
            ArgumentNullException.ThrowIfNull(skater);
            ArgumentNullException.ThrowIfNull(teamsProvider);
            ArgumentNullException.ThrowIfNull(schedulerProvider);

            Entity = skater;

            var teamsDisposable = teamsProvider.List.Connect()
                .ObserveOn(schedulerProvider.Dispatcher)
                .Bind(out var teams)
                .Subscribe()
                .DisposeWith(_cleanup);

            Teams = teams;

            InitializeProperties(skater);

            InitializeValidations();

            UpdateOnValueChange();

            this.WhenAnyValue(vm => vm.IdentificationNumber)
                .WhereNotNull()
                .Where(value => value.Length == 11)
                .Select(GetBirthDate)
                .ToPropertyEx(this, vm => vm.BirthDate);

            this.WhenAnyValue(vm => vm.BirthDate)
                .Select(birthDate => birthDate.Age())
                .ToPropertyEx(this, vm => vm.Age);
        }

        public Skater Entity { get; }
        [Reactive] public int Number { get; set; }

        [Reactive] public string Name { get; set; } = null!;

        [Reactive] public string LastNames { get; set; } = null!;

        [Reactive] public string IdentificationNumber { get; set; } = null!;

        [Reactive] public Sex? Sex { get; set; }

        [Reactive] public Team? Team { get; set; }

        [ObservableAsProperty] public DateTime BirthDate { get; }

        [ObservableAsProperty] public int Age { get; }

        public ReadOnlyObservableCollection<Team> Teams { get; }

        private void InitializeProperties(Skater skater)
        {
            Number = skater.Number;
            Name = skater.Name;
            LastNames = skater.LastNames;
            IdentificationNumber = skater.IdentificationNumber;
            Sex = skater.Sex;

            if (skater.TeamId == default)
            {
                return;
            }

            Team = Teams.FirstOrDefault(team => team.Id == skater.TeamId);
        }

        private void InitializeValidations()
        {
            // Name
            this.ValidationRule(viewModel => viewModel.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "You must specify a valid name");

            // Lastname
            this.ValidationRule(viewModel => viewModel.LastNames,
                name => !string.IsNullOrWhiteSpace(name),
                "You must specify a valid lastname");

            this.ValidationRule(viewModel => viewModel.LastNames,
                name => name != null && name.Contains(" ", StringComparison.InvariantCulture),
                "You must specify both lastname");

            // Identification Number
            // TODO: Use a more generic Identification Number validation rules according to others nationalities
            this.ValidationRule(viewModel => viewModel.IdentificationNumber,
                identificationNumber => identificationNumber is { Length: 11 },
                "Identification number must contain 11 positions");

            this.ValidationRule(viewModel => viewModel.IdentificationNumber,
                identificationNumber => identificationNumber.OnlyDigits(),
                "Identification number must contain only numbers");

            // Sex
            this.ValidationRule(viewModel => viewModel.Sex,
                sex => sex.HasValue,
                "You must provide a value for Sex");

            // Number
            this.ValidationRule(viewModel => viewModel.Number,
                number => number > 0,
                "The number must be higher than zero");

            // Team
            this.ValidationRule(viewmodel => viewmodel.Team,
                team => team != null,
                "Team is required");
        }

        private void UpdateOnValueChange()
        {
            this.WhenAnyValue(vm => vm.Name)
                .Subscribe(name => Entity.Name = name)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.LastNames)
                .Subscribe(lastNames => Entity.LastNames = lastNames)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.IdentificationNumber)
                .Subscribe(identificationNumber => Entity.IdentificationNumber = identificationNumber)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.Sex)
                .Subscribe(sex => Entity.Sex = sex ?? default)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.Number)
                .Subscribe(number => Entity.Number = number)
                .DisposeWith(_cleanup);

            var teamsObservable = Teams.AsObservableChangeSet().ToCollection();

            var teamObservable = this.WhenAnyValue(vm => vm.Team);

            teamObservable.CombineLatest(teamsObservable)
                .Subscribe(value =>
                {
                    var (team, teams) = value;

                    switch (team)
                    {
                        case null when teams.Count > 0:
                            Team = teams.FirstOrDefault(x => x.Id == Entity.TeamId);
                            return;
                        case null:
                            return;
                        default:
                            Entity.TeamId = team.Id;
                            break;
                    }
                }).DisposeWith(_cleanup);
        }

        private static DateTime GetBirthDate(string value)
        {
            var year = int.Parse(value.Substring(0, 2), CultureInfo.InvariantCulture);
            var month = int.Parse(value.Substring(2, 2), CultureInfo.InvariantCulture);
            var day = int.Parse(value.Substring(4, 2), CultureInfo.InvariantCulture);

            return new DateTime(year, month, day);
        }

        public static implicit operator Skater(SkaterProxy skaterProxy)
        {
            return skaterProxy.Entity;
        }

        public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}
