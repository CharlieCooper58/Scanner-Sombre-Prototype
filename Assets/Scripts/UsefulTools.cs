using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharlieExtras
{
    public class CountdownTimer
    {
        float timerMax;
        float currentTime;
        public CountdownTimer(float timerMax)
        {
            this.timerMax = timerMax;
            currentTime = timerMax;
        }
        public void Tick(float deltaTime)
        {
            currentTime -= deltaTime;
        }
        public bool CheckTimer()
        {
            if(currentTime <= 0)
            {
                currentTime = timerMax;
                return true;
            }
            return false;
        }
    }
}
