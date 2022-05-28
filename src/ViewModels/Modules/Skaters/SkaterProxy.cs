using System;
using System.Reactive.Linq;
using DataModel;
using DataModel.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ViewModels.Extensions;

namespace ViewModels.Modules.Skaters
{
    public class SkaterProxy : ValidatableViewModelBase
    {
        public SkaterProxy(Skater skater)
        {
            Skater = skater ?? throw new ArgumentNullException(nameof(skater));

            InitializeProperties(skater);

            InitializeValidations();

            UpdateSkaterOnValueChange();

            this.WhenAnyValue(vm => vm.IdentificationNumber)
                .Where(value => value.Length == 11)
                .Select(GetBirthDate)
                .ToPropertyEx(this, vm => vm.BirthDate);

            this.WhenAnyValue(vm => vm.BirthDate)
                .Select(birthDate => birthDate.Age())
                .ToPropertyEx(this, vm => vm.Age);
        }

        public Skater Skater { get; }

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
                name => name!.Contains(" "),
                "You must specify both lastname");

            // Identification Number
            // TODO: Use a more generic Identification Number validation rules according to others nationalities
            this.ValidationRule(viewModel => viewModel.IdentificationNumber,
                identificationNumber => identificationNumber!.Length == 11,
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

        private void UpdateSkaterOnValueChange()
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
                    Skater.Name = name;
                    Skater.LastNames = lastname;
                    Skater.IdentificationNumber = identification;
                    Skater.Sex = sex ?? DataModel.Enums.Sex.Male;
                    Skater.Number = number;
                    Skater.Team = team;
                });
        }

        private static DateTime GetBirthDate(string value)
        {
            var year = int.Parse(value.Substring(0, 2));
            var month = int.Parse(value.Substring(2, 2));
            var day = int.Parse(value.Substring(4, 2));

            return new DateTime(year, month, day);
        }

        public static implicit operator Skater(SkaterProxy skaterProxy)
        {
            return skaterProxy.Skater;
        }


        public static explicit operator SkaterProxy(Skater skater)
        {
            return new SkaterProxy(skater);
        }
    }
}
