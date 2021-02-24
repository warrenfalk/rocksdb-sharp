namespace RocksDbSharp
{
    public class LiveFileMetadata
    {
        public FileMetadata FileMetadata;
        public FileDataMetadata FileDataMetadata;
    }   

    public class FileMetadata
    {
        public string FileName;
        public int FileLevel;
        public ulong FileSize;
    }

    public class FileDataMetadata
    {
        public string SmallestKeyInFile;
        public string LargestKeyInFile;
        public ulong NumEntriesInFile;
        public ulong NumDeletionsInFile;
    }
}
