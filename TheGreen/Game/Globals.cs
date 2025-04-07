using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheGreen.Game
{
    static class Globals
    {
        public static Point NativeResolution = new Point(960, 540);
        public static readonly int TILESIZE = 16;
        public static Point DrawDistance = new Point(960 / TILESIZE + 1, 540 / TILESIZE + 2);
        public static readonly float GRAVITY = 1400.0f;
        public static Point ScreenCenter = new Point(960 / 2, 540 / 2);
        /// <summary>
        /// The light level of an empty tile at the current time. Does not affect stored tile light, only the final draw light
        /// </summary>
        public static byte GlobalLight = 255;
        /// <summary>
        /// The time value of the current day in the world
        /// </summary>
        private static double _gameTime;

        /// <summary>
        /// The total time in seconds of a game day
        /// </summary>
        public static int TotalDayCycleTime;

        /// <summary>
        /// A mapping gradient of the current day time to the current Global lighting value.
        /// </summary>
        private static List<(int, byte)> _timeToLightGradient;

        public static void UpdateGameTime(double delta)
        {
            //wrap time between 0 and totalTimeInDay
            _gameTime += delta;
            _gameTime = (_gameTime + TotalDayCycleTime) % TotalDayCycleTime;
            //calculate gradient light value
            GlobalLight = (byte)Lerp(0.0, 1.0, _gameTime);
            for (int i = 0; i < _timeToLightGradient.Count; i++)
            {
                var (x1, y1) = _timeToLightGradient[i];
                var (x2, y2) = _timeToLightGradient[(i+ 1) % _timeToLightGradient.Count];

                if (_gameTime >= x1 && _gameTime <= x2)
                {
                    GlobalLight = (byte)Lerp(y1, y2, (_gameTime - x1) / (x2 - x1));
                }
            }
        }
        public static int GetGameTime()
        {
            return (int)_gameTime;
        }

        private static double Lerp(double y1, double y2, double t)
        {
            return y1 + (y2 - y1) * t;
        }

        /// <summary>
        /// Should only be called at the start of the game
        /// </summary>
        /// <param name="time"></param><param name="totalDayCycleTime">The total time in a game day in seconds</param>
        public static void StartGameClock(int currentTime, int totalDayCycleTime)
        {
            _gameTime = currentTime;
            TotalDayCycleTime = totalDayCycleTime;
            _timeToLightGradient = [
                (0, 40),
                (totalDayCycleTime/4 - totalDayCycleTime/8, 40),
                (totalDayCycleTime/4 + totalDayCycleTime/8, 255),

                (totalDayCycleTime/2 + totalDayCycleTime/4 - totalDayCycleTime/8, 255),
                (totalDayCycleTime/2 + totalDayCycleTime/4 + totalDayCycleTime/8, 40),
                (totalDayCycleTime, 40)
            ];
        }
    }
}
