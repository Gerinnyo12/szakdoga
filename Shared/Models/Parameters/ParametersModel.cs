using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Parameters
{
    public class ParametersModel
    {
        [Required(ErrorMessage = "Az abszolút útvonalat kötelező megadni!")]
        public string Path { get; set; } = @"C:\";
        [Required(ErrorMessage = "A mintát kötelező megadni!")]
        public string Pattern { get; set; } = "*";
        [Required(ErrorMessage = "A várakozási időt kötelező megadni!")]
        [Range(2, 30000, ErrorMessage = "Egy 2 milisec és 30 sec kötötti értéket adj meg!")]
        public int MaxCopyTimeInMiliSec { get; set; } = 1000;
    }
}
