﻿using System;

namespace CSharpPICollision
{
    static class Utils
    {
        /// <summary>
        /// Returns segment with specific position and length
        /// </summary>
        /// <param name="position">Segment position</param>
        /// <param name="size">Segment length</param>
        /// <returns>Segment with specific position and length</returns>
        public static Segment GetSegment(double position, double size)
        {
            return new Segment(position, position + size);
        }
        /// <summary>
        /// Returns segment legnth
        /// </summary>
        /// <param name="segment">Segment to which method is applied</param>
        /// <returns>Length</returns>
        public static double GetLength(Segment segment)
        {
            return Math.Abs(segment.Point2 - segment.Point1);
        }
        /// <summary>
        /// Retruns center of segment
        /// </summary>
        /// <param name="segment">Segment to which method is applied</param>
        /// <returns>Point on axis</returns>
        public static double GetCenter(Segment segment)
        {
            return (segment.Point2 + segment.Point1) / 2;
        }
        /// <summary>
        /// Returns distance between two segments
        /// </summary>
        /// <param name="segment1">First segment to which method is applied</param>
        /// <param name="segment2">Second segment to which method is applied</param>
        /// <returns>Distance</returns>
        public static double DistanceBeetweenSegments(Segment segment1, Segment segment2)
        {
            double center1 = Utils.GetCenter(segment1);
            double center2 = Utils.GetCenter(segment2);
            double radius1 = Utils.GetLength(segment1) / 2;
            double radius2 = Utils.GetLength(segment2) / 2;

            return Utils.GetLength(new Segment(center2, center1)) - (radius1 + radius2);
        }
        /// <summary>
        /// Returns distance between segment and point
        /// </summary>
        /// <param name="segment">Segment to which method is applied</param>
        /// <param name="point">Point to which method is applied</param>
        /// <returns>Distance</returns>
        public static double DistanceBeetweenSegmentAndPoint(Segment segment, double point)
        {
            return Utils.GetLength(new Segment(Utils.GetCenter(segment), point)) - Utils.GetLength(segment) / 2;
        }
    }
}