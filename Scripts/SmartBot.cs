using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crion
{
    public class SmartBot : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        protected BotStatus status = BotStatus.Idle;

        [SerializeField, Header("Settings")]
        private float minAngleToShoot = 5f;
        [SerializeField]
        private float raycastTime = 0.5f;
        [SerializeField]
        private int maxRaycasts = 3;
        [SerializeField]
        private LayerMask raycastLayer;
        [SerializeField]
        private float fighterScanDelay = 5f;

        [Header("Idle settings"), SerializeField]
        protected float scanDelay = 2f;
        [SerializeField]
        protected float minIdleTime = 3f;
        [SerializeField]
        protected float maxIdleTime = 10f;
        [SerializeField]
        protected float maxIdleYAngle = 90f;

        private Campaign2 campaign2;
        private SmartSpawner spawner;
        private ZTurretSystem turret;
        private ZWeapon weapon;
        private TurretSpot spot;
        private Transform playerShip;
        private float value;

        protected Timer timerIdle = new();
        protected Timer timerScan = new();
        protected Timer timerRaycast = new();

        [SerializeField, ReadOnly]
        private ISmartBotTarget targetShip;
        private Transform targetTransform, tempTransform, barrel;
        private Vector3 dist;
        [SerializeField, ReadOnly]
        private bool isShooting;
        [SerializeField, ReadOnly]
        private bool isVisible;
        private RaycastHit hit;
        private int raycastCounter = 0;

        protected List<IReadOnlyList<ISmartBotTarget>> priorityLists = new();
        private Dictionary<EOwner, int> shipTypeToPriority = new();
        private BotPriority botPriority; 
        private List<ISmartBotTarget> tempPriority = new List<ISmartBotTarget>();
        //private IReadOnlyList<SmartShip> tempList = new List<SmartShip>();

        private PowerWidget powerWidget;
        private ShipModule module;
        private Coroutine scanFighter = null;
        private float attackDistance, sqrAttackDistance;

        private void Awake()
        {
            raycastLayer = (LayerMask)(1 << 11) | (1 << 14);// LayerMask.NameToLayer("Station");

            campaign2 = Campaign2.Instance;
            spawner = SmartSpawner.Instance;

            playerShip = ModuleController.Instance.transform;
            spot = transform.parent.GetComponent<TurretSpot>();
            turret = GetComponent<ZTurretSystem>();
            weapon = GetComponent<ZWeapon>();
            barrel = turret.dummyBarrelModel.transform;
            botPriority = GetComponent<BotPriority>();

            if (weapon is ZWeaponLaser laser)
                attackDistance = laser.maxDistance;
            else
                attackDistance = 1000f;
            sqrAttackDistance = attackDistance * attackDistance;

            if (botPriority.ChangeMinAngle)
                minAngleToShoot = botPriority.MinAngleToShoot;
            botPriority.OnUpdate += OnPriorityUpdate;
            OnPriorityUpdate(botPriority.GetPriorityList());

            powerWidget = PowerWidget.Instance;

            module = GetComponentInParent<ShipModule>();
            if (module == null)
                this.enabled = false;
        }

        private void OnEnable()
        {
            weapon.DamageFactor = powerWidget.DamageFactor;
            GoToIdle();
        }

        private void OnPriorityUpdate(List<int> list)
        {
            priorityLists.Clear();
            shipTypeToPriority.Clear();

            for (int i=1;i<=list.Count;i++)
            {
                for(int j=0;j<list.Count;j++)
                    if(list[j]==i)
                    {
                        switch(j)
                        {
                            case 0:
                                shipTypeToPriority[EOwner.Fighter] = i;
                                priorityLists.Add(spawner.GetShipsByType(EOwner.Fighter));
                                break;
                            case 1:
                                shipTypeToPriority[EOwner.Frigate] = i;
                                priorityLists.Add(spawner.GetShipsByType(EOwner.Frigate));
                                break;
                            case 2:
                                shipTypeToPriority[EOwner.Cruiser] = i;
                                priorityLists.Add(spawner.GetShipsByType(EOwner.Cruiser));
                                break;
                            case 3:
                                shipTypeToPriority[EOwner.Carrier] = i;
                                priorityLists.Add(spawner.GetShipsByType(EOwner.Carrier));
                                break;
                            case 4:
                                shipTypeToPriority[EOwner.Suicider] = i;
                                priorityLists.Add(spawner.GetShipsByType(EOwner.Suicider));
                                break;
                            case 5:
                                shipTypeToPriority[EOwner.Worm] = i;
                                priorityLists.Add(spawner.GetShipsByType(EOwner.Worm));
                                break;
                            case 6:
                                shipTypeToPriority[EOwner.Boss] = i;
                                priorityLists.Add(spawner.GetShipsByType(EOwner.Boss));
                                break;
                        }
                        break;
                    }
            }

        }

        private void FixedUpdate()
        {
            if (module.IsDamaged)
            {
                turret.track = TTrack.off;
                if (weapon.IsShooting)
                    weapon.Shoot(false);
                return;
            } else
                turret.track = TTrack.photon;

            switch (status)
            {
                case BotStatus.Idle:
                    ProcessIdle();
                    break;
                case BotStatus.Aiming:
                    ProcessAiming();
                    break;
            }

            //do weapon cooling
            if (weapon != null)
            {
                value = powerWidget.CoolingRate * Time.fixedDeltaTime;//PlayerBase.coolingRateValue * 
                weapon.DoCooling(value);
            }
        }

        private void ProcessIdle()
        {
            if (timerIdle.IsFinished)
            {
                timerIdle.Activate(Random.Range(minIdleTime, maxIdleTime));

                //set rotation
                turret.TargetVector = Quaternion.Euler(0f, Random.Range(-maxIdleYAngle, maxIdleYAngle), 0f) *
                    Quaternion.Euler(Random.Range(-60f, 30f), 0f, 0f) *
                    turret.dummyBaseModel.transform.forward * 100f;
            }

            if (timerScan.IsFinished && campaign2.Mode == ECampaignMode.Battle)
            {
                timerScan.Activate(scanDelay * Random.Range(1f, 2f));

                //we found enemy? aim to priority target
                targetShip = GetPriorityTarget();
                if (targetShip != null)
                    GoToAim();
            }
        }

        private void ProcessAiming()
        {
            if(targetShip == null)
            {
                GoToIdle();
                return;
            }

            //probably targetShip cannot be null. only deactivated?
            if (!targetShip.IsTargetable)
            {//we killed this ship
                //check if it was fighter and aim next random fighter!
                if (targetShip.ShipType == EOwner.Fighter)
                {
                    tempTransform = targetShip.GetRandomBoid();
                    if (tempTransform != null)
                    {
                        targetShip = tempTransform.GetComponent<SmartShip>();
                        targetTransform = tempTransform;
                        turret.target = targetShip.GameObject;
                    }
                    else
                    {
                        //we killed all swarms - search next target
                        GoToIdle();
                        return;
                    }
                }
                else
                {
                    GoToIdle();
                    return;
                }
            }

            //we have target - aim to it
            if (weapon.GetWeaponType() != EWeaponType.Projectile)
            {//aim directly
                turret.TargetVector = targetTransform.position;
            }
            else
            {//aim to predict position
                turret.TargetVector =
                    targetShip.PredictPositionForBot(barrel.position, weapon.GetBulletSpeed());
            }

            dist = turret.TargetVector - barrel.position;
            isShooting = !weapon.IsOverheated && Vector3.Angle(barrel.forward, dist) < minAngleToShoot;

            if (isShooting && timerRaycast.IsFinished)
            {
                isVisible = CheckSeeTarget();
            }

            isShooting &= isVisible;

            if (weapon.IsShooting != isShooting)
                weapon.Shoot(isShooting);
        }

        private void GoToIdle()
        {
            status = BotStatus.Idle;
            timerScan.Activate(Random.Range(scanDelay / 2f, scanDelay));
            targetTransform = null;
            targetShip = null;
            if (weapon.IsShooting)
                weapon.Shoot(false);

            if (scanFighter != null)
                StopCoroutine(scanFighter);
        }

        private void GoToAim()
        {
            status = BotStatus.Aiming;
            targetTransform = targetShip.Transform;
            turret.target = targetShip.GameObject;
            timerRaycast.Activate(Random.Range(0.5f, 1f) * raycastTime);
            isVisible = false;

            if (targetShip.ShipType == EOwner.Fighter && scanFighter == null)
                scanFighter = StartCoroutine(ScanFighterTarget());
        }

        private IEnumerator ScanFighterTarget()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f) * fighterScanDelay);

                var target = GetPriorityTarget();

                if (target != null)
                {
                    var priority1 = 100;
                    if (targetShip != null)
                        shipTypeToPriority.TryGetValue(targetShip.ShipType, out priority1);

                    if (priority1 > shipTypeToPriority[target.ShipType])
                    {
                        scanFighter = null;
                        targetShip = target;
                        GoToAim();
                        break;
                    }

                }
            }
        }

        private bool CheckSeeTarget()
        {
            bool raycast;
            timerRaycast.Activate(raycastTime);

            if (dist.sqrMagnitude > sqrAttackDistance)
                return false;

            raycast = Physics.Raycast(barrel.position, turret.TargetVector - barrel.position, out hit, 150f, raycastLayer);
            if (raycast)
            {
                raycastCounter++;
                //we tried few times and target is hided - request another target
                if (raycastCounter > maxRaycasts)
                {
                    targetShip = GetPriorityTarget();
                    raycastCounter = 0;
                    if (targetShip == null)
                    {
                        //cant find enemy in angle
                        GoToIdle();
                    } else
                    {
                        if (targetShip.ShipType != EOwner.Fighter && scanFighter != null)
                            StopCoroutine(scanFighter);

                        GoToAim();
                    }
                }
            }
            else
            {
                raycastCounter = 0;
            }
            return !raycast;
        }

        private ISmartBotTarget GetPriorityTarget()
        {
            switch(spot.TargetSide)
            {
                case EEnemySide.ForwardAngle:
                    return PriorityAngle(spot.transform.position, spot.transform.forward, spot.MaxAngle);
                case EEnemySide.UpAngle:
                    return PriorityAngle(spot.transform.position, spot.transform.up, spot.MaxAngle);
                case EEnemySide.Left:
                    return PriorityAngle(playerShip.position, -playerShip.right, 90f);
                case EEnemySide.Right:
                    return PriorityAngle(playerShip.position, playerShip.right, 90f);
                case EEnemySide.Top:
                    return PriorityAngle(playerShip.position, playerShip.up, 90f);
                case EEnemySide.Bottom:
                    return PriorityAngle(playerShip.position, -playerShip.up, 90f);
                case EEnemySide.None:
                    return PriorityRandom();
                default:
                    return null;
            }
        }

        private ISmartBotTarget PriorityAngle(Vector3 pos, Vector3 dir, float angle)
        {
            for (int i = 0; i < priorityLists.Count; i++)
            {
                tempPriority.Clear();
                var list = priorityLists[i];

                for (int j = 0; j < list.Count; j++)
                {
                    var item = list[j];
                    if (item.IsTargetable && Vector3.Angle(dir, (item.Transform.position - pos)) <= angle)
                        tempPriority.Add(item);
                }

                if (tempPriority.Count > 0)
                    return tempPriority[Random.Range(0, tempPriority.Count)];
            }

            return null;
        }

        private ISmartBotTarget PriorityRandom()
        {
            for (int i = 0; i < priorityLists.Count; i++)
            {
                if (priorityLists[i].Count > 0)
                {
                    tempPriority.Clear();
                    var list = priorityLists[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        var item = list[j];
                        if (item.IsTargetable)
                            tempPriority.Add(item);
                    }

                    if (tempPriority.Count > 0)
                        return tempPriority[Random.Range(0, tempPriority.Count)];
                }
            }

            return null;
        }

        private void OnDisable()
        {
            if (weapon.IsShooting)
                weapon.Shoot(false);
        }

        private void OnDestroy()
        {
            if(botPriority != null)
                botPriority.OnUpdate -= OnPriorityUpdate;
        }
    }
}
