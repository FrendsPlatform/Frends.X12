using System;
using Frends.X12.CreateFromJson.Definitions;

namespace Frends.X12.CreateFromJson;

/// <summary>
/// Class to handle exceptions returned to Frends
/// </summary>
public static class ErrorHandler
{
    /// <summary>
    /// Handler for exceptions
    /// </summary>
    /// <param name="exception">Caught exception</param>
    /// <param name="throwOnFailure">Frends flag</param>
    /// <param name="errorMessageOnFailure">Message to throw in error event</param>
    /// <returns>Throw exception if a flag is true, else return Result with Error info</returns>
    public static Result Handle(
        Exception exception,
        bool throwOnFailure,
        string errorMessageOnFailure)
    {
        if (throwOnFailure)
        {
            if (string.IsNullOrEmpty(errorMessageOnFailure))
                throw new Exception(exception.Message, exception);

            throw new Exception(errorMessageOnFailure, exception);
        }

        var errorMessage = !string.IsNullOrEmpty(errorMessageOnFailure)
            ? $"{errorMessageOnFailure}: {exception.Message}"
            : exception.Message;

        return new Result
        {
            Success = false,
            Output = null,
            Error = new Error
            {
                Message = errorMessage,
                AdditionalInfo = exception,
            },
        };
    }
}