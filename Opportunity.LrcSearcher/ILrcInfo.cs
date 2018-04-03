using Windows.Foundation;

namespace Opportunity.LrcSearcher
{
    public interface ILrcInfo
    {
        string Album { get; }
        string Artist { get; }
        string Title { get; }

        IAsyncOperation<string> FetchLryics();
    }
}