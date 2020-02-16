﻿// Copyright (c) andy840119 <andy840119@gmail.com>. Licensed under the GPL Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
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

        private readonly PitchTracker.PitchTracker pitchTracker = new PitchTracker.PitchTracker();
        private readonly int deviceIndex;
        private int stream;

        public OsuTKMicrophoneHandler(int device)
        {
            deviceIndex = device;
        }

        public float DetectLevelThreshold
        {
            get => pitchTracker.DetectLevelThreshold;
            set => pitchTracker.DetectLevelThreshold = value;
        }

        public override bool Initialize(GameHost host)
        {
            try
            {
                // Open microphone device if available
                Bass.RecordInit(deviceIndex);
                stream = Bass.RecordStart(44100, 2, BassFlags.RecordPause | BassFlags.Float, 60, Procedure);
                pitchTracker.PitchDetected += onPitchDetected;

                Enabled.BindDisabledChanged(enabled =>
                {
                    if (enabled)
                    {
                        // Start channel
                        Bass.ChannelPlay(stream);
                    }
                    else
                    {
                        // Pause channel
                        Bass.ChannelPause(stream);
                    }
                }, true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private float[] buffer;

        private bool Procedure(int Handle, IntPtr Buffer, int Length, IntPtr User)
        {
            // Read and save buffer
            if (buffer == null || buffer.Length < Length / 4)
                buffer = new float[Length / 4];

            Marshal.Copy(Buffer, buffer, 0, Length / 4);

            // Process buffer
            pitchTracker.ProcessBuffer(buffer);

            return true;
        }

        private MicrophoneState lastState = new MicrophoneState();

        void onPitchDetected(MicrophoneState state)
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Close microphone
            Bass.RecordFree();
        }
    }
}
