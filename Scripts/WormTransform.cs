using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crion
{
    public class WormTransform : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> transformList;

        [SerializeField]
        private float distance = 1.5f;

        [SerializeField, Range(0f, 50f)]
        private float moveSpeed = 10f;

        [SerializeField, Range(0f, 50f)]
        private float rotateSpeed = 10f;

        private List<WormCell> cellList = new List<WormCell>();
        private float deltaMove, deltaRotate;

        private void Awake()
        {
            for(int i=0;i<transformList.Count-1;i++)
            {
                cellList.Add(new WormCell(transformList[i], transformList[i + 1]));
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < cellList.Count; i++)
                cellList[i].Init(distance);
        }

        private void FixedUpdate()
        {
            int i;

            deltaMove = moveSpeed * Time.fixedDeltaTime;
            deltaRotate = rotateSpeed * Time.fixedDeltaTime;

            //for (i = cellList.Count-1; i >= 0; i--)
            for (i = 0; i < cellList.Count; i++)
                cellList[i].Calculate(deltaMove, deltaRotate, distance);

            for (i = 0; i < cellList.Count; i++)
                cellList[i].Apply();
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            for (int i = 0; i < cellList.Count; i++)
                cellList[i].Apply();
        }
#endif
    }

    public class WormCell
    {
        private Transform source, target;

        private Quaternion rotation;
        private Vector3 position;
        
        public WormCell(Transform source, Transform target)
        {
            this.source = source;
            this.target = target;
        }

        public void Init(float distance)
        {
            target.position = source.position -source.forward * distance;
            target.rotation = source.rotation;
            rotation = target.rotation;
            position = target.position;
        }

        public void Calculate(float deltaMove, float deltaRotate, float distance)
        {
            rotation = Quaternion.Lerp(rotation, source.rotation, deltaRotate);
            position = source.position - target.forward * distance;
        }

        public void Apply()
        {
            target.rotation = rotation;
            target.position = position;
        }
    }

}
