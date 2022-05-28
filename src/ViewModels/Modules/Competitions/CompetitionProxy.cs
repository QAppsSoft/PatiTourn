﻿using System;
using DataModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace ViewModels.Modules.Competitions
{
    public class CompetitionProxy : ValidatableViewModelBase
    {
        public CompetitionProxy(Competition competition)
        {
            Competition = competition ?? throw new ArgumentNullException(nameof(competition));

            InitializeProperties(competition);

            InitializeValidations();

            UpdateSkaterOnValueChange();
        }

        public Competition Competition { get; }

        [Reactive] public string Name { get; set; } = null!;

        [Reactive] public string Category { get; set; } = null!;

        [Reactive] public DateTime StartDate { get; set; }

        [Reactive] public DateTime EndDate { get; set; }

        private void InitializeProperties(Competition competition)
        {
            Name = competition.Name;
            Category = competition.Category;
            StartDate = competition.StartDate;
            EndDate = competition.EndDate;
        }

        private void InitializeValidations()
        {
            // Name
            this.ValidationRule(viewModel => viewModel.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "You must specify a valid name");

            // Category
            this.ValidationRule(viewModel => viewModel.Category,
                category => !string.IsNullOrWhiteSpace(category),
                "You must specify a valid category");

            // Start date
            // End date
            var checkDateRangeObservable = this.WhenAnyValue(
                viewModel => viewModel.StartDate,
                viewModel => viewModel.EndDate,
                (startDate, endDate) => startDate < endDate);

            this.ValidationRule(
                viewModel => viewModel.EndDate,
                checkDateRangeObservable,
                "Competition end date must after start date");
        }

        private void UpdateSkaterOnValueChange()
        {
            this.WhenAnyValue(vm => vm.Name,
                    vm => vm.Category,
                    vm => vm.StartDate,
                    vm => vm.EndDate)
                .Subscribe(value =>
                {
                    var (name, category, startDate, endDate) = value;
                    Competition.Name = name;
                    Competition.Category = category;
                    Competition.StartDate = startDate;
                    Competition.EndDate = endDate;
                });
        }

        public static implicit operator Competition(CompetitionProxy competitionProxy)
        {
            return competitionProxy.Competition;
        }


        public static explicit operator CompetitionProxy(Competition competition)
        {
            return new CompetitionProxy(competition);
        }
    }
}
