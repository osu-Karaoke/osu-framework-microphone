﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using NWaves.Features;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.States;
using osu.Framework.Platform;
using System;
using System.Runtime.InteropServices;

namespace osu.Framework.Input.Handlers.Microphone
{
    public class OsuTKMicrophoneHandler : InputHandler
    {
        public override bool IsActive => Bass.RecordingDeviceCount > 0;
        public override int Priority => 3;

        private readonly int deviceIndex;
        private int stream;

        public OsuTKMicrophoneHandler(int device)
        {
            deviceIndex = device;
        }

        public override bool Initialize(GameHost host)
        {
            Enabled.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    // Open microphone device if available
                    Bass.RecordInit(deviceIndex);
                    stream = Bass.RecordStart(44100, 2, BassFlags.RecordPause | BassFlags.Float, 10, procedure);

                    // Start channel
                    Bass.ChannelPlay(stream);
                }
                else
                {
                    // Pause channel
                    Bass.ChannelPause(stream);

                    // Close microphone
                    Bass.RecordFree();
                }
            }, true);

            return true;
        }

        private float[] buffer;

        private bool procedure(int handle, IntPtr buffer, int length, IntPtr user)
        {
            // Read and save buffer
            if (this.buffer == null || this.buffer.Length < length / 4)
                this.buffer = new float[length / 4];

            Marshal.Copy(buffer, this.buffer, 0, length / 4);

            // Process buffer
            var pitch = Pitch.FromYin(this.buffer, 44100, low: 40, high: 1000);
            //var loudness = Perceptual.Loudness(this.buffer);
            onPitchDetected(new MicrophoneState(pitch, 0));

            return true;
        }

        private MicrophoneState lastState = new MicrophoneState();

        private void onPitchDetected(MicrophoneState state)
        {
            // do not continuous sending no sound event
            if (!state.HasSound && lastState.HasSound == state.HasSound)
                return;

            // Throw into pending input
            PendingInputs.Enqueue(new MicrophoneInput
            {
                State = state
            });

            lastState = state;
        }
    }
}
