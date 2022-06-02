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
            Competition = competition ?? throw new ArgumentNullException(nameof(competition));
            TeamsViewModel = teamsViewModel ?? throw new ArgumentNullException(nameof(teamsViewModel));
            SkatersViewModel = skatersViewModel ?? throw new ArgumentNullException(nameof(skatersViewModel));

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
