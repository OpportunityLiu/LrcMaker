using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Audio;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Opportunity.LrcMaker
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AudioGraph audioGraph;
        private AudioFrameOutputNode outNode;
        private AudioFileInputNode fileNode;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var mediaSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Test/GirlishLover.mp3"));
            await mediaSource.OpenAsync();
            this.mpe.Source = mediaSource;
            this.mpe.MediaPlayer.MediaOpened += this.MediaPlayer_MediaOpened;


            var settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Other)
            {
                QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency
            };
            var result = await AudioGraph.CreateAsync(settings);
            this.audioGraph = result.Graph;

            this.outNode = this.audioGraph.CreateFrameOutputNode();

            this.fileNode = (await this.audioGraph.CreateFileInputNodeAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Test/GirlishLover.mp3")))).FileInputNode;
            this.fileNode.LoopCount = 0;
            this.fileNode.AddOutgoingConnection(this.outNode);
            this.fileNode.FileCompleted += this.FileNode_FileCompleted;
            this.audioGraph.QuantumStarted += this.AudioGraph_QuantumStarted;

            this.audioGraph.Start();
        }

        private async void FileNode_FileCompleted(AudioFileInputNode sender, object args)
        {
            this.audioGraph.Stop();
            await this.img.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var img = new WriteableBitmap(this.data.Count, 100);
                using (var data = img.PixelBuffer.AsStream())
                {
                    for (var j = 0; j < img.PixelHeight; j++)
                    {
                        for (var i = 0; i < this.data.Count; i++)
                        {
                            var (a, _, _) = this.data[i];
                            if (a * 100 < 100 - j)
                            {
                                data.WriteByte(255);
                                data.WriteByte(255);
                                data.WriteByte(255);
                                data.WriteByte(255);
                            }
                            else
                            {
                                data.WriteByte(0);
                                data.WriteByte(0);
                                data.WriteByte(0);
                                data.WriteByte(255);
                            }
                        }
                    }
                }
                img.Invalidate();
                this.img.Source = img;

                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Start();
                timer.Tick += this.Timer_Tick;
            });
        }

        private void Timer_Tick(object sender, object e)
        {
            var ps = this.mpe.MediaPlayer.PlaybackSession;
            this.svImg.ChangeView(ps.Position.TotalMilliseconds / ps.NaturalDuration.TotalMilliseconds * this.svImg.ExtentWidth, null, null, true);
        }

        private List<(float avg, float comm, float diff)> data = new List<(float avg, float comm, float diff)>();

        TimeSpan lastPos = TimeSpan.MinValue;

        private unsafe void AudioGraph_QuantumStarted(AudioGraph sender, object args)
        {
            using (var frame = this.outNode.GetFrame())
            using (var buffer = frame.LockBuffer(Windows.Media.AudioBufferAccessMode.Write))
            using (var reference = buffer.CreateReference())
            {
                if (buffer.Length == 0)
                    return;
                if (this.fileNode.Position == lastPos)
                    return;
                this.lastPos = this.fileNode.Position;
                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess)reference).GetBuffer(out var dataInBytes, out var capacityInBytes);

                var data = (float*)dataInBytes;
                var capacity = capacityInBytes * sizeof(byte) / sizeof(float);
                var channelCount = capacity / sender.SamplesPerQuantum;

                var begin1 = data;
                var end1 = begin1 + capacity / channelCount;
                var begin2 = end1;
                var end2 = begin2 + capacity / channelCount;

                var sum = 0f;
                var commSum = 0f;
                var diffSum = 0f;

                for (float* p1 = begin1, p2 = begin2; p1 < end1 && p2 < end2; p1++, p2++)
                {
                    sum += *p1 * *p1 + *p2 * *p2;
                    var comm = (*p1 + *p2) / 2;
                    commSum += comm * comm;
                    var diff = (*p1 - *p2) / 2;
                    diffSum += diff * diff;
                }

                this.data.Add((MathF.Sqrt(sum / sender.SamplesPerQuantum * 2), MathF.Sqrt(commSum / sender.SamplesPerQuantum), MathF.Sqrt(diffSum / sender.SamplesPerQuantum)));
            }
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
        }
    }

    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }
}
