namespace Shared.Models
{
    public class Parameters
    {
        public const string ParameterString = "Parameters";

        public string Path { get; set; }
        public string Pattern { get; set; }
        public int MaxCopyTimeInMiliSec { get; set; }
    }
}
