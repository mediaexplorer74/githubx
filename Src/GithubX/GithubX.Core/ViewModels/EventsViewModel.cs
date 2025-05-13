﻿using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using GithubXamarin.Core.Contracts.Service;
using GithubXamarin.Core.Contracts.ViewModel;
using GithubXamarin.Core.Messages;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Octokit;
using Plugin.SecureStorage;

namespace GithubXamarin.Core.ViewModels
{
    public class EventsViewModel : BaseViewModel, IEventsViewModel
    {
        #region Properties and Commands

        private readonly IEventDataService _eventDataService;

        private ObservableCollection<Activity> _events;
        public ObservableCollection<Activity> Events
        {
            get => _events;
            set
            {
                _events = value;
                RaisePropertyChanged(() => Events);
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        private ICommand _eventClickCommand;
        public ICommand EventClickCommand
        {
            get
            {
                _eventClickCommand = _eventClickCommand ?? new MvxCommand<object>(NavigateToEventType);
                return _eventClickCommand;
            }
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                _refreshCommand = _refreshCommand ?? new MvxAsyncCommand(async () => await Refresh());
                return _refreshCommand;
            }
        }

        private long? _repositoryId;
        private string _userLogin;

        #endregion

        public EventsViewModel(IGithubClientService githubClientService, IEventDataService eventDataService, IMvxMessenger messenger, IDialogService dialogService) : base(githubClientService, messenger, dialogService)
        {
            _eventDataService = eventDataService;
        }

        public async void Init(long repositoryId, string userLogin = null)
        {
            _repositoryId = repositoryId == 0 ? (long?)null : repositoryId;
            _userLogin = userLogin;
            await Refresh();
        }

        private void NavigateToEventType(object obj)
        {
            var activity = obj as Activity ?? Events[SelectedIndex];
            if (activity == null) return;
            switch (activity.Type)
            {
                case "CommitCommentEvent":
                    break;
                case "CreateEvent":
                    ShowViewModel<RepositoryViewModel>(new { repositoryId = activity.Repo.Id });
                    break;
                case "DeleteEvent":
                    break;
                case "ForkEvent":
                    var forkEventPayload = activity.Payload as ForkEventPayload;
                    ShowViewModel<RepositoryViewModel>(new { repositoryId = forkEventPayload.Forkee.Id });
                    break;
                case "GollumEvent":
                    break;
                case "IssuesEvent":

                    var issueEventPayload = activity.Payload as IssueEventPayload;
                    try
                    {
                        if (issueEventPayload != null)
                        {
                            ShowViewModel<IssueViewModel>(
                                new
                                {
                                    issueNumber = issueEventPayload.Issue.Number,
                                    repositoryId = issueEventPayload.Repository.Id
                                });
                        }
                    }
                    catch (NullReferenceException)
                    {
                        ShowViewModel<RepositoryViewModel>(
                                new
                                {
                                    repositoryId = activity.Repo.Id
                                });
                    }
                    break;
                case "IssueCommentEvent":
                    break;
                case "LabelEvent":
                    break;
                case "MemberEvent":
                    break;
                case "ProjectCardEvent":
                    break;
                case "ProjectEvent":
                    break;
                case "PublicEvent":
                    break;
                case "PullRequestEvent":
                    break;
                case "PushEvent":
                    ShowViewModel<RepositoryViewModel>(new { repositoryId = activity.Repo.Id });
                    break;
                case "ReleaseEvent":
                    break;
                case "WatchEvent":
                    ShowViewModel<RepositoryViewModel>(new { repositoryId = activity.Repo.Id });
                    break;
            }
        }

        public override async void Start()
        {
            base.Start();

            //RnD
            //*****************
            if (!CrossSecureStorage.Current.HasKey("AskForStarring"))
            {
                CrossSecureStorage.Current.SetValue("AskForStarring", "asked");
                var result = await DialogService.ShowBooleanDialogAsync("", "Star GithubXamarin repository?");
                if (result)
                {
                    var starredClient = new StarredClient(new ApiConnection(GithubClientService.GetAuthorizedGithubClient().Connection));
                    await starredClient.StarRepo("mediaexplorer74", "GitX");
                }
            }
            //*****************
            
        }

        public async Task Refresh()
        {
            if (!(await IsInternetAvailable()))
            {
                await DialogService.ShowSimpleDialogAsync("Use this moment to look up from your screen and enjoy life.", 
                    "No Internet Connection!");
                return;
            }

            Messenger.Publish(new LoadingStatusMessage(this) { IsLoadingIndicatorActive = true });
            try
            {
                if (_repositoryId.HasValue)
                {
                    Events = await _eventDataService.GetAllEventsOfRepository(_repositoryId.Value,
                        GithubClientService.GetAuthorizedGithubClient());

                    Messenger.Publish(new AppBarHeaderChangeMessage(this)
                    {
                        HeaderTitle = $"Events for {Events[0]?.Repo.FullName}"
                    });
                }
                else if (!string.IsNullOrWhiteSpace(_userLogin))
                {
                    Messenger.Publish(new AppBarHeaderChangeMessage(this)
                    {
                        HeaderTitle = $"Public Events for {_userLogin}"
                    });
                    Events = await _eventDataService.GetAllPublicEventsForUser(_userLogin,
                        GithubClientService.GetAuthorizedGithubClient());
                }
                else
                {
                    Messenger.Publish(new AppBarHeaderChangeMessage(this) { HeaderTitle = "Your Events" });
                    Events =
                        await _eventDataService.GetAllEventsForCurrentUser(
                            GithubClientService.GetAuthorizedGithubClient());
                }
            }
            catch (HttpRequestException)
            {
                await DialogService.ShowSimpleDialogAsync("The internet seems to be working but the code threw an HttpRequestException. Try again.", "Hmm, this is weird!");
            }

            Messenger.Publish(new LoadingStatusMessage(this) { IsLoadingIndicatorActive = false });
        }
    }
}
