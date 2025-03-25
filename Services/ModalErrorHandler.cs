namespace Food_maui.Services
{
    /// <summary>
    /// Modal Error Handler.
    /// </summary>
    /// 
    public class ModalErrorHandler : IErrorHandler
    {
        SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Handle error in UI.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public void HandleError(Exception ex)
        {
            DisplayAlert(ex).FireAndForgetSafeAsync();
        }

        async Task DisplayAlert(Exception ex)
        {
            try
            {
                await _semaphore.WaitAsync();
                if (Shell.Current is Shell shell)
                    await shell.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void EnterSearch(string query)
        {
            PerformSearch(query).FireAndForgetSafeAsync();
        }

        async Task PerformSearch(string query)
        {
            try
            {
                await _semaphore.WaitAsync();
                // Implement search logic here
                // Example: await SearchService.SearchAsync(query);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void OnNewButtonPressed()
        {
            ChangeButtonAppearance().FireAndForgetSafeAsync();
        }

        async Task ChangeButtonAppearance()
        {
            try
            {
                await _semaphore.WaitAsync();
                // Implement logic to change border or color of the button
                // Example: Button button = GetButtonByName("New (num)");
                // button.BorderColor = Color.Red;
                // button.BackgroundColor = Color.Blue;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public static class TaskExtensions
    {
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
            }
        }
    }
}