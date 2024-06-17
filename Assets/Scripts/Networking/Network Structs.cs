using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NetworkTools
{
    public class CircularBuffer<T>
    {
        T[] buffer;
        int bufferSize;

        public CircularBuffer(int bufferSize)
        {
            this.bufferSize = bufferSize;
            buffer = new T[bufferSize];
        }

        public void Add(T item, int index) => buffer[index % bufferSize] = item;
        public T Get(int index) => buffer[index % bufferSize];
        public void Clear() => buffer = new T[bufferSize];
    }
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public Vector3 inputVector;
        public Vector2 lookAround;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref inputVector);
            serializer.SerializeValue(ref lookAround);
        }
    }
    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 lookDirection;
        public float controllerHeight;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref lookDirection);
            serializer.SerializeValue(ref controllerHeight);
        }
    }

}

