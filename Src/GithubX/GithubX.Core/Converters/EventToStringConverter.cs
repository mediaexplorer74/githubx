using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using Octokit;

namespace GithubXamarin.Core.Converters
{
    /// <summary>
    /// Takes an event from the binding and return proper description for it.
    /// </summary>
    public class EventToStringConverter : MvxValueConverter<Activity,string>
    {
        protected override string Convert(Activity value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            //https://developer.github.com/v3/activity/events/types/
            string eventString = null;
            switch (value.Type)
            {
                case "CommitCommentEvent":
                    eventString = $"commented on a commit in";
                    break;
                case "CreateEvent":
                    eventString = $"created";
                    break;
                case "DeleteEvent":
                    eventString = $"deleted";
                    break;
                case "ForkEvent":
                    eventString = $"forked";
                    break;
                case "GollumEvent":
                    eventString = "created/edited a wiki page in";
                    break;
                case "IssuesEvent":
                    eventString = $"edited an issue in";
                    break;
                case "IssueCommentEvent":
                    eventString = $"commented on #{(value.Payload as IssueCommentPayload).Issue.Number} in";
                    break;
                case "LabelEvent":
                    eventString = $"created/edited a label in";
                    break;
                case "MemberEvent":
                    eventString = $"added collaborator to";
                    break;
                case "ProjectCardEvent":
                    eventString = $"created a project card in";
                    break;
                case "ProjectEvent":
                    eventString = $"created project in";
                    break;
                case "PublicEvent":
                    eventString = $"has open-sourced";
                    break;
                case "PullRequestEvent":
                    var pullRequestEventPayload = value.Payload as PullRequestEventPayload;
                    eventString = $"{pullRequestEventPayload.Action} pull request no. {pullRequestEventPayload.Number} in";
                    break;
                case "PushEvent":
                    eventString = $"pushed to";
                    break;
                case "ReleaseEvent":
                    eventString = $"published a release in";
                    break;
                case "WatchEvent":
                    eventString = $"starred";
                    break;
                default:
                    eventString = "did something on";
                    return string.Empty;
            }
            
            return $"{value.Actor.Login} {eventString} {value.Repo.Name}";
        }
    }
}
