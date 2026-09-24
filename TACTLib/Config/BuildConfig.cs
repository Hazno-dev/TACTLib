using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TACTLib.Config {
    public class BuildConfig : Config {
        public FileRecord Root;
        public FileRecord? Install;
        public FileRecord? Patch;
        public FileRecord? Download;
        public FileRecord Encoding;
        public SizeRecord? EncodingSize;
        public FileRecord? VFSRoot;
        public SizeRecord? VFSRootSize;
        public FileRecord[]? VFSManifests;
        public SizeRecord[]? VFSManifestsSize;

        public string GetBuildName() => Values["build-name"][0];

        public BuildConfig(Stream? stream) : base(stream) {
            if (!GetRecord<FileRecord>("root", out var root)) {
                throw new NullReferenceException(nameof(root));
            }

            GetRecord("install", out Install);
            GetRecord("patch", out Patch);
            GetRecord("download", out Download);
            GetRecord("encoding", out Encoding);
            /*if (!GetRecord<FileRecord>("encoding", out var encoding)) {
                throw new NullReferenceException(nameof(encoding));
            }*/

            GetRecord("encoding-size", out EncodingSize);
            GetRecord("vfs-root", out VFSRoot);
            GetRecord("vfs-root-size", out VFSRootSize);
            GetRecords("vfs-{0}", 1, out VFSManifests);
            GetRecords("vfs-{0}-size", 1, out VFSManifestsSize);

            Root = root;
            //Encoding = encoding;
        }


        private bool GetRecord<T>(string key, [NotNullWhen(true)] out T? @out) where T : IDecodableRecord<T> {
            if (!Values.TryGetValue(key, out var vals)) {
                @out = default;
                return false;
            }

            @out = T.Decode(vals);
            return true;
        }

        private bool GetRecords<T>(string key, int baseIter, [NotNullWhen(true)] out T[]? @out) where T : IDecodableRecord<T> {
            Debug.Assert(key.Contains("{0}"));

            var values = new List<T>();
            while (true) {
                var curKey = string.Format(key, baseIter);
                if (!Values.TryGetValue(curKey, out var vals)) {
                    break;
                }

                values.Add(T.Decode(vals));
                baseIter++;
            }

            if (values.Count == 0) {
                @out = null;
                return false;
            }

            @out = [.. values];
            return true;
        }

        private interface IDecodableRecord<out T> where T : IDecodableRecord<T> {
            abstract static T Decode(List<string> vals);
        }

        public class FileRecord : IDecodableRecord<FileRecord> {
            public CKey ContentKey;
            public FullEKey EncodingKey;

            public static FileRecord Decode(List<string> vals) {
                FileRecord record = new FileRecord();

                if (vals.Count > 0) {
                    record.ContentKey = CKey.FromString(vals[0]);
                }

                if (vals.Count > 1) {
                    record.EncodingKey = FullEKey.FromString(vals[1]);
                }

                return record;
            }
        }

        public class SizeRecord : IDecodableRecord<SizeRecord> {
            public int ContentSize;
            public int EncodedSize;

            public static SizeRecord Decode(List<string> vals) {
                return new SizeRecord {
                    ContentSize = int.Parse(vals[0]),
                    EncodedSize = int.Parse(vals[1])
                };
            }
        }
    }
}