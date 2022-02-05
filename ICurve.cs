//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

namespace Curve {
    public interface ICurve {
        Vector3 GetPosition(float t, bool normalizedT = false);
        float GetCurveLength();
        int GetPointsNum();
        Vector3 GetPoint(int index);
    }
}