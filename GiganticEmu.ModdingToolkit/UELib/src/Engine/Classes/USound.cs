using System.Collections.Generic;
using UELib.Core;

namespace UELib.Engine
{
    [UnrealRegisterClass]
    public class USound : UObject, IUnrealViewable, IUnrealExportable
    {
        #region Serialized Members

        public UName FileType;
        /// <summary>
        /// The likely hood that this sound will be selected from an array of sounds, see "USoundGroup".
        /// Null if not serialized.
        /// </summary>
        public float? Likelihood;
        public byte[] Data;

        #endregion

        private const string WAVExtension = "wav";

        public IEnumerable<string> ExportableExtensions => new[] { WAVExtension };

        public USound()
        {
            ShouldDeserializeOnDemand = true;
        }

        public bool CompatableExport()
        {
            return Package.Version >= 61 && Package.Version <= 129
                                         && FileType != null && FileType.ToString().ToLower() == WAVExtension &&
                                         Data != null;
        }

        public void SerializeExport(string desiredExportExtension, System.IO.Stream exportStream)
        {
            switch (desiredExportExtension)
            {
                case WAVExtension:
                    exportStream.Write(Data, 0, Data.Length);
                    break;
            }
        }

        protected override void Deserialize()
        {
            base.Deserialize();

            FileType = _Buffer.ReadNameReference();
            Record(nameof(FileType), FileType);
#if UT
            if ((Package.Build == UnrealPackage.GameBuild.BuildName.UT2004
                 || Package.Build == UnrealPackage.GameBuild.BuildName.UT2003) /*&& Package.LicenseeVersion >= 2*/)
            {
                Likelihood = _Buffer.ReadFloat();
                Record(nameof(Likelihood), Likelihood);
            }
#endif
            if (Package.Version >= 63)
            {
                // LazyArray skip-offset
                int nextSerialOffset = _Buffer.ReadInt32();
                Record(nameof(nextSerialOffset), nextSerialOffset);
            }

            // Resource Interchange File Format
            Data = new byte[_Buffer.ReadIndex()];
            _Buffer.Read(Data, 0, Data.Length);
            Record(nameof(Data), Data);
        }
    }
}