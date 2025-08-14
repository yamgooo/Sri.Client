// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Yamgooo.SRI.Client.Common;

public enum ApiResultStatusCode
{
    [Display(Name = "Operation completed successfully")]
    Success = 0,

    [Display(Name = "An error occurred on the server")]
    ServerError = 1,

    [Display(Name = "The sent parameters are invalid")]
    BadRequest = 2,

    [Display(Name = "Not found")] 
    NotFound = 3,

    [Display(Name = "The list is empty")] 
    ListEmpty = 4,

    [Display(Name = "An error occurred during processing")]
    LogicError = 5,

    [Display(Name = "Authentication error")]
    UnAuthorized = 6
}
