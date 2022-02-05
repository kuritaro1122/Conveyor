using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MetalLingConveyor : Conveyor {
    [Header("--- MetalLingConveyor ---")]
    [SerializeField] LaneParts lane_right;
    [SerializeField] LaneParts lane_right_edge;
    [SerializeField] LaneParts lane_rightAndUp;
    [SerializeField] Vector3 laneOffset = Vector3.forward;
    [SerializeField] List<GameObject> lanesList = new List<GameObject>();
    [Header("--- MetalLing ---")]
    [SerializeField] GameObject metalLing = null;
    [SerializeField] MetalLingInfo[] lingInfos;
    [SerializeField] List<GameObject> lingsList = new List<GameObject>();
    [Header("--- Other ---")]
    [SerializeField] OtherInfo[] others = null;
    [System.Serializable]
    private class OtherInfo {
        [SerializeField] Transform obj;
        [SerializeField] float distance;
        public void Update(Conveyor conveyor) {
            if (obj == null) return;
            this.obj.position = conveyor.GetElementPosition(this.distance);
        }
    }

    // Start is called before the first frame update
    void Start() {
        InitObject();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
        for (int i = 0; i < lingInfos.Length; i++) {
            if (lingsList.Count < this.lingInfos.Length)
                lingsList.Add(Instantiate(this.metalLing, this.transform));
            lingsList[i].transform.position = base.GetElementPosition(this.lingInfos[i].distance);
        }
        if (this.others != null) foreach (var o in this.others)
            o.Update(this);
    }

    public void InitObject() {
        ClearObject();
        Vector3 prevDirection = base.GetPoint(base.GetPointsNum() - 1) - base.GetPoint(base.GetPointsNum() - 2).normalized;
        for (int i = 1; i < base.GetPointsNum(); i++) {
            Vector3 pos = base.GetPoint(i - 1);
            Vector3 direction = base.GetPoint(i) - pos;
            float distance = direction.magnitude;
            direction.Normalize();
            prevDirection.Normalize();
            float _distance = this.lane_rightAndUp.Horizontal / 2f;
            for (int k = 0; (k + 1) * this.lane_right.Horizontal + this.lane_rightAndUp.Horizontal < distance; k++) {
                Vector3 _pos = pos + direction * (this.lane_rightAndUp.Horizontal / 2f + this.lane_right.Horizontal * k);
                Quaternion _rot = Quaternion.LookRotation(direction, Vector3.forward)
                        * Quaternion.LookRotation(this.lane_right.forward, this.lane_right.up);
                this.lanesList.Add(lane_right.Instantiate(Local(_pos + this.laneOffset), Local(_rot), this.transform));
                _distance += this.lane_right.Horizontal;
            }
            if (_distance + this.lane_rightAndUp.Horizontal / 2f < distance) {
                Vector3 _pos = pos + direction * (distance - 2 * this.lane_right.Horizontal);
                Quaternion _rot = Quaternion.LookRotation(direction, Vector3.forward)
                    * Quaternion.LookRotation(this.lane_right.forward, this.lane_right.up); //謎
                GameObject g = this.lane_right.Instantiate(Local(_pos + this.laneOffset), Local(_rot), this.transform);
                g.name += "_omake";
                lanesList.Add(g);
            }
            if (i > 1) {
                Quaternion __rot = Quaternion.LookRotation(Mathf.Sign(Vector3.SignedAngle(prevDirection, direction, Vector3.forward)) * (prevDirection + direction), Vector3.forward)
                    * Quaternion.LookRotation(this.lane_rightAndUp.forward, this.lane_rightAndUp.up)
                    * Quaternion.Euler(0, 0, -45f);
                this.lanesList.Add(this.lane_rightAndUp.Instantiate(Local(pos + this.laneOffset), Local(__rot), this.transform));
            }
            prevDirection = direction;
        }
        if (base.GetIsLoop()) {
            Vector3 _prevDirection = base.GetPoint(base.GetPointsNum() - 1) - base.GetPoint(base.GetPointsNum() - 2);
            Vector3 pos = base.GetPoint(0);
            Vector3 direction = base.GetPoint(1) - pos;
            direction.Normalize();
            _prevDirection.Normalize();
            Quaternion rot = Quaternion.LookRotation(Mathf.Sign(Vector3.SignedAngle(_prevDirection, direction, Vector3.forward)) * (_prevDirection + direction), Vector3.forward)
                * Quaternion.LookRotation(this.lane_rightAndUp.forward, this.lane_rightAndUp.up)
                * Quaternion.Euler(0, 0, -45f);
            this.lanesList.Add(this.lane_rightAndUp.Instantiate(Local(pos + this.laneOffset), Local(rot), this.transform));
        } else {
            //始点
            Vector3 pos1 = base.GetPoint(0);
            Vector3 direction1 = base.GetPoint(1) - pos1;
            Quaternion rot1 = Quaternion.LookRotation(-direction1, Vector3.forward)
                * Quaternion.LookRotation(this.lane_right_edge.forward, this.lane_right_edge.up);
            this.lanesList.Add(this.lane_right_edge.Instantiate(Local(pos1 + this.laneOffset), Local(rot1), this.transform));
            //終点
            Vector3 pos2 = base.GetPoint(base.GetPointsNum() - 1);
            Vector3 direction2 = base.GetPoint(base.GetPointsNum() - 2) - pos2;
            Quaternion rot2 = Quaternion.LookRotation(-direction2, Vector3.forward)
                * Quaternion.LookRotation(this.lane_right_edge.forward, this.lane_right_edge.up);
            this.lanesList.Add(this.lane_right_edge.Instantiate(Local(pos2 + this.laneOffset), Local(rot2), this.transform));
        }
    }

    public void ClearObject() {
        foreach (var l in this.lanesList) DestroyImmediate(l);
        this.lanesList.Clear();
    }

    [System.Serializable]
    private class LaneParts {
        [SerializeField] GameObject parts = null;
        [SerializeField, Min(0)] float horizontal = 1f;
        [SerializeField, Min(0)] float vertical = 1f;
        public float Horizontal { get { return this.scale.x * this.horizontal; } }
        public float Vertical { get { return this.scale.y * this.vertical; } }
        //[SerializeField] public Vector3 center = Vector3.zero;
        [SerializeField] public Vector3 forward = Vector3.forward;
        [SerializeField] public Vector3 up = Vector3.up;
        [SerializeField] public Vector3 scale = Vector3.one;
        public GameObject Instantiate(Vector3 pos, Quaternion rot, Transform parent = null) {
            GameObject obj = MonoBehaviour.Instantiate(this.parts, pos, rot, parent);
            obj.transform.localScale = this.scale;
            return obj;
        }
    }

    [System.Serializable]
    private class MetalLingInfo {
        [SerializeField] public float distance = 10f;
        [SerializeField] public float size = 0f;
    }
}

[CustomEditor(typeof(MetalLingConveyor))]
public class MetalLingConvetorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        MetalLingConveyor cls = target as MetalLingConveyor;
        EditorGUILayout.LabelField("~~~ Object Controller ~~~");
        if (GUILayout.Button("Init Object!!")) cls.InitObject();
        if (GUILayout.Button("Clear Object!!")) cls.ClearObject();
    }
}