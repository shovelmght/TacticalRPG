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
    [SerializeField] private GameObject _TrailParticleEffect;


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
    public Character _IncomingAttacker{ get; set; }
    public Team CurrentTeam { get; set; }
    public int UniqueID { get; private set; }

    public Attack _Attack;
    public Attack _SkillAttack;


    public event Action<int, int, int> OnHealthPctChange = delegate { };
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
        _IncomingAttacker = null;
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
            StartCoroutine(_gameManager.EndOfCharacterTurn(0.75f));
        }
        HaveMoved = true;
        _gameManager.Wait = false;
        
        if (!IsAI && !HaveAttacked && _gameManager.IsController)
        {
            _gameManager.SelectCharacter?.Invoke();
        }
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
    
    public IEnumerator Attack(Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection, Attack attack)
    {
        _gameManager.Wait = true;
        yield return new WaitForSeconds(1f);
        attack.DoAttack(this,tile, isAcounterAttack, attackDirection);
    }
    
    
    public IEnumerator ThrowProjectile(Vector3 endPositionProjectile, float delay, GameObject projectilePrefab, float MovementProjectilSpeed, AudioManager.SfxClass sfxAtSpawn)
    {
        Debug.Log("Character ThrowProjectile");
        yield return new WaitForSeconds(delay);
        if (sfxAtSpawn != null)
        {
            AudioManager._Instance.SpawnSound(sfxAtSpawn);
        }
        
        GameObject gameObjectProjectile = Instantiate(projectilePrefab, _StartPositionProjectile.position, _StartPositionProjectile.rotation);

        float speed = MovementProjectilSpeed;
        while (Vector3.Distance(gameObjectProjectile.transform.position, endPositionProjectile) > 0.1f)
        {
            gameObjectProjectile.transform.position = Vector3.MoveTowards(gameObjectProjectile.transform.position, endPositionProjectile, speed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObjectProjectile, 2);
        gameObjectProjectile.transform.GetChild(0).gameObject.SetActive(false);
        Hit();
    }
    
    public IEnumerator SpawnAttack(AudioManager.SfxClass sfxAtSpawn, GameObject spawnPrefab, Tile tile)
    {
        _tileManager.DeselectTiles();
        yield return new WaitForSeconds(0.75f);

        Instantiate(spawnPrefab, tile.Position, Quaternion.identity);
        HaveAttacked = true;
        _gameManager.Wait = false;
        _gameManager.ActivateUIButtonCharacter?.Invoke();
        _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
        InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
        yield return new WaitForSeconds(0.3f);
        
        if (sfxAtSpawn != null)
        {
            AudioManager._Instance.SpawnSound(sfxAtSpawn);
        }
        
        yield return new WaitForSeconds(0.75f);

        if (HaveMoved)
        {
            if (!_isCounterAttack)
            {
                StartCoroutine(_gameManager.EndOfCharacterTurn(0.75f));
            }
        }
        else
        {
            if (_gameManager.IsController)
            {
                _gameManager.SelectCharacter?.Invoke();
            }
        }
    }

    [ContextMenu("DeselectTile")]
    public void DeselectTile()
    {
        _tileManager.DeselectTiles();
    }
    
    public IEnumerator MoveAttack(AudioManager.SfxClass sfxAtSpawn, GameObject particleEffectPrefab, Tile tile)
    {
        if (sfxAtSpawn != null)
        {
            AudioManager._Instance.SpawnSound(sfxAtSpawn);
        }
        _TrailParticleEffect.SetActive(true);
        _tileManager.DeselectTiles();
        GameObject particleEffect = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.75f);

        yield return MoveTo(tile.Position, 0.05f);

     
        HaveAttacked = true;
        _gameManager.Wait = false;
        _gameManager.ActivateUIButtonCharacter?.Invoke();
        _gameManager.TileSelected.UnSetCharacter();
        tile.SetCharacter(GameManager.Instance.CurrentCharacter);
        _gameManager.TilePreSelected = tile;
        CurrentTile = tile;
        InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
        for (int i = 0; i < _gameManager.IndexOccupiedTiles; i++)
        {
            Character character = _gameManager.OccupiedTiles[i].CharacterReference;
            if(character == null) {continue;}
            
            if (character.GetIsBlock(character._attackDirection))
            {
                AudioManager._Instance.SpawnSound(  AudioManager._Instance._BlockSound);
                character.CharacterAnimator.SetTrigger(Block);
  
                character._isCounterAttack = _isCounterAttack;
                character.OnHealthPctChange(0, 0, 0);
                Debug.Log("Character IsAttacked Set _isCounterAttack = " + _isCounterAttack + " GO = " + gameObject.name + "StateAttackCharacter._Attack.IsProjectile =" + _gameManager.StateAttackCharacter._Attack.IsProjectile);
                if (!_gameManager.StateAttackCharacter._Attack.IsProjectile)
                {
                    CharacterAnimator.SetTrigger(HandUp);
                }
                
            }
            else
            {
                AudioManager._Instance.SpawnSound(_gameManager.StateAttackCharacter._Attack.ImpactSfx);
                character.IsAttacked(_gameManager.StateAttackCharacter._Attack.Power * Strength, _isCounterAttack);
            }
        }
        
        Destroy(particleEffect);
        HaveAttacked = true;
        _gameManager.Wait = false;
        _gameManager.ActivateUIButtonCharacter?.Invoke();
        _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
        InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
        
       
        yield return new WaitForSeconds(0.75f);
        
        _TrailParticleEffect.SetActive(false);
        if (HaveMoved)
        {
            if (!_isCounterAttack)
            {
                StartCoroutine(_gameManager.EndOfCharacterTurn(0.75f));
            }
        }
        else
        {
            if (_gameManager.IsController)
            {
                _gameManager.SelectCharacter?.Invoke();
            }
        }
    }

    [ContextMenu("RotateTest")]
    public void RotateTest()
    {
        StartCoroutine(RotateTo(_gameManager.TileSelected.Position));
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
        Debug.Log("Character Hit");
        if (_attackTarget)
        {
            if (_attackTarget.GetIsBlock(_attackTarget._attackDirection))
            {
                AudioManager._Instance.SpawnSound(  AudioManager._Instance._BlockSound);
                _attackTarget.CharacterAnimator.SetTrigger(Block);
  
                _attackTarget._isCounterAttack = _isCounterAttack;
                _attackTarget.OnHealthPctChange(0, 0, 0);
                Debug.Log("Character IsAttacked Set _isCounterAttack = " + _isCounterAttack + " GO = " + gameObject.name + "StateAttackCharacter._Attack.IsProjectile =" + _gameManager.StateAttackCharacter._Attack.IsProjectile);
                if (!_gameManager.StateAttackCharacter._Attack.IsProjectile)
                {
                    CharacterAnimator.SetTrigger(HandUp);
                }
                
            }
            else
            {
                AudioManager._Instance.SpawnSound(_gameManager.StateAttackCharacter._Attack.ImpactSfx);
                _attackTarget.IsAttacked(_gameManager.StateAttackCharacter._Attack.Power * Strength, _isCounterAttack);
            }

            if (_gameManager.CurrentCharacter != null)
            {
                _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
            }
            InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
        }
        else
        {
            AudioManager._Instance.SpawnSound(_gameManager.StateAttackCharacter._Attack.NoTargetSfx);
            HaveAttacked = true;
            _gameManager.Wait = false;
            _gameManager.ActivateUIButtonCharacter?.Invoke();
            _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
            InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
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
                StartCoroutine(_gameManager.EndOfCharacterTurn(0.75f));
            }
        }
        else
        {
            if (_gameManager.IsController)
            {
                _gameManager.SelectCharacter?.Invoke();
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
                yield return _tileManager.GetAttackTiles(_Attack.AttackLenght, null, CurrentTile, null, false);

                yield return new WaitForSeconds(.15f);
                bool canAttackIncomingAttacker = false;
                foreach (var occupiedTile in _gameManager.OccupiedTiles)
                {
                    if (occupiedTile != null)
                    {
                        if (occupiedTile.CharacterReference != null && occupiedTile.CharacterReference == _IncomingAttacker)
                        {
                            canAttackIncomingAttacker = true;
                            break;
                        }
                    }
                }

                if (!canAttackIncomingAttacker)
                {
                    _gameManager.Wait = false;
                    yield break;
                }
                GetAttackDirection.AttackDirection attackDirection = GetAttackDirection.SetAttackDirection(_gameManager.CurrentCharacter.transform.position, transform);
               
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
                if (!IsAI && _gameManager.IsController && !HaveMoved)
                {
                    _gameManager.SelectCharacter?.Invoke();
                }
            }
        }
        else
        {
            Debug.Log("Character CheckIfCanCounterAttack Set _gameManager.Wait(false) 111");
            _gameManager.Wait = false;
            if (!IsAI && _gameManager.IsController && !HaveMoved)
            {
                _gameManager.SelectCharacter?.Invoke();
            }
        }
    }
    
    private IEnumerator Vanish()
    {
        Character tempCurrentCharacter = _gameManager.CurrentCharacter;
        _gameManager.RemoveCharacter(this);
        CharacterAnimator.SetTrigger(Die);
        yield return StartCoroutine(MoveTo(transform.position + Vector3.up * DeathGapZPosition, 0.5f));
        CurrentTile.ActivateFloorParticleSystem();
        StartCoroutine(LerpScale(Vector3.zero, _lerpScalingSpeed));
        yield return StartCoroutine(MoveTo(transform.position + transform.TransformDirection(Vector3.forward) * ForwardDistanceWhenDie, DyingMoveSpeed));
        _gameManager.Wait = false;

        yield return MoveTo(transform.position + Vector3.down * DownDistanceWhenDie,DyingMoveSpeed);
        
        if (!IsAI == tempCurrentCharacter == this)
        {
            Debug.Log("Character _gameManager.CurrentCharacter == this");
            _gameManager.NextCharacterTurn();
        }

        RemoveUIPopUpCharacterInfo(true);

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

    private bool GetIsBlock(GetAttackDirection.AttackDirection _attackDirection)
    { 
        float randomChange = Random.Range(0f, 100.0f);
     
        switch (_attackDirection)
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
                Debug.Log("randomChange = " + randomChange + "   <=  _behindHitSuccesChance = " + behindHitSuccesChance);
                if (randomChange <= behindHitSuccesChance)
                {
                    return true;
                }
                break;
            default:
                
                float sideHitSuccesChance = 100 - _sideHitSuccesChance;
                Debug.Log("randomChange = " + randomChange + "   <=  _sideHitSuccesChance = " + sideHitSuccesChance);
                if ( randomChange <= sideHitSuccesChance)
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
