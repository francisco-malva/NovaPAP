using System;

namespace DragonGame.Engine.Utilities
{
    internal class DeterministicRandom
    {
        private byte[] _randomBytes = new byte[2048];
        private int _randomPtr;

        public DeterministicRandom()
        {
            var random = new Random();
            random.NextBytes(_randomBytes);
        }

        /// <summary>
        ///     Gets a random byte between 0 and byte.MaxValue.
        /// </summary>
        /// <returns></returns>
        public byte GetByte()
        {
            var value = _randomBytes[_randomPtr++];

            if (_randomPtr == _randomBytes.Length) _randomPtr = 0;

            return value;
        }

        /// <summary>
        ///     Compose an int off of 4 random bytes.
        /// </summary>
        /// <returns></returns>
        private uint ComposeInteger()
        {
            return (uint)(GetByte() | (GetByte() << 8) | (GetByte() << 16) | (GetByte() << 24));
        }

        public int GetInteger(int min, int max)
        {
            return (int)(min + MathF.Floor(GetFloat() * (max - min)));
        }

        public int GetInteger(int max)
        {
            return GetInteger(0, max);
        }

        /// <summary>
        ///     Returns a float between 0,0 and 1,0.
        /// </summary>
        /// <returns></returns>
        public float GetFloat()
        {
            var value = ComposeInteger() / (float)uint.MaxValue;
            return value;
        }


        public byte[] GetInternalArray()
        {
            var array = new byte[_randomBytes.Length];
            Array.Copy(_randomBytes, array, array.Length);
            return array;
        }

        public void SetInternalArray(ref byte[] array)
        {
            Array.Resize(ref _randomBytes, array.Length);
            Array.Copy(array, _randomBytes, array.Length);
        }
    }
}