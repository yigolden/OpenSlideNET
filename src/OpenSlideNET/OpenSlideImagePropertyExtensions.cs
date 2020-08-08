using System;

namespace OpenSlideNET
{
    /// <summary>
    /// Helper class for reading properties of <see cref="OpenSlideImage"/>.
    /// </summary>
    public static class OpenSlideImagePropertyExtensions
    {
        /// <summary>
        /// Read the property with the specified <paramref name="name"/>. If the property does not exists, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The value returned when the specified property does not exists.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static string? GetProperty(this OpenSlideImage image, string name, string? defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="short"/> value. If the property does not exists or can not be parsed as <see cref="short"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static short GetProperty(this OpenSlideImage image, string name, short defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            if (!short.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }


        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="ushort"/> value. If the property does not exists or can not be parsed as <see cref="ushort"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static ushort GetProperty(this OpenSlideImage image, string name, ushort defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            if (!ushort.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }


        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="int"/> value. If the property does not exists or can not be parsed as <see cref="int"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static int GetProperty(this OpenSlideImage image, string name, int defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            if (!int.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }


        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="uint"/> value. If the property does not exists or can not be parsed as <see cref="uint"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static uint GetProperty(this OpenSlideImage image, string name, uint defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            if (!uint.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }


        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="long"/> value. If the property does not exists or can not be parsed as <see cref="long"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static long GetProperty(this OpenSlideImage image, string name, long defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            if (!long.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }


        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="ulong"/> value. If the property does not exists or can not be parsed as <see cref="ulong"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static ulong GetProperty(this OpenSlideImage image, string name, ulong defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            if (!ulong.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }


        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="float"/> value. If the property does not exists or can not be parsed as <see cref="float"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static float GetProperty(this OpenSlideImage image, string name, float defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
            {
                return defaultValue;
            }
            if (!float.TryParse(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }


        /// <summary>
        /// Read the property with the specified <paramref name="name"/> and parse it as a <see cref="double"/> value. If the property does not exists or can not be parsed as <see cref="double"/>, This function returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="image">The <see cref="OpenSlideImage"/> object.</param>
        /// <param name="name">The property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the property or <paramref name="defaultValue"/>.</returns>
        public static double GetProperty(this OpenSlideImage image, string name, double defaultValue)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!image.TryGetProperty(name, out string? value))
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
