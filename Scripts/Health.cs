using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crion;

/// <summary>
/// It's a perfect script to describe how code was chanhed during 5 years lol
/// </summary>
public enum EDamageType { Hit, Explosion, Nuclear };
public enum EOwner { Player, Fighter, Frigate, Cruiser, Carrier, Suicider, Boss, Dropship, Worm }
public class Health : MonoBehaviour, IHealth
{

    [Tooltip("Can object receive damage?")]
    public bool isImmortal = false;
    public float maxHealth = 100.0f;
    public bool destroyOnDeath = true;
    public bool disableOnDeath = true;
    public bool resetHealthOnEnable = true;
    public float scale { get { return health / maxHealth; } }

    public short CurrentHealth2 { get { return (short)health; } set { SetHealth(value); } }
    public bool DeathTriggered { get { return deathTriggered; } }

    public byte PercentHealth { get { return pHealth; } set { SetHealth(value * maxHealth / 100); } }
    private byte pHealth;

    [SerializeField]
    [ReadOnly]
    private float health = 100f;

    /*
    public delegate void IndicatorHandler(Health sender, float ratio);
    public event IndicatorHandler OnHealthChanged;
    public event IndicatorHandler OnHealthInitiated;
    */

    protected bool deathTriggered;
    protected float lastDamage = 0f;

    public EDamageType LastDamageType { get; private set; }
    public EOwner LastDamageOwner { get; private set; }

    #region IHealth realization
    public float CurrentHealth => health;
    public float MaxHealth => maxHealth;
    public float Ratio => health / maxHealth;

    public event HealthRatioHandler OnHealthChanged;
    public event HealthRatioHandler OnHealthInitiated;
    public event HealthHandler OnDeath;
    #endregion

    // Use this for initialization
    public virtual void Start () {
        health = maxHealth;
        pHealth = 100;
	}

    public virtual int GetScores()
    { return 0; }

    public void SetHealth(float currentHealth)
    {
        if (currentHealth > maxHealth)
            health = maxHealth;
        else
            health = currentHealth;

        pHealth = (byte)(health * 100 / maxHealth);

        if (OnHealthChanged != null)
            OnHealthChanged.Invoke(this, scale);
    }

    public virtual void OnEnable()
    {
        lastDamage = 0f;
        deathTriggered = false;
        if (resetHealthOnEnable)
        {
            health = maxHealth;
            pHealth = 100;
            if (OnHealthInitiated != null)
                OnHealthInitiated.Invoke(this, scale);
        }
    }

    public virtual bool AddDamage(EOwner damageOwner, EDamageType damageType, float dmg, Collider collider = null)
    {
        if (isImmortal)
            return false;
        //return true if health lower or equal 0
        health -= dmg;
        pHealth = (byte)(health * 100 / maxHealth);

        if(health<=0)
        {
            health = pHealth = 0;
            lastDamage = dmg;
            LastDamageOwner = damageOwner;
            LastDamageType = damageType;

            if (deathTriggered)
                return false;
            else
                deathTriggered = true;

            if (OnHealthChanged != null)
                OnHealthChanged.Invoke(this, scale);

            //moved up for zorg script
            OnKilled(damageType == EDamageType.Nuclear);

            if (disableOnDeath)
                gameObject.SetActive(false);
            
            if (destroyOnDeath)
                Destroy(gameObject);
                
            return true;
        }

        if (OnHealthChanged != null)
            OnHealthChanged.Invoke(this, scale);

        return false;
    }

    public virtual void OnKilled(bool nuclear)
    { }
	
}
