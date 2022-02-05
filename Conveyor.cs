using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Curve;

public class Conveyor : MonoBehaviour {
    [Header("--- Way ---")]
    [SerializeField] Vector2[] points = new Vector2[1];
    [SerializeField] bool loop = false;
    [SerializeField] bool turn = false;
    [SerializeField] bool reverse = false;
    private List<Vector3> _points = new List<Vector3>();
    private StraightLine straightLine;

    [Header("--- Element ---")]
    [SerializeField] float speed = 5f;
    [SerializeField] float waitTime;
    private float WayLength { get { return this.straightLine.GetCurveLength(); } }
    private float T { get { return (!turn ? 1 : 2) + this.waitTime / (this.WayLength / this.speed); } }
    private float t = 0f;
    private Vector3 pos;
    [Header("--- Gizmos ---")]
    [SerializeField] float pointSize = 0.5f;
    [SerializeField] float elementSize = 1f;

    public Vector3 GetPoint(int index) => this.straightLine.GetPoint(index);
    public bool GetIsLoop() => this.loop;
    public int GetPointsNum() => this.straightLine.GetPointsNum();
    public float GetCumulativeLength(int index) => this.straightLine.GetCumulativeLength(index);
    public float GetTotalLength() => this.straightLine.GetTotalLength();

    // Update is called once per frame
    protected virtual void Update() {
        t += (this.speed / this.WayLength) * Time.deltaTime;
        this.pos = GetElementPosition(0f);
    }

    protected virtual void OnValidate() {
        InitPoints();
    }

    protected virtual void Awake() {
        InitPoints();
    }

    public Vector3 GetElementPosition(float offsetDistance) {
        float offsetT = offsetDistance / WayLength;
        float _t = (this.t + offsetT) % this.T;
        if (!turn) return Local(GetPositonOnLine(Mathf.Clamp01(_t)));
        else {
            float __t = Mathf.Clamp(_t, 0f, 2f);
            if (__t < 1)
                return Local(GetPositonOnLine(__t));
            else return Local(GetPositonOnLine(2f - __t));
        }
    }
    private Vector3 GetPositonOnLine(float t) {
        return this.straightLine.GetPosition(!reverse ? t : 1 - t, true);
    }

    protected virtual void OnDrawGizmos() {
        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i < this.straightLine.GetPointsNum(); i++) {
            Vector3 p = this.straightLine.GetPoint(i);
            Gizmos.DrawWireSphere(Local(p), this.pointSize);
            if (i > 0) Gizmos.DrawLine(Local(p), Local(prevPoint));
            prevPoint = p;
        }
        Gizmos.color = Color.red;
        foreach (var p in this.points)
            Gizmos.DrawWireSphere(Local(p), this.pointSize * 1.2f);
        Gizmos.DrawWireSphere(this.pos, this.elementSize);
    }

    protected Vector3 Local(Vector3 localPos) => this.transform.TransformPoint(localPos);
    protected Quaternion Local(Quaternion localRot) => this.transform.rotation * localRot;

    private void InitPoints() {
        this._points.Clear();
        Vector2 prevPoint = Vector2.zero;
        for (int i = 0; i < this.points.Length; i++) {
            Vector2 p = this.points[i];
            if (i > 0) {
                int equal = 0;
                for (int k = 0; k < 2; k++)
                    if (p[k] == prevPoint[k]) equal++;
                if (equal == 0) this._points.Add(new Vector3(prevPoint.x, p.y));
                else if (equal == 2) continue;
            }
            this._points.Add(p);
            prevPoint = p;
        }
        if (loop) {
            Vector2 p = this.points[0];
            int equal = 0;
            for (int k = 0; k < 2; k++)
                if (p[k] == prevPoint[k]) equal++;
            if (equal == 0) this._points.Add(new Vector3(prevPoint.x, p.y));
            this._points.Add(p);
        }
        this.straightLine = new StraightLine(this._points.ToArray());
    }
}
