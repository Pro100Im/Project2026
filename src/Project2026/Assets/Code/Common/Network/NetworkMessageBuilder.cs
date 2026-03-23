using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Code.Common.Network
{
    public class NetworkMessageBuilder : IDisposable
    {
        private FastBufferWriter _writer;

        public NetworkMessageBuilder(int totalSize)
        {
            _writer = new FastBufferWriter(totalSize, Allocator.Temp);
        }

        public NetworkMessageBuilder Write(int value)
        {
            _writer.WriteValueSafe(value);

            return this;
        }

        public NetworkMessageBuilder Write(uint value)
        {
            _writer.WriteValueSafe(value);

            return this;
        }

        public NetworkMessageBuilder Write(ulong value)
        {
            _writer.WriteValueSafe(value);

            return this;
        }

        public NetworkMessageBuilder Write(float value)
        {
            _writer.WriteValueSafe(value);

            return this;
        }

        public NetworkMessageBuilder Write(string value)
        {
            _writer.WriteValueSafe(value);

            return this;
        }

        public NetworkMessageBuilder Write(Vector2 value)
        {
            _writer.WriteValueSafe(value);

            return this;
        }

        public NetworkMessageBuilder Write(Vector3 value)
        {
            _writer.WriteValueSafe(value);

            return this;
        }

        public FastBufferWriter Build()
        {
            return _writer;
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
