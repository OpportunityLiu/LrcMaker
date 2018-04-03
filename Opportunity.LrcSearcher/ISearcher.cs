using System.Collections.Generic;
using Windows.Foundation;
using Windows.Web.Http.Headers;

namespace Opportunity.LrcSearcher
{
    public interface ISearcher
    {
        IAsyncOperation<IEnumerable<LrcInfo>> FetchLrcListAsync(string artist, string title);
    }

    public static class Searchers
    {
        public static ISearcher NeteaseSearcher { get; } = new NeteaseSearcher();
        public static ISearcher ViewLyricsSearcher { get; } = new ViewLyricsSearcher();
        public static ISearcher TTSearcher { get; } = new TTSearcher();

        public static IEnumerable<ISearcher> All
        {
            get
            {
                yield return NeteaseSearcher;
                yield return ViewLyricsSearcher;
                yield return TTSearcher;
            }
        }
    }
}