using OpenSlideNET.Interop;

namespace OpenSlideNET
{
    internal static class ThrowHelper
    {
        internal static void CheckAndThrowError(OpenSlideImageSafeHandle osr)
        {
            string message = OpenSlideInterop.GetError(osr);
            if (message != null)
            {
                ThrowOpenSlideException(message);
            }
        }

        private static void ThrowOpenSlideException(string message)
        {
            throw new OpenSlideException(message);
        }

        internal static bool TryCheckError(OpenSlideImageSafeHandle osr, out string message)
        {
            message = OpenSlideInterop.GetError(osr);
            return message == null;
        }
    }
}
