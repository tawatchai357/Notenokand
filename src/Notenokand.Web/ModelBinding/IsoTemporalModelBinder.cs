using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Notenokand.Web.ModelBinding;

public sealed class IsoTemporalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var result = context.ValueProvider.GetValue(context.ModelName);
        if (result == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, result);
        var value = result.FirstValue;
        var underlyingType = Nullable.GetUnderlyingType(context.ModelType) ?? context.ModelType;
        var isNullable = Nullable.GetUnderlyingType(context.ModelType) is not null;
        if (string.IsNullOrWhiteSpace(value))
        {
            if (isNullable) context.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (underlyingType == typeof(DateOnly) && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            context.Result = ModelBindingResult.Success(date);
        else if (underlyingType == typeof(DateTime) && DateTime.TryParseExact(value, ["yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            context.Result = ModelBindingResult.Success(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified));
        else
            context.ModelState.TryAddModelError(context.ModelName, "รูปแบบวันที่ไม่ถูกต้อง");
        return Task.CompletedTask;
    }
}

public sealed class IsoTemporalModelBinderProvider : IModelBinderProvider
{
    private static readonly IModelBinder Binder = new IsoTemporalModelBinder();
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type == typeof(DateOnly) || type == typeof(DateTime) ? Binder : null;
    }
}