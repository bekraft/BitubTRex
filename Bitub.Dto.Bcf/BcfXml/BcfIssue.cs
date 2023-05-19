using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bitub.Dto.BcfXml
{
    public class BcfIssue
    {
        public static string DefaultBcfvFileNamePerIDPattern = "Viewpoint_{0}";
        public static string DefaultSnapshotFileNamePerIDPattern = "Snapshot_{0}";
        public static BcfBitmapFormat DefaultSnapshotFormat = BcfBitmapFormat.PNG;

        public (BcfViewpoint, BcfVisualizationInfo)[] Viewpoints { get; private set; }

        public System.Guid ID { get; private set; }
        public BcfMarkup Markup { get; private set; }

#if NETFRAMEWORK
        public Image GetSnapshotImage(int index) => Image.FromStream(GetSnapshotBinary(index));

        public Image GetSnapshotImage(BcfViewpoint viewpoint) => Image.FromStream(GetSnapshotBinary(viewpoint));
#endif

        public Stream GetSnapshotBinary(int index) => FileAccessor(Viewpoints[index].Item1.Snapshot);

        public Stream GetSnapshotBinary(BcfViewpoint viewpoint) => FileAccessor(viewpoint.Snapshot);

        public Stream FetchResource(string name) => FileAccessor(name);

        #region Internals
        internal readonly Func<string, Stream> FileAccessor;
        internal readonly Func<Regex, string[]> FileFilter;

        internal BcfIssue(Func<string, Stream> fileAccessor, Func<Regex, string[]> fileFilter)
        {
            FileAccessor = fileAccessor;
            FileFilter = fileFilter;
            Init();
        }

        #endregion

        public static BcfIssue FromPath(string path)
        {
            return new BcfIssue(
                (fileName) => new FileStream(Path.Combine(path, fileName), FileMode.OpenOrCreate), 
                (regEx) => Directory.GetFiles(path).Where(p => regEx.IsMatch(p)).ToArray()
            );
        }

        private void Init()
        {
            Markup = BcfFile.Deserialize<BcfMarkup>(FileAccessor("markup.bcf"));
            if (null == Markup)
                throw new NotSupportedException($"Non BXFXML conformant: Required markup.bcf missing.");

            var extractedViewpoints = Markup.Viewpoints?
                .Select(v => (v, BcfFile.Deserialize<BcfVisualizationInfo>(FileAccessor(v.Reference))))
                .ToArray();

            Viewpoints = extractedViewpoints ?? new (BcfViewpoint v, BcfVisualizationInfo)[0];

            var topic = Markup.Topic;            
            if (null == topic)
                throw new NotSupportedException($"Empty Markup or Topic of BCF topic not supported.");
            ID = topic.ID;
        }

        public IEnumerable<(string, BcfVisualizationInfo)> GetVisualisationInfos()
        {
            return FileFilter(new Regex(@"(\.bcfv)$"))
                .Select(f => (f, BcfFile.Deserialize<BcfVisualizationInfo>(FileAccessor(f))))
                .ToArray();
        }
    }
}
