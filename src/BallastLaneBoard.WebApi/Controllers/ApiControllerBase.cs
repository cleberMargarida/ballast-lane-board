using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Domain.Core;
using BallastLaneBoard.Domain.Identity;
using BallastLaneBoard.Domain.TaskManagement;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Reflection;

namespace BallastLaneBoard.WebApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkOrError<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return ErrorResult(result.Error!.Value);
    }

    protected IActionResult NoContentOrError(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return ErrorResult(result.Error!.Value);
    }

    protected IActionResult CreatedAtOrError<T>(Result<T> result, string actionName, Func<T, object> routeValuesFactory)
    {
        if (result.IsFailed)
            return ErrorResult(result.Error!.Value);

        var value = result.Value;
        return CreatedAtAction(actionName, routeValuesFactory(value!), value);
    }

    private IActionResult ErrorResult(int errorCode)
    {
        var message = MapErrorMessage(errorCode);

        if (Enum.IsDefined(typeof(ApplicationError), errorCode))
        {
            var appError = (ApplicationError)errorCode;
            return appError switch
            {
                ApplicationError.NotFound => NotFound(message),
                ApplicationError.Forbidden => Forbid(),
                ApplicationError.Unauthorized => Unauthorized(message),
                ApplicationError.Conflict => Conflict(message),
                _ => BadRequest(message),
            };
        }

        return BadRequest(message);
    }

    private static string MapErrorMessage(int errorCode)
    {
        var hasKnownErrorCode = false;

        if (TryGetEnumDescription<TaskError>(errorCode, out var taskErrorDescription))
            return taskErrorDescription;
        hasKnownErrorCode |= Enum.IsDefined(typeof(TaskError), errorCode);

        if (TryGetEnumDescription<UserError>(errorCode, out var userErrorDescription))
            return userErrorDescription;
        hasKnownErrorCode |= Enum.IsDefined(typeof(UserError), errorCode);

        if (TryGetEnumDescription<ApplicationError>(errorCode, out var applicationErrorDescription))
            return applicationErrorDescription;
        hasKnownErrorCode |= Enum.IsDefined(typeof(ApplicationError), errorCode);

        return hasKnownErrorCode
            ? $"Validation error ({errorCode})."
            : $"An unexpected error occurred ({errorCode}).";
    }

    private static bool TryGetEnumDescription<TEnum>(int errorCode, out string description)
        where TEnum : struct, Enum
    {
        description = string.Empty;

        if (!Enum.IsDefined(typeof(TEnum), errorCode))
            return false;

        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), errorCode);
        var resolvedDescription = GetDescription(enumValue);
        if (string.IsNullOrWhiteSpace(resolvedDescription))
            return false;

        description = resolvedDescription;
        return true;
    }

    internal static string? GetDescription<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        var enumName = Enum.GetName(enumType, value);
        if (enumName is null)
            return null;

        var field = enumType.GetField(enumName, BindingFlags.Public | BindingFlags.Static);
        return field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }
}