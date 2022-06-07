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
using ViewModels.Interfaces;

namespace ViewModels.Modules.Competitions
{
    public class CompetitionsViewModel : ValidatableViewModelBase, IEntitiesProxyContainer<CompetitionProxy>, IDisposable
    {
        private readonly IEntityService<Competition> _competitionService;
        private readonly IDisposable _cleanup;

        public CompetitionsViewModel(IEntityService<Competition> competitionService, ISchedulerProvider schedulerProvider)
        {
            ArgumentNullException.ThrowIfNull(competitionService);
            ArgumentNullException.ThrowIfNull(schedulerProvider);

            _competitionService = competitionService;

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

            ProxyItems = competitions;

            Delete = ReactiveCommand.CreateFromTask<CompetitionProxy>(DeleteCompetitionAsync);

            Refresh = ReactiveCommand.CreateFromTask(competitionService.RefreshAsync);

            AddNew = ReactiveCommand.CreateFromTask(AddCompetitionAsync);

            Edit = ReactiveCommand.CreateFromTask<CompetitionProxy>(EditCompetitionAsync);

            var allValid = transform.AutoRefreshOnObservable(teamProxy => teamProxy.IsValid())
                .Filter(competitionProxy => !competitionProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            this.ValidationRule(viewModel => viewModel.ProxyItems, allValid, "A Competition info is in an invalid state");

            _cleanup = new CompositeDisposable(competitionsListDisposable, transform.Connect(), allValid.Connect());
        }

        private async Task AddCompetitionAsync()
        {
            var competition = _competitionService.Create();

            var result = await AddDialog.Handle(new CompetitionProxy(competition));

            if (result)
            {
                await _competitionService.AddAsync(competition).ConfigureAwait(false);
            }
        }

        public Interaction<CompetitionProxy, bool> ConfirmDeleteDialog { get; }

        public Interaction<CompetitionProxy, Unit> EditDialog { get; }

        public Interaction<CompetitionProxy, bool> AddDialog { get; }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<CompetitionProxy, Unit> Delete { get; }

        public ReactiveCommand<CompetitionProxy, Unit> Edit { get; }

        [Reactive] public CompetitionProxy? SelectedProxy { get; set; }

        public ReadOnlyObservableCollection<CompetitionProxy> ProxyItems { get; }

        [Reactive] public string Filter { get; set; } = string.Empty;

        private async Task DeleteCompetitionAsync(CompetitionProxy competition)
        {
            var canDelete = await ConfirmDeleteDialog.Handle(competition);

            if (canDelete)
            {
                await _competitionService.RemoveAsync(competition).ConfigureAwait(false);
            }
        }
        
        private async Task EditCompetitionAsync(CompetitionProxy competition)
        {
            await EditDialog.Handle(competition);
            await _competitionService.EditAsync(competition, UpdateDataBaseEntityProperties).ConfigureAwait(false);
        }

        private static void UpdateDataBaseEntityProperties(UpdateEntityContainer<Competition> container)
        {
            var (editedEntity, databaseEntity) = container;
            databaseEntity.Name = editedEntity.Name;
            databaseEntity.Category = editedEntity.Category;
            databaseEntity.StartDate = editedEntity.StartDate;
            databaseEntity.EndDate = editedEntity.EndDate;
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
