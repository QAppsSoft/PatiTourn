using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common;
using DataModel;
using Domain.Services.Interfaces;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace ViewModels.Modules.Competitions
{
    public class CompetitionsViewModel : ValidatableViewModelBase, IDisposable
    {
        private readonly IEntityService<Competition> _competitionService;
        private readonly IDisposable _cleanup;

        private Action<string> _applyFilter = _ => { };

        public CompetitionsViewModel(IEntityService<Competition> competitionService, ISchedulerProvider schedulerProvider)
        {
            _competitionService = competitionService ?? throw new ArgumentNullException(nameof(competitionService));

            ConfirmDeleteDialog = new Interaction<CompetitionProxy, bool>(schedulerProvider.Dispatcher);

            EditDialog = new Interaction<CompetitionProxy, Unit>(schedulerProvider.Dispatcher);

            AddDialog = new Interaction<CompetitionProxy, bool>(schedulerProvider.Dispatcher);

            var transform = competitionService.List.Connect()
                .Transform(competition => new CompetitionProxy(competition))
                .Publish();

            var filterBuilder = this.WhenAnyValue(x => x.Filter)
                .Throttle(TimeSpan.FromMilliseconds(500), schedulerProvider.TaskPool)
                .StartWith(string.Empty)
                .Select(BuildFilter);

            var competitionsListDisposable = transform
                .Filter(filterBuilder)
                .Sort(SortExpressionComparer<CompetitionProxy>.Ascending(proxy => proxy.Name))
                .ObserveOn(schedulerProvider.Dispatcher)
                .Bind(out var competitions)
                .Subscribe();

            Competitions = competitions;

            Delete = ReactiveCommand.CreateFromTask<CompetitionProxy>(DeleteCompetitionAsync);

            Refresh = ReactiveCommand.Create(competitionService.Refresh);

            AddNew = ReactiveCommand.CreateFromTask(AddCompetitionAsync);

            Edit = ReactiveCommand.CreateFromTask<CompetitionProxy>(EditCompetitionAsync);

            var allValid = transform.AutoRefreshOnObservable(teamProxy => teamProxy.IsValid())
                .Filter(competitionProxy => !competitionProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            Save = ReactiveCommand.CreateFromTask(competitionService.SaveAsync, allValid);

            this.ValidationRule(viewModel => viewModel.Competitions, allValid, "A Competition info is in an invalid state");

            _cleanup = new CompositeDisposable(competitionsListDisposable, transform.Connect(), allValid.Connect());
        }

        private async Task AddCompetitionAsync()
        {
            var competition = _competitionService.Create();

            var result = await AddDialog.Handle(new CompetitionProxy(competition));

            if (result)
            {
                _competitionService.Add(competition);
            }
        }

        public Interaction<CompetitionProxy, bool> ConfirmDeleteDialog { get; }

        public Interaction<CompetitionProxy, Unit> EditDialog { get; }

        public Interaction<CompetitionProxy, bool> AddDialog { get; }

        public ReactiveCommand<Unit, int> Save { get; }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<CompetitionProxy, Unit> Delete { get; }

        public ReactiveCommand<CompetitionProxy, Unit> Edit { get; }

        [Reactive] public CompetitionProxy? SelectedCompetitionProxy { get; set; }

        public ReadOnlyObservableCollection<CompetitionProxy> Competitions { get; }

        [Reactive] public string Filter { get; set; } = string.Empty;

        public void ApplyFilter(string filter)
        {
            _applyFilter(filter);
        }

        private async Task DeleteCompetitionAsync(CompetitionProxy competition)
        {
            var canDelete = await ConfirmDeleteDialog.Handle(competition);

            if (canDelete)
            {
                _competitionService.Remove(competition);
                await _competitionService.SaveAsync().ConfigureAwait(false);
            }
        }
        
        private async Task EditCompetitionAsync(CompetitionProxy competition)
        {
            await EditDialog.Handle(competition);
            await _competitionService.SaveAsync().ConfigureAwait(false);
        }

        private static Func<CompetitionProxy, bool> BuildFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return _ => true;
            }

            var words = filter.Split(' ');

            return competition =>
                words.All(word => competition.Name
                    .Contains(word, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}
