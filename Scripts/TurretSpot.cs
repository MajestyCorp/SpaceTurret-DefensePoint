using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crion
{
    public enum ETurretPrefab { Small, Medium, Large }
    public enum EEnemySide { None, Left, Right, ForwardAngle, UpAngle, Top, Bottom }
    public class TurretSpot : MonoBehaviour
    {
        public delegate void SelectHandler(TurretSpot spot);
        public static event SelectHandler OnSelect;//to show selected and non-selected spots

        public EEnemySide TargetSide { get { return targetSide; } }
        public float MaxAngle { get { return maxAngle; } }
        public UpgradeTurret Turret { get { return turret; } }
        public BotPriority BotPriority { get { return priority; } }
        public Vector3 OriginalPosition => transform.parent.position + localPosition;

        public Quaternion OriginalRotation => transform.parent.rotation * localRotation;
        public bool Interactable {
            get => collider != null && collider.enabled;
            set => SetInteractable(value);
        }

        [SerializeField, ReadOnly]
        private UpgradeTurret turret;

        private BotPriority priority = null;

        [SerializeField]
        private ETurretPrefab prefabType = ETurretPrefab.Small;
        [SerializeField]
        private EEnemySide targetSide = EEnemySide.None;
        [SerializeField]
        private float maxAngle = 90f;

        private ModuleController controller;
        private GameObject selectable = null;
        private bool isSelected = false;
        private GameObject demo = null;
        [SerializeField]
        private new SphereCollider collider = null;
        private Vector3 localPosition;
        private Quaternion localRotation;

        private void OnEnable()
        {
            OnSelect += OnSelected;
        }

        private void OnDisable()
        {
            OnSelect -= OnSelected;
        }

        private void SetInteractable(bool value)
        {
            if (collider != null)
                collider.enabled = value;
        }

        public void SetOffset(Vector3 value, Quaternion rotation, bool withCollider = true)
        {
            var delta = transform.position - value;

            transform.localPosition = transform.parent.InverseTransformPoint(value);

            if (turret != null)
                turret.transform.rotation = rotation;

            if (!withCollider && collider != null)
                collider.center = Quaternion.Inverse(transform.localRotation) * delta;//localPosition - transform.localPosition;

        }

        public void SwapWith(TurretSpot anotherSpot)
        {
            var turret1 = turret;
            var turret2 = anotherSpot.turret;

            if (turret1 != null)
                MoveTurret(turret1, anotherSpot);

            if(turret2 != null)
                MoveTurret(turret2, this);

            this.turret = turret2;
            anotherSpot.turret = turret1;
        }

        private void MoveTurret(UpgradeTurret movedTurret, TurretSpot targetSpot)
        {
            movedTurret.Weapon.Shoot(false);
            Destroy(movedTurret.GetComponent<SmartBot>());

            var movedTransform = movedTurret.transform;
            movedTransform.SetParent(targetSpot.transform);
            movedTransform.localPosition = Vector3.zero;
            movedTransform.localRotation = Quaternion.identity;

            targetSpot.turret = movedTurret;
            movedTurret.gameObject.AddComponent<SmartBot>();
        }

        public void ResetOffset()
        {
            transform.localPosition = localPosition;
            if (turret != null)
                turret.transform.localRotation = Quaternion.identity;
            if (collider != null)
                collider.center = Vector3.zero;
        }

        public void Init(ModuleController controller)
        {
            this.controller = controller;
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;

            //try to get turret
            turret = GetComponentInChildren<UpgradeTurret>();
            if (turret != null)
            {
                turret.Init();
                turret.ApplyAll();
                //apply auto mode
                turret.TurretSystem.track = TTrack.photon;
                turret.Weapon.PlayerStats = Campaign2.Instance.PlayerStats;

                //check bot priority
                priority = turret.GetComponent<BotPriority>();
                if (priority == null)
                {
                    priority = turret.gameObject.AddComponent<BotPriority>();
                    priority.SetDefaultValues();
                }

                //add smart bot
                turret.gameObject.AddComponent<SmartBot>();
            }
        }

        public void PlaceDemoTurret()
        {
            GameObject prefab = TurretTypes.Instance.GetPrefab(prefabType);
            demo = Instantiate(prefab, transform);
            demo.transform.localPosition = Vector3.zero;
            demo.transform.localRotation = Quaternion.identity;
        }

        public void ClearDemo()
        {
            if (demo != null)
                Destroy(demo);
        }

        private void OnSelected(TurretSpot spot)
        {
            if (spot == this)
            {
                ShowSelected();
            }
            else
            {
                if (selectable != null)
                    ShowSelectable();
            }
        }

        public void Select()
        {
            OnSelect?.Invoke(this);
        }

        public void Deselect()
        {
            OnSelect?.Invoke(null);
        }

        public void ShowEdit(bool show, bool showEmpty = false)
        {
            ClearSelectable();

            if(show)
            {
                isSelected = false;

                if(turret != null || showEmpty)
                    ShowSelectable();
            }
        }

        private void ClearSelectable()
        {
            if (selectable == null)
                return;

            selectable.SetActive(false);
            selectable.transform.SetParent(null);
            selectable = null;
            collider = null;
        }

        private void ShowSelectable()
        {
            if (isSelected && selectable != null)
                ClearSelectable();

            isSelected = false;

            if(selectable == null)
            {
                selectable = controller.GetNewSelect();
                selectable.transform.SetParent(this.transform);
                selectable.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                selectable.SetActive(true);
                if (selectable.TryGetComponent(out collider))
                    collider.enabled = true;
            }
        }

        private void ShowSelected()
        {
            if (!isSelected && selectable != null)
                ClearSelectable();

            isSelected = true;

            if (selectable == null)
            {
                selectable = controller.GetNewSelected();
                selectable.transform.SetParent(this.transform);
                selectable.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                selectable.SetActive(true);
                if (selectable.TryGetComponent(out collider))
                    collider.enabled = true;
            }
        }

        public void MountTurret(UpgradeTurret prefab)
        {
            BotPriority priority;
            if (turret != null)
            {
                turret.Weapon.Shoot(false);
                Destroy(turret.gameObject);
            }

            turret = Instantiate(prefab, transform);
            turret.name = prefab.name;
            turret.transform.localPosition = Vector3.zero;
            turret.Init();
            turret.ApplyAll();
            turret.TurretSystem.track = TTrack.photon; 
            turret.Weapon.PlayerStats = Campaign2.Instance.PlayerStats;
            turret.Weapon.DamageFactor = PowerWidget.Instance.DamageFactor;

            if (!turret.gameObject.activeSelf)
                turret.gameObject.SetActive(true);

            turret.gameObject.name = prefab.name;

            //check bot priority
            priority = turret.GetComponent<BotPriority>();
            if (priority == null)
            {
                priority = turret.gameObject.AddComponent<BotPriority>();
                priority.SetDefaultValues();
            }

            //add smart bot
            turret.gameObject.AddComponent<SmartBot>();
        }

        public void DestroyTurret()
        {
            turret.Weapon.Shoot(false);
            Destroy(turret.gameObject);
            turret = null;
        }

        public void MountTurret(UpgradeTurret prefab, TurretState2 state)
        {
            MountTurret(prefab);
            turret.LoadDataFromState2(state);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (targetSide == EEnemySide.ForwardAngle)
            {
                Gizmos.DrawLine(transform.position, Quaternion.Euler(0f, maxAngle, 0f) * transform.forward * 30f + transform.position);
                Gizmos.DrawLine(transform.position, Quaternion.Euler(0f, -maxAngle, 0f) * transform.forward * 30f + transform.position);
            }
        }
#endif
    }
}
