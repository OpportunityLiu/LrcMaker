using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace Opportunity.LrcSearcher
{
    public sealed class AuroraExtensionBackgroundTask : IBackgroundTask
    {
        private IBackgroundTaskInstance taskInstance;
        private BackgroundTaskDeferral taskDeferral;
        private AppServiceConnection appServiceConnection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.taskInstance = taskInstance;
            this.taskDeferral = taskInstance.GetDeferral();

            var appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            this.appServiceConnection = appService.AppServiceConnection;
            this.taskInstance.Canceled += OnBackgroundTaskCanceled;
            this.appServiceConnection.RequestReceived += OnAppServiceRequestReceived;
            this.appServiceConnection.ServiceClosed += OnServiceClosed;
        }

        private void OnBackgroundTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Close();
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Close();
        }

        private static ValueSet failedData = new ValueSet { ["status"] = 0 };

        private async void OnAppServiceRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();
            try
            {
                var message = args.Request.Message;
                if (!message.TryGetValue("q", out var query) || query.ToString() != "lyric")
                {
                    await args.Request.SendResponseAsync(failedData);
                    return;
                }
                message.TryGetValue("title", out var t);
                message.TryGetValue("artist", out var a);
                var title = (t ?? "").ToString();
                var artist = (a ?? "").ToString();

                foreach (var searcher in Searchers.All)
                {
                    var r = await searcher.FetchLrcListAsync(artist, title);
                    var lrc1 = r.FirstOrDefault();
                    if (lrc1 is null)
                    {
                        continue;
                    }
                    var returnData = new ValueSet { ["status"] = 1, ["result"] = await Helper.HttpClient.GetStringAsync(lrc1.Uri) };
                    await args.Request.SendResponseAsync(returnData);
                }

                // Not found in all providers.
                await args.Request.SendResponseAsync(failedData);
            }
            catch (Exception)
            {
                await args.Request.SendResponseAsync(failedData);
            }
            finally
            {
                messageDeferral.Complete();
            }
        }

        private void Close()
        {
            if (this.taskDeferral is null)
                return;
            this.taskDeferral.Complete();
            this.taskDeferral = null;
            this.appServiceConnection.RequestReceived -= OnAppServiceRequestReceived;
            this.appServiceConnection.ServiceClosed -= OnServiceClosed;
            this.taskInstance.Canceled -= OnBackgroundTaskCanceled;
        }
    }
}
