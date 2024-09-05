// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>Extension methods for <see cref="FadeGroup"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/FadeGroupExtensions
    public static class FadeGroupExtensions
    {
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Assigns the `function` as the <see cref="FadeGroup.Easing"/>.
        /// </summary>
        /// <remarks>
        /// This method allows you to avoid null-checking the `fade`.
        /// <para></para>
        /// <em>Animancer Lite ignores this feature in runtime builds.</em>
        /// </remarks>
        public static void SetEasing(this FadeGroup fade, Func<float, float> function)
        {
            if (fade != null)
                fade.Easing = function;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Assigns the <see cref="Easing.GetDelegate(Easing.Function)"/> as the
        /// <see cref="FadeGroup.Easing"/>.
        /// </summary>
        /// <remarks>
        /// This method allows you to avoid null-checking the `fade`.
        /// <para></para>
        /// <em>Animancer Lite ignores this feature in runtime builds.</em>
        /// </remarks>
        public static void SetEasing(this FadeGroup fade, Easing.Function function)
        {
            if (fade != null)
                fade.Easing = function.GetDelegate();
        }

        /************************************************************************************************************************/
    }
}

