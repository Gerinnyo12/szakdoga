using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Parameters
{
    public class ParametersModel
    {
        public const string ParameterString = "Parameters";

        [Required(ErrorMessage = "Ezt a mezőt kötelező megadni")]
        public string Path { get; set; }
        [Required(ErrorMessage = "Ezt a mezőt kötelező megadni")]
        public string Pattern { get; set; }
        [Required(ErrorMessage = "Ezt a mezőt kötelező megadni")]
        public int MaxCopyTimeInMiliSec { get; set; }
    }
}
