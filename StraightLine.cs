//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System;

namespace Curve {
    //[System.Serializable]
    public struct StraightLine : ICurve {
        private readonly Vector3[] points;
        private readonly float[] cumulativeLength;
        private readonly float totalLength;

        public float GetCumulativeLength(int index) => cumulativeLength[index];
        public float GetTotalLength() => this.totalLength;

        private float LengthPerP(int i) => (float)i / (points.Length - 1);
        private float CumulativeLengthPerc(int index) => cumulativeLength[index] / totalLength;
        private float CumulativeDistancePerc(int index0, int index1) => CumulativeLengthPerc(index1) - CumulativeLengthPerc(index0);

        public StraightLine(params Vector3[] points) {
            this.points = new Vector3[points.Length];
            this.cumulativeLength = new float[points.Length];
            for (int i = 0; i < points.Length; i++) this.points[i] = points[i];

            for (int i = 1; i < this.points.Length; i++) { //length[0] = 0f
                float length = Vector3.Distance(this.points[i], this.points[i - 1]);
                cumulativeLength[i] += cumulativeLength[i - 1] + length;
            }
            totalLength = cumulativeLength[cumulativeLength.Length - 1];
        }

        public Vector3 GetPosition(float t, bool normalizeT = false) {
            if (!normalizeT) t /= totalLength;
            float T = 0f;
            for (int i = 0; i < points.Length - 1; i++) {
                if (t > CumulativeLengthPerc(i) || i == 0)
                    T = LengthPerP(i) + (LengthPerP(i + 1) - LengthPerP(i)) / CumulativeDistancePerc(i, i + 1) * (t - CumulativeLengthPerc(i));
                else break;
            }
            Vector3 P = Vector3.zero;
            for (int i = 0; i < points.Length - 1; i++) {
                if (T > LengthPerP(i) || i == 0)
                    P = points[i] + (points[i + 1] - points[i]) / LengthPerP(1) * (T - LengthPerP(1) * i);
                else break;
            }
            return P;
        }

        public float GetCurveLength() => this.totalLength;
        public int GetPointsNum() => this.points.Length;
        public Vector3 GetPoint(int index) => this.points[index];
    }
}