using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    [System.Serializable]
    public enum Team
    {
        None = 0,
        Team1 = 1,
        Team2 = 2,
        Team3 = 3
    }

    public ParticleSystem HitParticleSystem;
    public GameObject VCamLeft;
    public GameObject VCamRight;
    public GameObject VCamFront;
    public GameObject VCamBehind;
    [SerializeField] private CinemachineImpulseSource[] _CinemachineImpulseSources;

    [field: SerializeField] public int MaxHealth { get; private set; }  = 100;
    [field: SerializeField] public int Strength { get; private set; }  = 25;
    [field: SerializeField] public int MovementPoint { get; private set; } = 4;
    [field: SerializeField] public int AttackLenght { get; private set; } = 1;
    [field: SerializeField] public int Speed { get; private set; } = 41;
    [field: SerializeField] public Animator CharacterAnimator;
    [field: SerializeField] public float MovingSpeed { get; private set; } = 0.1f;
    [field: SerializeField] public float DyingMoveSpeed { get; private set; } = 1.0f;
    [field: SerializeField] public float WaitToDeselectedTiles { get; private set; } = 0.5f;
    [field: SerializeField] public float DeathGapZPosition { get; private set; } = 0.4f;

    [SerializeField] protected bool _RandomMaterial;
    [SerializeField] protected SkinnedMeshRenderer _SkinnedMeshRenderer;
    [SerializeField] protected Material[] _Materials;
    [SerializeField] private Transform _StartPositionProjectile;
    [SerializeField] private float _ProjectileSpeed = 10f;


    [Header("the smaller the value, the greater the speed")] [SerializeField]
    private int _rotationSpeed  = 10;

    [Header("the smaller the value, the greater the speed")] [SerializeField]
    private int _lerpScalingSpeed  = 5;


    [SerializeField] [Header("This should be not 0")]
    private float DownDistanceWhenDie  = 1;

    [SerializeField] [Header("This should be not 0")]
    private float ForwardDistanceWhenDie  = 0.6f;

    public float TurnTimeRemaining { get; protected set; } = 100;
    public Tile CurrentTile { get; set; }
    public bool HaveMoved { get; set; }
    public bool HaveAttacked { get; set; }
    public int CurrentHealth { get; set; }
    public bool IsAI { get; set; }
    public bool HaveCounterAbility { get; set; }
    public Team CurrentTeam { get; set; }
    public int UniqueID { get; private set; }

    [SerializeField] private Attack _Attack;


    public event Action<int, int, int> OnHealthPctChange = delegate { };
    public Action DestroyUI;
    public Action CounterAttack;
    public Action<bool, bool> ShowUIPopUpCharacterInfo;
    public Action<bool> RemoveUIPopUpCharacterInfo;
    public Action<int> ShowUIHitSuccess;
    
    protected GameManager _gameManager;
    protected TilesManager _tileManager;
    protected const float START_TIME_TURN = 100;

    [SerializeField] private float _frontHitSuccesChance  = 35;
    [SerializeField] private float _sideHitSuccesChance = 60;
    [SerializeField] private float _behindHitSuccesChance = 85f;

    public GetAttackDirection.AttackDirection _attackDirection;
    public Character _attackTarget;
    
    private bool _turn;
    public bool _isCounterAttack;
    private const float ROATION_TIME = 1;
    
    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Attack1 = Animator.StringToHash("Attack");
    private static readonly int TakeHit = Animator.StringToHash("TakeHit");
    private static readonly int Die = Animator.StringToHash("Die");
    private static readonly int Block = Animator.StringToHash("Block");
    private static readonly int HandUp = Animator.StringToHash("HandUp");

    protected virtual void Start()
    {
        _gameManager = GameManager.Instance;
        _tileManager = TilesManager.Instance;
        HitParticleSystem.startColor = Color.blue;
        CurrentHealth = MaxHealth;

        if (_RandomMaterial)
        {
            int randomMaterialIndex = Random.Range(0, _Materials.Length);
            _SkinnedMeshRenderer.material = _Materials[randomMaterialIndex];
        }
    }
    
    public virtual void ResetCharacterTurn()
    {
        HaveMoved = false;
        HaveAttacked = false;
        _gameManager.CurrentCharacter = this;
        _attackTarget = null;
        TurnTimeRemaining = START_TIME_TURN ;
    }

    public void SetMovementCharacter(Tile tile)
    {
        StartCoroutine(MoveCharacter(tile));
    }
    
    public void TurnCharacter(Tile tile)
    {
        StartCoroutine(RotateTo(tile.Position));
    }

    private IEnumerator MoveCharacter(Tile tile)
    {
        _gameManager.Wait = true;
        CharacterAnimator.SetBool(Move, true);
        StartCoroutine(RotateTo(tile.Position));
    
        for (int i = 0; i < tile.GetPreviousMoveTileLenght(); i++)
        {
            tile.SetTopMaterial(_tileManager.PathTileMaterial);
            yield return MoveTo(tile.PreviousMoveTilesList[i].Position, MovingSpeed);
        }
        
        StartCoroutine(MoveTo(tile.Position, MovingSpeed));
        CharacterAnimator.SetBool(Move, false);
        tile.SetTopMaterial(_tileManager.PathTileMaterial);
        yield return new WaitForSeconds(WaitToDeselectedTiles);
  
        _tileManager.DeselectTiles();
        _turn = false;
        CurrentTile = tile;
        if (HaveAttacked)
        {
            StartCoroutine(_gameManager.EndOfCharacterTurn(1.5f));
        }
        HaveMoved = true;
        _gameManager.Wait = false;
    }

    private IEnumerator MoveTo(Vector3 destination, float speed)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;
        while (elapsedTime < speed)
        {
            transform.position = Vector3.Lerp(startPosition, destination, elapsedTime / speed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = destination;
    }
    
    public IEnumerator Attack(Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        _gameManager.Wait = true;
        yield return new WaitForSeconds(1f);
        _Attack.DoAttack(this,tile, isAcounterAttack, attackDirection);
        
    }
    
    
    public IEnumerator ThrowProjectile(Vector3 endPositionProjectile, float delay, GameObject projectilePrefab)
    {
        yield return new WaitForSeconds(delay);
       GameObject gameObjectProjectile = Instantiate(projectilePrefab, _StartPositionProjectile.position, _StartPositionProjectile.rotation);

       float speed = _ProjectileSpeed;
       while (Vector3.Distance(gameObjectProjectile.transform.position, endPositionProjectile) > 0.1f)
       {
           gameObjectProjectile.transform.position = Vector3.MoveTowards(gameObjectProjectile.transform.position, endPositionProjectile, speed * Time.deltaTime);
           yield return null;
       }
       Destroy(gameObjectProjectile);
       Hit();
    }

    /*public IEnumerator RotateAndAttack(Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
    
 
        StartCoroutine(RotateTo(tile.Position));
        
        if (tile.CharacterReference)
        {
            _attackTarget = tile.CharacterReference;
            _attackTarget._attackDirection = attackDirection;
        }

        CharacterAnimator.SetTrigger(Attack1);
        Debug.Log("Character RotateAndAttack Set _isCounterAttack = " + isAcounterAttack + " GO = " + gameObject.name);
        _isCounterAttack = isAcounterAttack;
    }*/
    
    public IEnumerator RotateTo(Vector3 destinationTile)
    {
        _turn = true;
        Invoke(nameof(StopTurn), ROATION_TIME);
        while (_turn)
        {
            Vector3 relativePos = destinationTile - transform.position;
            relativePos = new Vector3(relativePos.x, 0, relativePos.z);
            Quaternion toRotation = Quaternion.LookRotation(relativePos);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
            if (relativePos == Vector3.zero)
            {
                break;
            }
            yield return null;
        }
    }

    private void StopTurn()
    {
        _turn = false;
    }

    private void IsAttacked(int damage, bool isAcounterAttack)
    {
        _gameManager.StartCinemachineImpulseSource();
        StartCinemachineImpulseSource();
        Debug.Log("Character IsAttacked Set _isCounterAttack = " + isAcounterAttack + " GO = " + gameObject.name);
        _isCounterAttack = isAcounterAttack;
        HitParticleSystem.startColor = Color.red;
            HitParticleSystem.Play();
            CurrentHealth -= damage;
            
            OnHealthPctChange(CurrentHealth, damage, MaxHealth);
            if (CurrentHealth <= 0)
            {
                StartCoroutine(Vanish());
            }
            else
            {
                CharacterAnimator.SetTrigger(TakeHit);
            }
    }

    //Impact Time when this character hit another gameObject
    //THIS METHODE IS CALLED BY ANIMATOR (ATTACK) 
    public void Hit()
    {
        if (_attackTarget)
        {
            if (GetIsBlock())
            {
                AudioManager._Instance.SpawnSound(  AudioManager._Instance._BlockSound);
                _attackTarget.CharacterAnimator.SetTrigger(Block);
                Debug.Log("Character IsAttacked Set _isCounterAttack = " + _isCounterAttack + " GO = " + gameObject.name);
                _attackTarget._isCounterAttack = _isCounterAttack;
                _attackTarget.OnHealthPctChange(0, 0, 0);
                CharacterAnimator.SetTrigger(HandUp);

            }
            else
            {
                AudioManager._Instance.SpawnSound(  AudioManager._Instance._SwordHit);
                _attackTarget.IsAttacked(Strength, _isCounterAttack);
            }
            
        }
        else
        {
            AudioManager._Instance.SpawnSound(  AudioManager._Instance._SwordSoft);
            HaveAttacked = true;
            _gameManager.Wait = false;
            Debug.Log("Character Hit Set _gameManager.Wait(false) 000");
        }
        
        if (_attackTarget && !_attackTarget.HaveCounterAbility)
        {
            Debug.Log("Character Hit Set _gameManager.Wait(false) 111");
            _gameManager.Wait = false;
        }
        if (HaveMoved)
        {
            if (!_isCounterAttack)
            {
                StartCoroutine(_gameManager.EndOfCharacterTurn(1.5f));
            }
        }
            
        _tileManager.DeselectTiles();
        HaveAttacked = true;
    }

    //THIS METHODE IS CALLED BY ANIMATOR (TakeHit/Block)
    public void Counter()
    {
        StartCoroutine(CheckIfCanCounterAttack());
    }

    private IEnumerator CheckIfCanCounterAttack()
    {
        RemoveUIPopUpCharacterInfo(false);
        Debug.Log("Character Counter check for _isCounterAttack = " + _isCounterAttack + " GO = " + gameObject.name);
        if (!_isCounterAttack)
        {
            if (HaveCounterAbility)
            {
                GetAttackDirection.AttackDirection attackDirection = GetAttackDirection.SetAttackDirection(_gameManager.CurrentCharacter.transform.position, transform);
                yield return new WaitForSeconds(0.25f);
                StartCoroutine(RotateTo(_gameManager.CurrentCharacter.transform.position));
                if (attackDirection != GetAttackDirection.AttackDirection.Front)
                {
                    yield return new WaitForSeconds(0.15f);
                }
                else
                { Debug.Log("Character Counter check for Is in front GO = " + gameObject.name);
                
                }
                CounterAttack();
            }
            else
            {
                Debug.Log("Character CheckIfCanCounterAttack Set _gameManager.Wait(false) 000");
                _gameManager.Wait = false;
            }
        }
        else
        {
            Debug.Log("Character CheckIfCanCounterAttack Set _gameManager.Wait(false) 111");
            _gameManager.Wait = false;
        }
    }
    
    private IEnumerator Vanish()
    {
        _gameManager.RemoveCharacter(this);
        CharacterAnimator.SetTrigger(Die);
        yield return StartCoroutine(MoveTo(transform.position + Vector3.up * DeathGapZPosition, 0.5f));
        CurrentTile.ActivateFloorParticleSystem();
        StartCoroutine(LerpScale(Vector3.zero, _lerpScalingSpeed));
        yield return StartCoroutine(MoveTo(transform.position + transform.TransformDirection(Vector3.forward) * ForwardDistanceWhenDie, DyingMoveSpeed));
        DestroyUI();
        _gameManager.Wait = false;
        yield return MoveTo(transform.position + Vector3.down * DownDistanceWhenDie,DyingMoveSpeed);
        Destroy(this.gameObject);
    }

    private IEnumerator LerpScale(Vector3 scaleWanted, float speed)
    {
        Vector3 scale = transform.localScale;
        float elapsedTime = 0;
        while (elapsedTime < speed)
        {
            transform.localScale = Vector3.Lerp(scale, scaleWanted, elapsedTime / speed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = scaleWanted;
    }

    private bool GetIsBlock()
    { 
        float randomChange = Random.Range(0f, 100.0f);
     
        switch (_attackTarget._attackDirection)
        {

            case GetAttackDirection.AttackDirection.None:
                break;
            case GetAttackDirection.AttackDirection.Front:

                float frontHitSuccesChance = 100 - _frontHitSuccesChance;
                Debug.Log("randomChange = " + randomChange + "   <=  _frontHitSuccesChance = " + frontHitSuccesChance);
        
                if ( randomChange <= frontHitSuccesChance)
                {
                    return true;
                }
                break;
            case GetAttackDirection.AttackDirection.Behind:
                
                float behindHitSuccesChance = 100 - _behindHitSuccesChance;
                Debug.Log("randomChange = " + randomChange + "   <=  _frontHitSuccesChance = " + behindHitSuccesChance);
                if ( Random.Range(0f, 100.0f) <= behindHitSuccesChance)
                {
                    return true;
                }
                break;
            default:
                
                float sideHitSuccesChance = 100 - _sideHitSuccesChance;
                Debug.Log("randomChange = " + randomChange + "   <=  _frontHitSuccesChance = " + sideHitSuccesChance);
                if ( Random.Range(0f, 100.0f) <= sideHitSuccesChance)
                {
                    return true;
                }
                break;
        }

        return false;
    }
    
    public float GetHitSuccess(GetAttackDirection.AttackDirection direction)
    {
        switch (direction)
        {
            case GetAttackDirection.AttackDirection.None:
                break;
            case GetAttackDirection.AttackDirection.Front:
                return _frontHitSuccesChance;

            case GetAttackDirection.AttackDirection.Behind:
                return _behindHitSuccesChance;

            default:
                return _sideHitSuccesChance;
        }

        return 0;
    }
    
    [ContextMenu("StartCinemachineImpulseSource")]
    public void StartCinemachineImpulseSource()
    {
        Debug.Log("StartCinemachineImpulseSource");
        foreach (var cinemachineImpulseSources in _CinemachineImpulseSources)
        {
            if (cinemachineImpulseSources.gameObject.activeInHierarchy)
            {
                cinemachineImpulseSources.GenerateImpulse();
            }
        }
        
    }

    public void ShowCharacterHitSuccess(int pct)
    {
        ShowUIPopUpCharacterInfo(true, false);
        ShowUIHitSuccess(pct);
    }

    public void SetRemaininTimeTurn()
    {
        TurnTimeRemaining -= Speed;
    }
}
