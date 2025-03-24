using CommunityToolkit.Mvvm.Input;
using Food_maui.Models;

namespace Food_maui.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}