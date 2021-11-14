using System;
using System.Runtime.InteropServices;

namespace DragonGame.Engine.Rollback
{
    internal class StateBuffer : IDisposable
    {
        private unsafe byte* _backingBuffer = null;
        private int _bufferSize;
        private int _neededSize;
        private int _readPtr;
        private int _writePtr;


        public StateBuffer()
        {
        }

        public StateBuffer(Span<byte> initialContents)
        {
            _neededSize += initialContents.Length;
            ResizeBuffer();

            unsafe
            {
                fixed (byte* initialPtr = initialContents)
                {
                    Buffer.MemoryCopy(initialPtr, _backingBuffer, _bufferSize, initialContents.Length);
                }
            }
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private unsafe void ResizeBuffer()
        {
            if (_neededSize <= _bufferSize) return;
            _bufferSize = _bufferSize == 0 ? _neededSize : _bufferSize * 2;

            _backingBuffer = (byte*)(_backingBuffer == null
                ? Marshal.AllocHGlobal(_bufferSize)
                : Marshal.ReAllocHGlobal((IntPtr)_backingBuffer, (IntPtr)_bufferSize));
        }

        public unsafe void Write<T>(T value) where T : unmanaged
        {
            _neededSize += sizeof(T);
            ResizeBuffer();

            *(T*)(_backingBuffer + _writePtr) = value;
            _writePtr += sizeof(T);
        }

        public unsafe T Read<T>() where T : unmanaged
        {
            var value = *(T*)(_backingBuffer + _readPtr);
            _readPtr += sizeof(T);

            return value;
        }

        public unsafe Span<byte> AsSpan()
        {
            return new Span<byte>(_backingBuffer, _bufferSize);
        }

        public void Recycle()
        {
            _writePtr = 0;
            _readPtr = 0;
            _neededSize = 0;
        }

        private unsafe void ReleaseUnmanagedResources()
        {
            if (_backingBuffer == null) return;

            Marshal.FreeHGlobal(new IntPtr(_backingBuffer));
            _backingBuffer = null;
        }

        ~StateBuffer()
        {
            ReleaseUnmanagedResources();
        }
    }
}