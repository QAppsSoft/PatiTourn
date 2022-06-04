using System;
using System.Globalization;
using System.Reactive.Linq;
using DataModel;
using DataModel.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ViewModels.Extensions;

namespace ViewModels.Modules.Skaters
{
    public sealed class SkaterProxy : ValidatableViewModelBase, IEntityProxy<Skater>
    {
        public SkaterProxy(Skater skater)
        {
            Entity = skater ?? throw new ArgumentNullException(nameof(skater));

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

        [Reactive] public Team Team { get; set; } = null!;

        [ObservableAsProperty] public DateTime BirthDate { get; }

        [ObservableAsProperty] public int Age { get; }

        private void InitializeProperties(Skater skater)
        {
            Number = skater.Number;
            Name = skater.Name;
            LastNames = skater.LastNames;
            IdentificationNumber = skater.IdentificationNumber;
            Sex = skater.Sex;
            Team = skater.Team;
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
            this.WhenAnyValue(vm => vm.Name,
                    vm => vm.LastNames,
                    vm => vm.IdentificationNumber,
                    vm => vm.Sex,
                    vm => vm.Number,
                    vm => vm.Team)
                .Subscribe(value =>
                {
                    var (name, lastname, identification, sex, number, team) = value;
                    Entity.Name = name;
                    Entity.LastNames = lastname;
                    Entity.IdentificationNumber = identification;
                    Entity.Sex = sex ?? DataModel.Enums.Sex.Male;
                    Entity.Number = number;
                    Entity.Team = team;
                });
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


        public static explicit operator SkaterProxy(Skater skater)
        {
            return new SkaterProxy(skater);
        }
    }
}
