/*
 * Copyright (c) 2010 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using UnityEngine;

namespace org.critterai.geom
{
    /// <summary>
    /// Provides 2D line and line segment utility methods.
    /// </summary>
    /// <remarks>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class Line2
    {
        
        /// <summary>
        /// Indicates whether or not line AB intersects line BC.
        /// </summary>
        /// <param name="ax">The x-value for point A on line AB.</param>
        /// <param name="ay">The y-value for point A on line AB.</param>
        /// <param name="bx">The x-value for point B on line AB.</param>
        /// <param name="by">The y-value for point B on line AB.</param>
        /// <param name="cx">The x-value for point C on line CD.</param>
        /// <param name="cy">The y-value for point C on line CD.</param>
        /// <param name="dx">The x-value for point D on line CD.</param>
        /// <param name="dy">The y-value for point D on line CD.</param>
        /// <returns>TRUE if the two lines are either collinear or intersect 
        /// at one point.</returns>
        public static bool LinesIntersect(int ax, int ay
                , int bx, int by
                , int cx, int cy
                , int dx, int dy)
        {
            int numerator = 
                ((ay - cy) * (dx - cx)) - ((ax - cx) * (dy - cy));
            int denominator = 
                ((bx - ax) * (dy - cy)) - ((by - ay) * (dx - cx));
            if (denominator == 0 && numerator != 0)
                // Lines are parallel.
                return false; 
            // Lines are collinear or intersect at a single point.
            return true;  
        }
        
        /// <summary>
        /// Indicates whether or not line AB intersects line BC.
        /// </summary>
        /// <param name="ax">The x-value for point A on line AB.</param>
        /// <param name="ay">The y-value for point A on line AB.</param>
        /// <param name="bx">The x-value for point B on line AB.</param>
        /// <param name="by">The y-value for point B on line AB.</param>
        /// <param name="cx">The x-value for point C on line CD.</param>
        /// <param name="cy">The y-value for point C on line CD.</param>
        /// <param name="dx">The x-value for point D on line CD.</param>
        /// <param name="dy">The y-value for point D on line CD.</param>
        /// <returns>TRUE if the two lines are either collinear or intersect 
        /// at one point.</returns>
        public static bool LinesIntersect(float ax, float ay
                , float bx, float by
                , float cx, float cy
                , float dx, float dy)
        {
            float numerator = 
                ((ay - cy) * (dx - cx)) - ((ax - cx) * (dy - cy));
            float denominator = 
                ((bx - ax) * (dy - cy)) - ((by - ay) * (dx - cx));
            if (denominator == 0 && numerator != 0)
                // Lines are parallel.
                return false; 
            // Lines are collinear or intersect at a single point.
            return true;  
        }
        
        /// <summary>
        /// Returns the distance squared from point P to line segment AB.
        /// </summary>
        /// <param name="px">The x-value of point P.</param>
        /// <param name="py">The y-value of point P.</param>
        /// <param name="ax">The x-value of point A of line segment AB.</param>
        /// <param name="ay">The y-value of point A of line segment AB.</param>
        /// <param name="bx">The x-value of point B of line segment AB.</param>
        /// <param name="by">The y-value of point B of line segment AB.</param>
        /// <returns>The distance squared from point B to line segment AB.
        /// </returns>
        public static float GetPointSegmentDistanceSq(float px, float py
                , float ax, float ay
                , float bx, float by)
        {
            
            /*
             * Reference: 
             * http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
             * 
             * The goal of the algorithm is to find the point on line AB that 
             * is closest to P and then calculate the distance between P and 
             * that point.
             */
            
            float deltaABx = bx - ax;
            float deltaABy = by - ay;
            float deltaAPx = px - ax;
            float deltaAPy = py - ay;        
            
            float segmentABLengthSq = 
                deltaABx * deltaABx + deltaABy * deltaABy;
            
            if (segmentABLengthSq < MathUtil.Epsilon)
                // AB is not a line segment.  So just return
                // distanceSq from P to A
                return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
                
            float u = 
                (deltaAPx * deltaABx + deltaAPy * deltaABy) 
                    / segmentABLengthSq;
            
            if (u < 0)
                // Closest point on line AB is outside outside segment AB and 
                // closer to A. So return distanceSq from P to A.
                return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
            else if (u > 1)
                // Closest point on line AB is outside segment AB and closer 
                // to B. So return distanceSq from P to B.
                return (px - bx)*(px - bx) + (py - by)*(py - by);
            
            // Closest point on lineAB is inside segment AB.  So find the exact
            // point on AB and calculate the distanceSq from it to P.
            
            // The calculation in parenthesis is the location of the point on 
            // the line segment.
            float deltaX = (ax + u * deltaABx) - px;
            float deltaY = (ay + u * deltaABy) - py;
        
            return deltaX*deltaX + deltaY*deltaY;
        }
        
        /// <summary>
        /// Returns the distance squared from point P to line AB.
        /// </summary>
        /// <param name="px">The x-value of point P.</param>
        /// <param name="py">The y-value of point P.</param>
        /// <param name="ax">The x-value of point A of line AB.</param>
        /// <param name="ay">The y-value of point A of line AB.</param>
        /// <param name="bx">The x-value of point B of line AB.</param>
        /// <param name="by">The y-value of point B of line AB.</param>
        /// <returns>The distance squared from point B to line AB.</returns>
        public static float GetPointLineDistanceSq(float px, float py
            , float ax, float ay
            , float bx, float by)
        {
            
            /*
             * Reference: 
             * http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
             * 
             * The goal of the algorithm is to find the point on line AB that is
             * closest to P and then calculate the distance between P and that 
             * point.
             */
            
            float deltaABx = bx - ax;
            float deltaABy = by - ay;
            float deltaAPx = px - ax;
            float deltaAPy = py - ay;        
            
            float segmentABLengthSq = 
                deltaABx * deltaABx + deltaABy * deltaABy;
            
            if (segmentABLengthSq < MathUtil.Epsilon)
                // AB is not a line segment.  So just return
                // distanceSq from P to A
                return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
                
            float u = 
                (deltaAPx * deltaABx + deltaAPy * deltaABy) 
                    / segmentABLengthSq;
            
            // The calculation in parenthesis is the location of the point on 
            // the line segment.
            float deltaX = (ax + u * deltaABx) - px;
            float deltaY = (ay + u * deltaABy) - py;
        
            return deltaX*deltaX + deltaY*deltaY;
        }
        
        /// <summary>
        /// Returns the normalized vector that is perpendicular to line AB.
        /// <p>WARNING: This is an expensive method.</p>
        /// </summary>
        /// <remarks>
        /// <p>The direction of the vector will be to the right when viewed 
        /// from point A to point B along the line.</p>
        /// <p>Special Case: A zero length vector will be returned if points 
        /// A and B do not form a line.</p>
        /// </remarks>
        /// <param name="ax">The x-value of point A on line AB.</param>
        /// <param name="ay">The y-value of point A on line AB.</param>
        /// <param name="bx">The x-value of point B on line AB.</param>
        /// <param name="by">The y-value of point B on line AB.</param>
        /// <returns>The normalized vector that is perpendicular to line AB, 
        /// or a zero length vector if points A and B do not form a line.
        /// </returns>
        public static Vector2 GetNormalAB(float ax, float ay
                , float bx, float by)
        {
            if (Vector2Util.SloppyEquals(
                    ax, ay, bx, by, MathUtil.Tolerance))
                // Points do not form a line.
                return new Vector2();
            Vector2 result = Vector2Util.GetDirectionAB(ax, ay, bx, by);
            float origX = result.x;
            result.x = result.y;
            result.y = -origX;
            return result;
        }
        
        /// <summary>
        /// Determines the relationship between lines AB and CD.
        /// </summary>
        /// <remarks>
        /// While this check is technically inclusive of the segment end 
        /// points, floating point errors can result in end point intersection 
        /// being missed.  If this matters,  a 
        /// <see 
        /// cref="Vector2Util.SloppyEquals(float, float, float, float, float)">
        /// SloppyEquals</see> or similar test of  the intersection point can 
        /// be performed to check for this issue.
        /// </remarks>
        /// <param name="ax">The x-value for point A on line AB.</param>
        /// <param name="ay">The y-value for point A on line AB.</param>
        /// <param name="bx">The x-value for point B on line AB.</param>
        /// <param name="by">The y-value for point B on line AB.</param>
        /// <param name="cx">The x-value for point C on line CD.</param>
        /// <param name="cy">The y-value for point C on line CD.</param>
        /// <param name="dx">The x-value for point D on line CD.</param>
        /// <param name="dy">The y-value for point D on line CD.</param>
        /// <param name="outIntersectionPoint">If the lines intersect at
        /// a single point, the vector will represent the point of 
        /// intersection.</param>
        /// <returns>The relationship between lines AB and CD.</returns>
        public static LineRelType GetRelationship(float ax, float ay
                , float bx, float by
                , float cx, float cy
                , float dx, float dy
                , out Vector2 outIntersectionPoint)
        {
            
            float deltaAxBx = bx - ax;    
            float deltaAyBy = by - ay;
            
            float deltaCxDx = dx - cx;
            float deltaCyDy = dy - cy;
            
            float deltaCyAy = ay - cy;
            float deltaCxAx = ax - cx;    
            
            float numerator = 
                (deltaCyAy * deltaCxDx) - (deltaCxAx * deltaCyDy);
            float denominator = 
                (deltaAxBx * deltaCyDy) - (deltaAyBy * deltaCxDx);

            // Exit early if the lines do not intersect at a single point.
            if (denominator == 0)
            {
                outIntersectionPoint = Vector2.zero;
                if (numerator == 0)
                    return LineRelType.Collinear;
                return LineRelType.Parallel;
            }

            // Lines definitely intersect at a single point.
            
            float factorAB = numerator / denominator;
            float factorCD = 
                ((deltaCyAy * deltaAxBx) - (deltaCxAx * deltaAyBy))
                    / denominator;

            outIntersectionPoint = new Vector2(ax + (factorAB * deltaAxBx)
                        , ay + (factorAB * deltaAyBy));            

            // Determine the type of intersection
            if ((factorAB >= 0.0f) 
                && (factorAB <= 1.0f) 
                && (factorCD >= 0.0f) 
                && (factorCD <= 1.0f))
            {
                return LineRelType.SegmentsIntersect;
            }
            else if ((factorCD >= 0.0f) && (factorCD <= 1.0f))
            {
                return LineRelType.ALineCrossesBSeg;
            }
            else if ((factorAB >= 0.0f) && (factorAB <= 1.0f))
            {
                return LineRelType.BLineCrossesASeg;
            }

            return LineRelType.LinesIntersect;
            
        }
    }
}
