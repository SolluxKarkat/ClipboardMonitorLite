﻿using System;

namespace ClipboardLibrary
{
    class TimeCalculate
    {
        public DateTime TargetDate { get; set; }

        public void CalculateToSeconds(int time, int format = (int)Time.Format.Minutes)
        {
            int tempTime = 0;
            switch (format)
            {
                case (int)Time.Format.Seconds:
                    tempTime = time;
                    break;
                case (int)Time.Format.Minutes:
                    tempTime = time * 60;
                    break;
                case (int)Time.Format.Hours:
                    tempTime = time * 3600;
                    break;
                default:
                    tempTime = 0;
                    break;
            }
            TargetDate = DateTime.Now.AddSeconds(tempTime);
        }
    }
}