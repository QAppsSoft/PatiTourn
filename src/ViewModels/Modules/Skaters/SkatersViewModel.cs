using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Common;
using DataModel;
using Domain.Services.Interfaces;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace ViewModels.Modules.Skaters
{
    public class SkatersViewModel : ValidatableViewModelBase, IDisposable
    {
        private readonly IDisposable _cleanup;

        public SkatersViewModel(IEntityService<Skater> skatersService, ISchedulerProvider schedulerProvider, Competition competition)
        {
            var transform = skatersService.List.Connect()
                .Transform(skater => new SkaterProxy(skater))
                .Publish();

            var skatersListDisposable = transform
                .Sort(SortExpressionComparer<SkaterProxy>.Ascending(x => x.Team.Id).ThenByAscending(x => x.Number))
                .ObserveOn(schedulerProvider.Dispatcher)
                .Bind(out var skaters)
                .Subscribe();

            Skaters = skaters;

            var selectLast = transform.ActOnEveryObject(skaterProxy => SelectedSkaterProxy = skaterProxy, _ => { });

            var anySelected = this.WhenAnyValue(vm => vm.SelectedSkaterProxy)
                .Select(skaterProxy => skaterProxy != null)
                .Publish();

            Delete = ReactiveCommand.Create<Skater>(skatersService.Remove, anySelected);

            Refresh = ReactiveCommand.Create(() => skatersService.Refresh(skater => skater.CompetitionId == competition.Id));

            AddNew = ReactiveCommand.Create(() =>
            {
                var skater = skatersService.Create();
                skater.Competition = competition;

                skatersService.Add(skater);
            });

            var allValid = transform.AutoRefreshOnObservable(skaterProxy => skaterProxy.IsValid())
                .Filter(skaterProxy => !skaterProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            Save = ReactiveCommand.CreateFromTask(skatersService.SaveAsync, allValid);

            this.ValidationRule(viewModel => viewModel.Skaters, allValid, "A Skater info is in an invalid state");

            _cleanup = new CompositeDisposable(skatersListDisposable, selectLast, transform.Connect(), allValid.Connect(), anySelected.Connect());
        }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReadOnlyObservableCollection<SkaterProxy> Skaters { get; }

        [Reactive] public SkaterProxy? SelectedSkaterProxy { get; set; }

        public ReactiveCommand<Skater, Unit> Delete { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<Unit, int> Save { get; }

        public void Dispose()
        {
            _cleanup.Dispose();
            AddNew.Dispose();
            Delete.Dispose();
            Refresh.Dispose();
        }
    }
}
