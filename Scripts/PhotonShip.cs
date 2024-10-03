using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// play animation controller on player, on client fake it
/// </summary>
public class PhotonShip : MonoBehaviour
{
    public SpaceShip SpaceShip { get { return spaceShip; } }
    public ObjectCursor ObjectCursor { get { return objectCursor; } }

    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private Rigidbody rigidbody = null;
    [SerializeField]
    private SpaceShip spaceShip = null;
    [SerializeField]
    private ObjectCursor objectCursor = null;
    [SerializeField]
    private ExplodeOnDeath explodeOnDeath = null;
    [SerializeField]
    private ChargeEngines chargeEngines = null;

    [Header("Photon data"), SerializeField]
    private bool calcSpeed = false;
    [SerializeField]
    private bool noSpeedLimit = false;
    [SerializeField]
    public bool isAnimated = false;

    public ushort shipId { get; private set; }
    public PhotonView View { get; private set; } = null;

    private bool isMine = false;
    private bool syncRotation = false;
    private Vector3 lastPosition;
    private float photonSpeed, sqrSpeed, dist;
    private Vector3 netPosition;
    private float angle;
    private Vector3 dir;

    //stuff is used on server side
    private Crion.ShipManager manager = null;
    private Crion.ShipData shipData = null;

    //sync stuff on clients
    private Vector3 m_NetPosition, m_NetPrevPosition;
    private Quaternion m_NetRotation;
    private Vector3 m_Direction;
    private float m_Speed;
    private bool m_isCharging = false;

    /// <summary>
    /// set charging on server
    /// </summary>
    public void StartCharge()
    {
        this.shipData.data |= 0x01;//set flag to charge    
    }

    //initiate ship data, is used on all clients
    public void Init(bool isMine, Crion.ShipManager manager, Crion.ShipData shipData)
    {
        this.isMine = isMine;
        this.manager = manager;
        this.shipData = shipData;
        spaceShip.IsShooting = shipData.shooting;
        shipId = shipData.shipId;
        objectCursor.ObjectId = shipId;
        View = manager.View;

        syncRotation = manager.shipPools[shipData.shipType].syncRotation;

        if(!isMine)
        {
            m_NetPrevPosition = m_NetPosition = shipData.pos;
            m_NetRotation = transform.rotation;
            m_Direction = transform.forward;
        }

        if (explodeOnDeath != null)
        {//disable physics for suicider on clients
            explodeOnDeath.triggerDeathOnCollision = isMine;
            explodeOnDeath.addExplosionImpulse = isMine;
            explodeOnDeath.addExplosionDamage = isMine;
        }
    }

    //convert current state to ShipData, is used on owner
    public void UpdateData()
    {
        shipData.shooting = spaceShip.IsShooting;
        shipData.health = spaceShip.PercentHealth;
        shipData.pos = transform.position;
        if (syncRotation)
            shipData.rotation = transform.rotation;
    }

    /// <summary>
    /// shipData was just serialized from photon server
    /// </summary>
    /// <param name="shipData"></param>
    public void Serialize(Crion.ShipData shipData)
    {
        spaceShip.IsShooting = shipData.shooting;
        m_NetPrevPosition = m_NetPosition;
        m_NetPosition = shipData.pos;

        //we can calc rotation, depending on prev network position
        dir = m_NetPosition - m_NetPrevPosition;
        dist = dir.magnitude;
        if (dist > 0.001f)
        {
            m_Direction = dir.normalized;
            m_NetRotation = Quaternion.LookRotation(m_Direction);
        }

        if (calcSpeed)
        {
            //calculate speed
            m_Speed = dist / 0.1f;

            if (!noSpeedLimit && m_Speed > spaceShip.Speed)
                m_Speed = spaceShip.Speed;
        }

        if (shipData.syncRotation)
            m_NetRotation = shipData.rotation;

        //update charge state on clients
        if(chargeEngines!=null)
        {
            if (!m_isCharging && ((shipData.data & 0x01) > 0))
            {
                chargeEngines.StartCharge();
                m_isCharging = true;
            }
            else if (m_isCharging)
                chargeEngines.EngineCheck();
        }

        if (shipData.health != spaceShip.PercentHealth)
            spaceShip.PercentHealth = shipData.health;
    }

    /// <summary>
    /// Sync ship every fixed update on clients
    /// </summary>
    /// <param name="shipData"></param>
    public void Sync()
    {
        //smoothy change speed
        if (calcSpeed)
        {
            if (Mathf.Abs(spaceShip.PhotonSpeed - m_Speed) < 0.1f)
                spaceShip.PhotonSpeed = m_Speed;
            else
                spaceShip.PhotonSpeed += (m_Speed - spaceShip.PhotonSpeed) * 0.1f;
        }

        UpdatePhotonPosition();

        //smooth rotate to target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation,
                    m_NetRotation, 0.2f);

        //transform.rotation = Quaternion.RotateTowards(transform.rotation,
        //        m_NetRotation,
        //        spaceShip.TurningSpeed * Time.fixedDeltaTime);

        //check attack state and shoot
        if (spaceShip.IsShooting)
            spaceShip.ReloadAndShoot();
    }

    private void UpdatePhotonPosition()
    {
        Vector3 newPosition;

        try
        {
            Vector3 extPosition = m_NetPosition + m_Direction * spaceShip.PhotonSpeed * manager.TimeSinceUpdate;

            newPosition = transform.position + (extPosition - transform.position)*0.2f;// * Time.fixedDeltaTime;

            /*
            if (Vector3.Distance(transform.position, extPosition) > spaceShip.Speed * 2 * totalTimeSinceUpdate)
                newPosition = extPosition;
            else
                newPosition = Vector3.MoveTowards(transform.position, extPosition,
                    spaceShip.PhotonSpeed * Time.fixedDeltaTime);
                    */

            transform.position = newPosition;
        }catch
        {
            //Debug.LogError("pos error: " + newPosition.ToString() + " speed: " + spaceShip.PhotonSpeed.ToString() + " totalTime = " + totalTimeSinceUpdate.ToString() +
            //    " =  ping " + manager.PingInSeconds.ToString() + " + delta server " + ((float)PhotonNetwork.Time - manager.SentServerTime).ToString(), gameObject);
        }
    }

    public void KillShip()
    {
        spaceShip.SetHealth(0);
        spaceShip.OnKilled(false);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!isAnimated)
        {
            spaceShip.isImmortal = true;
            animator.enabled = false;
            rigidbody.isKinematic = true;
            dir = Vector3.zero;
            lastPosition = netPosition = transform.position;
            m_NetRotation = transform.rotation;

            if (calcSpeed)
                objectCursor.predictBy = EPredictBy.PhotonSpeed;
            else
                objectCursor.predictBy = EPredictBy.PhotonConst;
            m_Speed = spaceShip.PhotonSpeed = spaceShip.Speed;

            m_isCharging = false;
        }
    }

    private void OnDisable()
    {
        if (isMine && manager != null)
        {
            manager.RemoveShip(this, shipData);
        }
    }

}
