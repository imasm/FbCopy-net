namespace FbCopy
{
    internal class CopyGeneratorInfo
    {
        public string SourceName { get; }
        public string DestName { get; }

        public CopyGeneratorInfo(string sourceName, string destName)
        {
            SourceName = sourceName;
            DestName = destName;
        }
    }
}
