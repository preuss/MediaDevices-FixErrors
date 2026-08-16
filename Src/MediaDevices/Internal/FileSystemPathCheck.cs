using System.Runtime.CompilerServices;

namespace MediaDevices.Internal;

internal static class FileSystemPathCheck
{
    private const int maxPathLength = 255;

    /// <summary>
    /// Validates a device path and throws the appropriate exception when invalid.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="paramName">The name of the parameter, automatically inferred from the call site.</param>
    /// <exception cref="ArgumentNullException">path is null.</exception>
    /// <exception cref="ArgumentException">path is empty or whitespace, or contains invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
    /// <exception cref="PathTooLongException">path has more than <c>maxPathLength</c> characters.</exception>
    public static void ThrowIfInvalidPath(string? path, [CallerArgumentExpression(nameof(path))] string? paramName = null)
    {
        // separate check for a specific error message
        ArgumentNullException.ThrowIfNull(path, paramName);
        ArgumentException.ThrowIfNullOrWhiteSpace(path, paramName);
        var invalidChars = Path.GetInvalidPathChars();
        if (path.IndexOfAny(invalidChars) >= 0)
        {
            throw new ArgumentException($"{paramName} contains invalid path characters");
        }
        if (path.Length > maxPathLength)
        {
            throw new PathTooLongException($"{paramName} has more than {maxPathLength} characters");
        }
    }
}
