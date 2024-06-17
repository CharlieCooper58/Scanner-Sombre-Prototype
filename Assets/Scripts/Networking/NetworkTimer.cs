using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTools
{
    public class NetworkTimer
    {
        float timer;
        public float MinTimeBetweenTicks { get; }
        public int currentTick;
        public NetworkTimer(float serverTickRate)
        {
            MinTimeBetweenTicks = 1f / serverTickRate;
        }
        public void Update(float deltaTime)
        {
            timer += deltaTime;
        }
        public bool ShouldTick()
        {
            if (timer >= MinTimeBetweenTicks)
            {
                timer -= MinTimeBetweenTicks;
                currentTick++;
                return true;
            }
            return false;
        }
    }
}

