using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crion;

/// <summary>
/// Еhis is another script that changes over 5 years
/// </summary>
public class SpaceShip : Health, IAimable
{
    public delegate void OnDeathHandler(EOwner shipType, EOwner killedBy);
    public static event OnDeathHandler OnDeath;

    [Header("Ship settings")]
    [ReadOnly]
    [SerializeField]
    protected int currentLevel = 0;
    [SerializeField]
    protected EOwner shipType;

    public EOwner ShipType { get { return shipType; } }

    public bool speedAtStart = true;

    [Range(0,1000)]
    public int scores = 10;

    public Transform target;
    public BountyProfile bountyProfile;

    [SerializeField]
    private float turningSpeed = 90f;
    [SerializeField]
    private float attackDistance = 70f;
    [SerializeField]
    private float attackRocketDistance = 150f;
    [SerializeField]
    private LevelAttr hull = new LevelAttr();
    [SerializeField]
    private LevelAttr speed = new LevelAttr();
    [SerializeField]
    private LevelAttr damage = new LevelAttr();
    [SerializeField]
    private LevelAttr rocketDamage = new LevelAttr();
    [SerializeField]
    [Tooltip("A random angle added to direction")]
    private LevelAttr accuracy = new LevelAttr();

    //properties
    public FlockGroup Group { get; set; }
    public float TurningSpeed { get { return turningSpeed; }  }
    public float AttackDistance { get { return attackDistance; } }
    public float AttackRocketDistance { get { return attackRocketDistance; } }
    public float Hull { get; protected set; }
    public float Speed { get; private set; }
    public float Damage { get; private set; }
    public float RocketDamage { get; private set; }
    public float Accuracy { get; private set; }
    public bool TurretShoot { get; set; } = false;

    public bool IsShooting { get; set; } = false;
    public float PhotonSpeed { get; set; }

    public bool IsZorg { get; set; } = false;

    #region IAimable interface
    Transform IAimable.Target { get => this.target; set => this.target = value; }

    Transform IAimable.Transform => transform;

    bool IAimable.TurretShoot { get => this.TurretShoot; }

    float IAimable.Damage => this.Damage;

    float IAimable.AttackDistance => attackDistance;

    float IAimable.RocketDamage => this.RocketDamage;

    float IAimable.AttackRocketDistance => attackRocketDistance;

    float IAimable.Accuracy => this.Accuracy;

    GameObject IAimable.Owner => gameObject;

    EOwner IAimable.DamageOwner => shipType;
    #endregion

    private List<BotWeapon> listWeapons = new List<BotWeapon>();
    private int i;
    private TrailRenderer[] trails = null;

    protected Rigidbody rb = null;
    protected bool hullWasSet = false;

    private float speedFactor;
    private bool weaponsInitiated = false;

    #region Animator funcs
    public BotWeapon GetFirstWeapon()
    {
        InitWeapons();
        if (listWeapons.Count > 0)
            return listWeapons[0];
        else return null;
    }
    public float GetMinAttackRange()
    {
        return Mathf.Min(attackDistance, attackRocketDistance);
    }
    public void ReloadAndShoot()
    {
        for (i = 0; i < listWeapons.Count; i++)
            listWeapons[i].ReloadAndShoot();
    }
    public Transform AssignRandomTarget()
    {
        this.target = Crion.LobbyManager.AllSpots[Random.Range(0, Crion.LobbyManager.AllSpots.Count)];
        return target;
    }

    public void MoveSmooth(Vector3 dir)
    {
        speedFactor = Mathf.Abs(Vector3.Dot(transform.forward, dir.normalized));
        rb.AddForce(transform.forward * Speed * speedFactor, ForceMode.Acceleration);
    }

    public void MoveForward()
    {
        rb.AddForce(transform.forward * Speed, ForceMode.Acceleration);
    }

    public void RotateToDirection(Quaternion dir)
    {
        rb.rotation = Quaternion.RotateTowards(rb.rotation, 
            dir, 
            turningSpeed * Time.fixedDeltaTime);
    }

    public void RotateToDirection(Vector3 dir)
    {
        rb.rotation = Quaternion.RotateTowards(rb.rotation, 
            Quaternion.LookRotation(dir), 
            turningSpeed * Time.fixedDeltaTime);
    }

    public void RotateToPoint(Vector3 point)
    {
        rb.rotation = Quaternion.RotateTowards(rb.rotation, 
            Quaternion.LookRotation(point - transform.position), 
            turningSpeed * Time.fixedDeltaTime);
    }
    #endregion

    public float GetHull()
    {
        return hull.GetCurrentValue(currentLevel);
    }

    public void SetHull(float value)
    {
        Hull = value;
        hullWasSet = true;
    }
    
    public void SetLevel(int level)
    {
        currentLevel = level;
    }

    protected virtual void UpdateWithLvl()
    {
        if(!hullWasSet)
            Hull = hull.GetCurrentValue(currentLevel);

        Speed = speed.GetCurrentValue(currentLevel);
        Damage = damage.GetCurrentValue(currentLevel);
        RocketDamage = rocketDamage.GetCurrentValue(currentLevel);
        Accuracy = accuracy.GetCurrentValue(currentLevel);

        maxHealth = Hull;
    }

    protected virtual void Awake()
    {
        InitWeapons();

        //get all trails
        trails = GetComponentsInChildren<TrailRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    private void InitWeapons()
    {
        if (weaponsInitiated)
            return;

        weaponsInitiated = true;

        BotWeapon[] array;
        array = GetComponentsInChildren<BotWeapon>(true);

        if (array != null)
            for (i = 0; i < array.Length; i++)
                if (!(array[i] is BotTurret))
                    listWeapons.Add(array[i]);
    }

    public override void OnEnable()
    {
        //init target
        if (target == null)
        {
            if (Crion.LobbyManager.Instance != null)
            {
                target = Crion.LobbyManager.EnemyTargets[Random.Range(0, Crion.LobbyManager.EnemyTargets.Count)];
            }
            else if(Crion.Campaign2.Instance!=null)
            {

            }
            else
            {
                PlayerController c = PlayerController.GetInstance();

                target = c.listEnemyTargets[Random.Range(0, c.listEnemyTargets.Count)];
            }
        }

        TurretShoot = false;
        UpdateWithLvl();

        if (speedAtStart)
        {
            PhotonSpeed = Speed;
            rb.velocity = transform.forward * Speed;
            rb.angularVelocity = Vector3.zero;
        } else
        {
            PhotonSpeed = 0f;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (trails != null)
            for (int i = 0; i < trails.Length; i++)
                trails[i].Clear();

        IsShooting = false;

        base.OnEnable();
    }

    public void CheckAndShoot()
    {
        for (i = 0; i < listWeapons.Count; i++)
            if (listWeapons[i].ReadyToShoot)
                listWeapons[i].Shoot();
    }

    public virtual void FastDeath(float damage)
    {
        PlayerController.ShipsDestroyed++;

        if (bountyProfile != null && DynamicSpawner.Profile == null)
        {
            bountyProfile.GivePleasure();
            PlayerController.RoundScores += scores;
            PlayerController.Scores += scores;
        }
        else
        {
            bountyProfile.GivePleasureDirect(DynamicSpawner.Profile.BountyMultiplier * PlayerController.GoldMultiplier);
            PlayerController.Scores += (int)(scores * DynamicSpawner.Profile.scoreMultiplier * (1f + 0.5f * PlayerController.Accuracy));

        }
    }

    public override void OnKilled(bool nuclear)
    {
        base.OnKilled(nuclear);
        PlayerController.ShipsDestroyed++;
        
        if (IsZorg)
        {
            WaveSpawner.DecZorg();
            IsZorg = false;
        }

        if (bountyProfile != null && DynamicSpawner.Profile == null && Crion.LobbyManager.Instance == null)
        {
            bountyProfile.GivePleasure();
            PlayerController.RoundScores += scores;
            PlayerController.Scores += scores;
        }
        else if(Crion.LobbyManager.Instance!=null)
        {
            bountyProfile.GivePleasurePhoton();
            //PlayerController.Scores += scores;
        } else
        {
            bountyProfile.GivePleasureDirect(DynamicSpawner.Profile.BountyMultiplier * PlayerController.GoldMultiplier);
            PlayerController.Scores += (int)(scores * DynamicSpawner.Profile.scoreMultiplier * (1f + 0.5f * PlayerController.Accuracy));

        }

        OnDeath?.Invoke(shipType, LastDamageOwner);
        //Debug.Log("Died " + shipType.ToString() + " from " + LastDamageOwner.ToString() + ", damage type: " + LastDamageType.ToString());

    }

    public override int GetScores()
    {
        return scores;
    }
}

[System.Serializable]
public class LevelAttr
{
    public float value;
    public float incPerLvl;
    public bool setLimit = false;
    public float limitValue;
    
    public float GetCurrentValue(int level)
    {
        float v;
        v = value + level * incPerLvl;

        if (setLimit)
            if (value < limitValue)
                v = Mathf.Min(value, limitValue);
            else
                v = Mathf.Max(value, limitValue);

        return v;
    }
}
