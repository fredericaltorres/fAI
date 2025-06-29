namespace fAI.SourceCodeAnalysis
{
    public class FileLocation
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }

        public override string ToString()
        {
            return $"{FileName}:{LineNumber}";
        }
    }
}




