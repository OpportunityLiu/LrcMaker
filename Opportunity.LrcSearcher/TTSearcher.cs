using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Web.Http;

namespace Opportunity.LrcSearcher
{
    internal sealed class TTSearcher : ISearcher
    {
        private static string reform(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";
            return new string(data.Where(c => !char.IsWhiteSpace(c) && c != '\'' && c != '"').Select(char.ToLower).ToArray());
        }

        public IAsyncOperation<IEnumerable<LrcInfo>> FetchLrcListAsync(string artist, string title)
        {
            var artistHex = asHexString(reform(artist), Encoding.Unicode);
            var titleHex = asHexString(reform(title), Encoding.Unicode);

            var resultUrl = string.Format(SearchPath, artistHex, titleHex);

            return AsyncInfo.Run(async token =>
            {
                var xml = await httpClient.GetStringAsync(new Uri(resultUrl));
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var nodelist = doc.SelectNodes("/result/lrc");
                var lrclist = new LrcInfo[nodelist.Count];
                for (var i = 0; i < lrclist.Length; i++)
                {
                    var element = (XmlElement)nodelist[i];
                    var artistItem = element.GetAttribute("artist");
                    var titleItem = element.GetAttribute("title");
                    var idItem = element.GetAttribute("id");
                    var uri = new Uri(string.Format(DownloadPath, idItem, getQianQianCode(int.Parse(idItem), titleItem, artistItem)));
                    lrclist[i] = new LrcInfo(titleItem, artistItem, "", uri);
                }
                return (IEnumerable<LrcInfo>)lrclist;
            });
        }
        //歌词Id获取地址
        private static readonly string SearchPath = "http://lrcct2.ttplayer.com/dll/lyricsvr.dll?sh?Artist={0}&Title={1}&Flags=0";

        private static HttpClient httpClient = new HttpClient();

        //根据artist和title获取歌词信息
        public static LrcInfo[] GetLrcList(string artist, string title, string filepath)
        {
            string artistHex = asHexString(artist, Encoding.Unicode);
            string titleHex = asHexString(title, Encoding.Unicode);

            string resultUrl = string.Format(SearchPath, artistHex, titleHex);

            var doc = new XmlDocument();
            try
            {
                doc.Load(resultUrl);

                XmlNodeList nodelist = doc.SelectNodes("/result/lrc");
                List<LrcInfo> lrclist = new List<LrcInfo>();
                foreach (XmlNode node in nodelist)
                {
                    var element = (XmlElement)node;
                    var artistItem = element.GetAttribute("artist");
                    var titleItem = element.GetAttribute("title");
                    var idItem = element.GetAttribute("id");
                    lrclist.Add(new LrcInfo(titleItem, artistItem, "", new Uri(string.Format(DownloadPath, idItem, getQianQianCode(int.Parse(idItem), titleItem, artistItem)))));
                }
                return lrclist.ToArray();
            }
            catch (XmlException)
            {
                return null;
            }
        }

        //把字符串转换为十六进制
        private static string asHexString(string str, Encoding encoding)
        {
            var bytes = encoding.GetBytes(str);
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        //歌词下载地址
        private static readonly string DownloadPath = "http://lrcct2.ttplayer.com/dll/lyricsvr.dll?dl?Id={0}&Code={1}";
        private static string getQianQianCode(int lrcId, string title, string artist)
        {
            string qqHexStr = asHexString(artist + title, Encoding.UTF8);
            int length = qqHexStr.Length / 2;
            int[] song = new int[length];
            for (int i = 0; i < length; i++)
            {
                song[i] = int.Parse(qqHexStr.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            int t1 = 0, t2 = 0, t3 = 0;
            t1 = (lrcId & 0x0000FF00) >> 8;
            if ((lrcId & 0x00FF0000) == 0)
            {
                t3 = 0x000000FF & ~t1;
            }
            else
            {
                t3 = 0x000000FF & ((lrcId & 0x00FF0000) >> 16);
            }

            t3 = t3 | ((0x000000FF & lrcId) << 8);
            t3 = t3 << 8;
            t3 = t3 | (0x000000FF & t1);
            t3 = t3 << 8;
            if ((lrcId & 0xFF000000) == 0)
            {
                t3 = t3 | (0x000000FF & (~lrcId));
            }
            else
            {
                t3 = t3 | (0x000000FF & (lrcId >> 24));
            }

            int j = length - 1;
            while (j >= 0)
            {
                int c = song[j];
                if (c >= 0x80) c = c - 0x100;

                t1 = (int)((c + t2) & 0x00000000FFFFFFFF);
                t2 = (int)((t2 << (j % 2 + 4)) & 0x00000000FFFFFFFF);
                t2 = (int)((t1 + t2) & 0x00000000FFFFFFFF);
                j -= 1;
            }
            j = 0;
            t1 = 0;
            while (j <= length - 1)
            {
                int c = song[j];
                if (c >= 128) c = c - 256;
                int t4 = (int)((c + t1) & 0x00000000FFFFFFFF);
                t1 = (int)((t1 << (j % 2 + 3)) & 0x00000000FFFFFFFF);
                t1 = (int)((t1 + t4) & 0x00000000FFFFFFFF);
                j += 1;
            }

            int t5 = (int)conv(t2 ^ t3);
            t5 = (int)conv(t5 + (t1 | lrcId));
            t5 = (int)conv(t5 * (t1 | t3));
            t5 = (int)conv(t5 * (t2 ^ lrcId));

            long t6 = (long)t5;
            if (t6 > 2147483648)
                t5 = (int)(t6 - 4294967296);
            return t5.ToString();

            long conv(int i)
            {
                long r = i % 4294967296;
                if (i >= 0 && r > 2147483648)
                    r = r - 4294967296;

                if (i < 0 && r < 2147483648)
                    r = r + 4294967296;
                return r;
            }
        }
    }
}