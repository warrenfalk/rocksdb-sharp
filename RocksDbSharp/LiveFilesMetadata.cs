namespace RocksDbSharp
{
    public class LiveFileMetadata
    {
        public string FileName;
        public int FileLevel;
        public ulong FileSize; 
        public string SmallestKeyInFile;
        public string LargestKeyInFile;
        public ulong NumEntriesInFile;
        public ulong NumDeletionsInFile;
    }   
}
