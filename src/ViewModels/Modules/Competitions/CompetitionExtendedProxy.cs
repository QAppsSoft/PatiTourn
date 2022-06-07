using System;
using DataModel;
using ViewModels.Modules.Skaters;
using ViewModels.Modules.Teams;

namespace ViewModels.Modules.Competitions
{
    public class CompetitionExtendedProxy
    {
        public Competition Competition { get; }
        public TeamsViewModel TeamsViewModel { get; }
        public SkatersViewModel SkatersViewModel { get; }

        public CompetitionExtendedProxy(TeamsViewModel teamsViewModel, SkatersViewModel skatersViewModel, Competition competition)
        {
            ArgumentNullException.ThrowIfNull(teamsViewModel);
            ArgumentNullException.ThrowIfNull(skatersViewModel);
            ArgumentNullException.ThrowIfNull(competition);

            Competition = competition;
            TeamsViewModel = teamsViewModel;
            SkatersViewModel = skatersViewModel;

            Name = competition.Name;
            Category = competition.Category;
            StartDate = competition.StartDate;
            EndDate = competition.EndDate;
        }

        public string Name { get; }

        public string Category { get; }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

        public static implicit operator Competition(CompetitionExtendedProxy competitionProxy)
        {
            return competitionProxy.Competition;
        }
    }
}
