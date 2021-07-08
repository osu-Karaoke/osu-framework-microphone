// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Input.Bindings
{
    public interface IMicrophoneAction
    {
        /// <summary>
        /// Detected pitch
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Detected volumn
        /// </summary>
        public float Volumn { get; set; }
    }
}
