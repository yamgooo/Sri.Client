// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Yamgooo.SRI.Client.Common;

public class ApiResult(bool isSuccess, ApiResultStatusCode statusCode, string? message = null)
{
        public bool IsSuccess { get; set; } = isSuccess;
        public ApiResultStatusCode StatusCode { get; set; } = statusCode;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; } = message ?? statusCode.ToString();

        #region Implicit Operators
        public static implicit operator ApiResult(OkResult result)
        {
            return new ApiResult(true, ApiResultStatusCode.Success);
        }

        public static implicit operator ApiResult(BadRequestResult result)
        {
            return new ApiResult(false, ApiResultStatusCode.BadRequest);
        }

        public static implicit operator ApiResult(BadRequestObjectResult result)
        {
            var message = result.Value?.ToString();
            if (result.Value is not SerializableError errors)
                return new ApiResult(false, ApiResultStatusCode.BadRequest, message);
            var errorMessages = errors.SelectMany(p => (string[])p.Value).Distinct();
            message = string.Join(" | ", errorMessages);
            return new ApiResult(false, ApiResultStatusCode.BadRequest, message);
        }

        public static implicit operator ApiResult(ContentResult result)
        {
            return new ApiResult(true, ApiResultStatusCode.Success, result.Content);
        }

        public static implicit operator ApiResult(NotFoundResult result)
        {
            return new ApiResult(false, ApiResultStatusCode.NotFound);
        }
        #endregion
    }

    public class ApiResult<TData>(bool isSuccess, ApiResultStatusCode statusCode, TData? data, string? message = null)
        : ApiResult(isSuccess, statusCode, message)
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TData? Data { get; set; } = data;

        #region Implicit Operators
        public static implicit operator ApiResult<TData>(TData? data)
        {
            return new ApiResult<TData>(true, ApiResultStatusCode.Success, data);
        }

        public static implicit operator ApiResult<TData>(OkResult result)
        {
            return new ApiResult<TData>(true, ApiResultStatusCode.Success, default);
        }

        public static implicit operator ApiResult<TData>(OkObjectResult result)
        {
            return new ApiResult<TData>(true, ApiResultStatusCode.Success, result.Value is TData typedValue ? typedValue : default);
        }

        public static implicit operator ApiResult<TData>(BadRequestResult result)
        {
            return new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, default);
        }

        public static implicit operator ApiResult<TData>(BadRequestObjectResult result)
        {
            var message = result.Value?.ToString();
            if (result.Value is not SerializableError errors)
                return new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, default, message);
            var errorMessages = errors.SelectMany(p => (string[])p.Value).Distinct();
            message = string.Join(" | ", errorMessages);
            return new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, default, message);
        }

        public static implicit operator ApiResult<TData>(ContentResult result)
        {
            return new ApiResult<TData>(true, ApiResultStatusCode.Success, default, result.Content);
        }

        public static implicit operator ApiResult<TData>(NotFoundResult result)
        {
            return new ApiResult<TData>(false, ApiResultStatusCode.NotFound, default);
        }

        public static implicit operator ApiResult<TData>(NotFoundObjectResult result)
        {
            return new ApiResult<TData>(false, ApiResultStatusCode.NotFound, result.Value is TData typedValue ? typedValue : default);
        }
        #endregion
    }