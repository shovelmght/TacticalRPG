using System;
using System.Collections;
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
    public AudioSource AudioSource;
    public AudioClip SwordHit;
    public AudioClip SwordSoft;
    public AudioClip BlockSound;

    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public int Strength { get; private set; }
    [field: SerializeField] public int MovementPoint { get; private set; }
    [field: SerializeField] public int AttackLenght { get; private set; }
    [field: SerializeField] public int Speed { get; private set; }
    [field: SerializeField] public Animator CharacterAnimator;
    [field: SerializeField] public float MovingSpeed { get; private set; }
    [field: SerializeField] public float DyingMoveSpeed { get; private set; }
    [field: SerializeField] public float WaitToDeselectedTiles { get; private set; }
    [field: SerializeField] public float DeathGapZPosition { get; private set; }


    [Header("the smaller the value, the greater the speed")] [SerializeField]
    private int _rotationSpeed;

    [Header("the smaller the value, the greater the speed")] [SerializeField]
    private int _lerpScalingSpeed;


    [SerializeField] [Header("This should be not 0")]
    private float DownDistanceWhenDie;

    [SerializeField] [Header("This should be not 0")]
    private float ForwardDistanceWhenDie;

    public float TurnTimeRemaining { get; protected set; } = 100;
    public Tile CurrentTile { get; set; }
    public bool HaveMoved { get; set; }
    public bool HaveAttacked { get; set; }
    public int CurrentHealth { get; set; }
    public bool IsAI { get; set; }
    public bool HaveCounterAbility { get; set; }
    public Team CurrentTeam { get; set; }
    public int UniqueID { get; private set; }



    public event Action<int, int, int> OnHealthPctChange = delegate { };
    public Action DestroyUI;
    public Action CounterAttack;
    public Action<bool, bool> ShowUIPopUpCharacterInfo;
    public Action<bool> RemoveUIPopUpCharacterInfo;
    public Action<int> ShowUIHitSuccess;
    

    
    protected GameManager _gameManager;
    protected TilesManager _tileManager;
    protected const float START_TIME_TURN = 100;

    [SerializeField] private float _frontHitSuccesChance;
    [SerializeField] private float _sideHitSuccesChance;
    [SerializeField] private float _behindHitSuccesChance;

    private GetAttackDirection.AttackDirection _attackDirection;
    private Character _attackTarget;
    
    private bool _turn;
    private bool _isCounterAttack;
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
    
    public void Attack(Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        if (tile.CharacterReference)
        {
            _attackTarget = tile.CharacterReference;
            _attackTarget._attackDirection = attackDirection;
        }
        _gameManager.Wait = true;
        CharacterAnimator.SetTrigger(Attack1);
        StartCoroutine(RotateTo(tile.Position));
        _isCounterAttack = isAcounterAttack;
    }
    
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
                AudioSource.clip = BlockSound;
                _attackTarget.CharacterAnimator.SetTrigger(Block);
                _attackTarget._isCounterAttack = _isCounterAttack;
                _attackTarget.OnHealthPctChange(0, 0, 0);
                CharacterAnimator.SetTrigger(HandUp);

            }
            else
            {
                AudioSource.clip = SwordHit;
                _attackTarget.IsAttacked(Strength, _isCounterAttack);
            }
            
        }
        else
        {
            AudioSource.clip = SwordSoft;
            HaveAttacked = true;
            _gameManager.Wait = false;
        }
        
        if (_attackTarget && !_attackTarget.HaveCounterAbility)
        {
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
        AudioSource.Play();
        HaveAttacked = true;
    }

    //THIS METHODE IS CALLED BY ANIMATOR (TakeHit/Block)
    public void Counter()
    {
        RemoveUIPopUpCharacterInfo(false);
        
        if (!_isCounterAttack)
        {
            if (HaveCounterAbility)
            {
                CounterAttack();
            }
            else
            {
                _gameManager.Wait = false;
            }
        }
        else
        {
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
            case GetAttackDirection.AttackDirection.Font:

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
            case GetAttackDirection.AttackDirection.Side:
                
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
            case GetAttackDirection.AttackDirection.Font:
                return _frontHitSuccesChance;

            case GetAttackDirection.AttackDirection.Behind:
                return _behindHitSuccesChance;

            case GetAttackDirection.AttackDirection.Side:
                return _sideHitSuccesChance;
        }

        return 0;
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
