using MoviesG5.UI;

namespace MoviesG5.Tests.Fakes
{
    public class FakeDialogService : IDialogService
    {
        public List<string> ErrorMessages { get; } = new();
        public List<string> InfoMessages { get; } = new();

        public void ShowError(string message, string title) => ErrorMessages.Add(message);

        public void ShowInfo(string message, string title) => InfoMessages.Add(message);
    }
}
