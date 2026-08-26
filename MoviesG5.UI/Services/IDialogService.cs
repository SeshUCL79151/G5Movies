namespace MoviesG5.UI
{
    public interface IDialogService
    {
        void ShowError(string message, string title);
        void ShowInfo(string message, string title);
    }
}
