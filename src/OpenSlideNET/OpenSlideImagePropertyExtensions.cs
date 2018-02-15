using System;

namespace OpenSlideNET
{
    public static class OpenSlideImagePropertyExtensions
    {
        public static string GetProperty(this OpenSlideImage image, string name, string defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            return value;
        }

        public static short GetProperty(this OpenSlideImage image, string name, short defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!short.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        public static ushort GetProperty(this OpenSlideImage image, string name, ushort defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!ushort.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        public static int GetProperty(this OpenSlideImage image, string name, int defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!int.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        public static uint GetProperty(this OpenSlideImage image, string name, uint defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!uint.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        public static long GetProperty(this OpenSlideImage image, string name, long defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!long.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        public static ulong GetProperty(this OpenSlideImage image, string name, ulong defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!ulong.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        public static float GetProperty(this OpenSlideImage image, string name, float defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!float.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        public static double GetProperty(this OpenSlideImage image, string name, double defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string value))
            {
                return defaultValue;
            }
            if (!double.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }
    }
}
