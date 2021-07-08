// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using System.Collections.Generic;

namespace osu.Framework.Input.Bindings
{
    public class MicrophoneKeyBindingContainer<T> : KeyBindingContainer<T> where T : struct, IMicrophoneAction
    {
        public override IEnumerable<KeyBinding> DefaultKeyBindings => new List<KeyBinding>();

        protected override bool Handle(UIEvent e)
        {
            return base.Handle(e);
        }
    }
}
