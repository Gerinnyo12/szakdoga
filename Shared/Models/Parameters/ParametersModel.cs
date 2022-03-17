using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Parameters
{
    public class ParametersModel
    {
        [Required(ErrorMessage = "A mintát kötelező megadni!")]
        [FileExtensions(Extensions = ".zip", ErrorMessage = ".zip kiterjesztésű mintát kell megadni!")]
        public string Pattern { get; set; } = "*.zip";
        [Required(ErrorMessage = "A várakozási időt kötelező megadni!")]
        [Range(2, 30000, ErrorMessage = "Egy 2 milisec és 30 sec kötötti értéket adj meg!")]
        public int MaxCopyTimeInMiliSec { get; set; } = 2000;
    }
}
