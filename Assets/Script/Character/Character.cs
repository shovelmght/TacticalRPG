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
    public ParticleSystem[] _ElementSwordParticleEffect;
    public ParticleSystem[] _ElemenCharactertParticleEffect;
    public GameObject _ElemenFireCharactertParticleEffect;
    public GameObject _HitFireParticleEffect;
    public DataCharacterSpawner.CharacterAbility BaseAbility1;
    public DataCharacterSpawner.CharacterAbility BaseAbility2;

    [field: SerializeField] public int MaxHealth { get;  set; } = 100;
    [field: SerializeField] public int Strength { get;  set; } = 2;
    [field: SerializeField] public int MovementPoint { get;  set; } = 4;
    [field: SerializeField] public int Speed { get;  set; } = 41;
    [field: SerializeField] public Animator CharacterAnimator;
    [field: SerializeField] public float MovingSpeed { get; private set; } = 0.2f;
    [field: SerializeField] public float DyingMoveSpeed { get; private set; } = 1.0f;
    [field: SerializeField] public float WaitToDeselectedTiles { get; private set; } = 0.5f;
    [field: SerializeField] public float DeathGapZPosition { get; private set; } = 0.4f;

    [SerializeField] protected bool _RandomMaterial;
    [SerializeField] protected SkinnedMeshRenderer _SkinnedMeshRenderer;
    [SerializeField] protected Material[] _Materials;
    [SerializeField] private Transform _StartPositionProjectile;
    [SerializeField] private Transform PotionHandPostion;
    [SerializeField] private GameObject _TrailParticleEffect;
    [SerializeField] private GameObject _DieFloorParticleEffect;
    [SerializeField] private GameObject _Potion;
    [SerializeField] private CinemachineImpulseSource[] _CinemachineImpulseSources;
    [SerializeField] private MeshRenderer[] _MeshRenderersTeamColor;
    [SerializeField] protected GameObject _PoisonParticleEffect;
    [SerializeField] private GameObject XpEarnPrefab;
    
    [Header("the smaller the value, the greater the speed")] [SerializeField]
    private int _rotationSpeed = 10;

    [Header("the smaller the value, the greater the speed")] [SerializeField]
    private int _lerpScalingSpeed = 5;


    [SerializeField] [Header("This should be not 0")]
    private float DownDistanceWhenDie = 1;

    [SerializeField] [Header("This should be not 0")]
    private float ForwardDistanceWhenDie = 0.6f;


    public Tile CurrentTile { get; set; }
    public float TurnTimeRemaining { get; protected set; } = 100;
    public DataCharacterSpawner.CharactersPrefab Class { get; set; }
    public bool HaveMoved { get; set; }
    public bool HaveAttacked { get; set; }
    public int CurrentHealth { get; set; }
    public int XPEarned { get; set; }
    public int NextXPLevel { get; set; }
    
    public int CurrentXP { get; set; }
    public int Level { get; set; }
    public bool IsAI { get; set; }

    public bool CanMove = true;
    
    public bool _CanSpawnParticle = true;
    public bool HaveCounterAbility { get; set; }
    public Character _IncomingAttacker { get; set; }
    public Character _CharacterRelated { get; set; }
    public Team CurrentTeam { get; set; }
    public int UniqueID { get; private set; }
    
    public DataCharacterSpawner.CharactersPrefab CharacterType { get; set; }

    public Attack _Attack;
    public Attack _SkillAttack;
    public Attack _WaterAttack;
    public Attack _PoisonAttack;
    public Attack _FireAttack;
    public bool _IsSelfDestruct;


    public event Action<int, int, int, bool> OnHealthPctChange = delegate { };
    public Action ActionCounterAttack;
    public Action<bool, bool> ActionShowUIPopUpCharacterInfo;
    public Action<bool> ActionRemoveUIPopUpCharacterInfo;
    public Action<int> ActionShowUIHitSuccess;
    public Action<int> SetXpEarned;
    public Action ActionDestroyCharacterRelated;
    public Action<bool> ActionShowHideHealthBar;
    public Action<string, string> ActionShowBuffDebuffPotionEffect;
    public Action<string> ActionStartDialogue;
    public Action<bool> ActionShowDialogueBubble;

    protected int _IsPoisoned;
    protected GameManager _gameManager;
    protected TilesManager _tileManager;
    protected const float START_TIME_TURN = 100;

    [SerializeField] private float _frontHitSuccesChance = 35;
    [SerializeField] private float _sideHitSuccesChance = 60;
    [SerializeField] private float _behindHitSuccesChance = 85f;

    public GetAttackDirection.AttackDirection _attackDirection{ get; set; }
    public Character _attackTarget{ get; set; }
    public bool _isCounterAttack{ get; set; }
    public bool _IsDead{ get; set; }
    public int _NbrRepeatAttack{ get; set; }
    
    private bool _SkipCounter;
    private bool _CanHit = false;
    private bool _turn;
    private bool ImmuneToFire;
    private bool ImmuneToPoison;
    protected bool InFire;
    private bool _DoOnceVanish;

    private const float ROATION_TIME = 1;

    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Attack1 = Animator.StringToHash("Attack");
    protected static readonly int TakeHit = Animator.StringToHash("TakeHit");
    private static readonly int Die = Animator.StringToHash("Die");
    private static readonly int Block = Animator.StringToHash("Block");
    private static readonly int HandUp = Animator.StringToHash("HandUp");
    private static readonly int GetPotion = Animator.StringToHash("GetPotion");
    private static readonly int Drink = Animator.StringToHash("Drink");
    private static readonly int Kneel = Animator.StringToHash("Kneel");

    public void SetCharacterColor(Material materail)
    {
        int indexMaterial = FBPP.GetInt("TeamColor");

        foreach (var characterMaterial in _MeshRenderersTeamColor)
        {
            characterMaterial.material = materail;
        }

    }

    protected virtual void Start()
    {
        _gameManager = GameManager.Instance;
        _tileManager = TilesManager.Instance;
        CurrentHealth = MaxHealth;

        if (_CanSpawnParticle)
        {
            HitParticleSystem.startColor = Color.blue;
            HitParticleSystem.Play();
        }

        if (_RandomMaterial)
        {
            int randomMaterialIndex = Random.Range(0, _Materials.Length);
            _SkinnedMeshRenderer.material = _Materials[randomMaterialIndex];
        }
    }

    public virtual void ResetCharacterTurn()
    {
        if (_IsPoisoned > 0)
        {
            StartCoroutine(DealPoisonDamage());
        }

        HaveMoved = false;
        HaveAttacked = false;
        _gameManager.CurrentCharacter = this;
        _attackTarget = null;
        _IncomingAttacker = null;
        TurnTimeRemaining = START_TIME_TURN;
    }

    protected IEnumerator DealPoisonDamage()
    {
        yield return new WaitForSeconds(0.35f);
        _IsPoisoned--;
        if (_IsPoisoned <= 0)
        {
            _PoisonParticleEffect.SetActive(false);
        }

        AudioManager._Instance.SpawnSound( AudioManager._Instance._PoisonDamage);
        HitParticleSystem.startColor = Color.green;
        HitParticleSystem.Play();
        CurrentHealth -= (MaxHealth / 8);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            if (IsAI)
            {
                if (_gameManager.CurrentCharacterTurn == this)
                {
                    _gameManager.NextCharacterTurn();
                }
               
            }

            StartCoroutine(Vanish());
        }
        else
        {
            _SkipCounter = true;
            CharacterAnimator.SetTrigger(TakeHit);
        }

        OnHealthPctChange(CurrentHealth, MaxHealth / 8, MaxHealth, true);
    }

    private IEnumerator StartFireParticleEffect()
    {
        _HitFireParticleEffect.SetActive(false);
        yield return new WaitForSeconds(0.001f);
        _HitFireParticleEffect.SetActive(true);
    }
    
    private void DealFireDamage()
    {

        StartCoroutine(StartFireParticleEffect());
        AudioManager._Instance.SpawnSound( AudioManager._Instance._FireDamage);
        HitParticleSystem.startColor = Color.red;
        HitParticleSystem.Play();
        CurrentHealth -= (MaxHealth / 4);
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            if (IsAI)
            {
                if (_gameManager.CurrentCharacterTurn != null && _gameManager.CurrentCharacterTurn == this)
                {
                    _gameManager.NextCharacterTurn();
                }
               
            }

            StartCoroutine(Vanish());
        }
        else
        {
            _SkipCounter = true;
            CharacterAnimator.SetTrigger(TakeHit);
        }

        OnHealthPctChange(CurrentHealth, MaxHealth / 4, MaxHealth, true);
    }
    
    private IEnumerator DealFireDamageWithDelay()
    {
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(StartFireParticleEffect());
        AudioManager._Instance.SpawnSound( AudioManager._Instance._FireDamage);
        HitParticleSystem.startColor = Color.red;
        HitParticleSystem.Play();
        CurrentHealth -= (MaxHealth / 4);
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            if (IsAI)
            {
                if (_gameManager.CurrentCharacterTurn != null && _gameManager.CurrentCharacterTurn == this)
                {
                    _gameManager.NextCharacterTurn();
                }
               
            }

            StartCoroutine(Vanish());
        }
        else
        {
            _SkipCounter = true;
            CharacterAnimator.SetTrigger(TakeHit);
        }

        OnHealthPctChange(CurrentHealth, MaxHealth / 4, MaxHealth, true);
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
            if (_gameManager._IsMapScene && _gameManager.IsController)
            {

            }
            else
            {
                tile.SetTopMaterial(_tileManager.PathTileMaterial);
            }

            yield return MoveTo(tile.PreviousMoveTilesList[i].Position, MovingSpeed);
        }

        Vector3 spawnPosition = tile.Position;
        if (tile.IsWater)
        {
            if (_gameManager._IsMapScene)
            {
                spawnPosition = tile.Position + new Vector3(0, 0.3f, 0);
            }
            else
            {
                spawnPosition = tile.Position + new Vector3(0, 0.1f, 0);
            }
        }
        else if (_gameManager._IsMapScene)
        {
            spawnPosition = tile.Position + new Vector3(0, 0.2f, 0);
        }

        StartCoroutine(MoveTo(spawnPosition, MovingSpeed));
        CharacterAnimator.SetBool(Move, false);

        if (_gameManager._IsMapScene && _gameManager.IsController)
        {

        }
        else
        {
            tile.SetTopMaterial(_tileManager.PathTileMaterial);
        }

        if (tile.IsPotionTile)
        {
            tile.PotionAnimator.SetTrigger(GetPotion);
        }

        yield return new WaitForSeconds(WaitToDeselectedTiles);

        _tileManager.DeselectTiles();
        _turn = false;
        CurrentTile = tile;
        if (HaveAttacked && !IsAI)
        {
            StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0.75f));
        }

        SetElementEffect(tile.IsWater, false);

        if (tile.IsPotionTile)
        {
            tile.IsPotionTile = false;
            AudioManager._Instance.SpawnSound(AudioManager._Instance._GetPotion);
            yield return new WaitForSeconds(1);
            CharacterAnimator.SetTrigger(Drink);
            StartCoroutine(_gameManager.ZoomBattleCamera(4));

            if (_Potion != null)
            {
                _Potion.SetActive(false);
                yield return new WaitForSeconds(0.001f);
                _Potion.SetActive(true);
                _Potion.transform.position = tile.PotionAnimator.gameObject.transform.GetChild(0).position;
                StartCoroutine(GameObjectMoveTo(_Potion, PotionHandPostion.position, 0.2f));
            }

            yield return new WaitForSeconds(2);
            SetPotionEffect(tile.PotionData.PotionType);
            AudioManager._Instance.SpawnSound(AudioManager._Instance._ShowBuffDebuffStats);
            yield return new WaitForSeconds(1);
            HaveMoved = true;
            _gameManager.Wait = false;

            if (!IsAI && !HaveAttacked && _gameManager.IsController)
            {
                _gameManager.SelectCharacter?.Invoke();
            }

            if (_gameManager._IsMapScene)
            {
                _gameManager.ShowPossibleMapMove(tile);
            }
        }
        else
        {
            HaveMoved = true;
            _gameManager.Wait = false;

            if (!IsAI && !HaveAttacked && _gameManager.IsController)
            {
                _gameManager.SelectCharacter?.Invoke();
            }

            if (_gameManager._IsMapScene)
            {
                _gameManager.ShowPossibleMapMove(tile);
            }
        }
    }

    private IEnumerator GameObjectMoveTo(GameObject go, Vector3 destination, float speed)
    {
        Vector3 startPosition = go.transform.position;
        float elapsedTime = 0;
        while (elapsedTime < speed)
        {
            go.transform.position = Vector3.Lerp(startPosition, destination, elapsedTime / speed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        go.transform.position = destination;
    }

    public IEnumerator MoveTo(Vector3 destination, float speed)
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
        attack.DoAttack(this, tile, isAcounterAttack, attackDirection);
    }


    public IEnumerator ThrowProjectile(Vector3 endPositionProjectile, float delay, GameObject projectilePrefab, float MovementProjectilSpeed,
        AudioManager.SfxClass sfxAtSpawn)
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
            gameObjectProjectile.transform.position =
                Vector3.MoveTowards(gameObjectProjectile.transform.position, endPositionProjectile, speed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObjectProjectile, 2);
        gameObjectProjectile.transform.GetChild(0).gameObject.SetActive(false);
        _CanHit = true;
        Hit();
    }

    public IEnumerator SpawnAttack(AudioManager.SfxClass preSfx, AudioManager.SfxClass sfxAtSpawn, GameObject spawnPrefab, Tile tile)
    {
        _tileManager.DeselectTiles();
        yield return new WaitForSeconds(0.75f);
        if (preSfx != null)
        {
            AudioManager._Instance.SpawnSound(preSfx);
        }

        _gameManager.LastSpawnTile = tile;
        Instantiate(spawnPrefab, tile.Position, Quaternion.identity);
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

        if (HaveMoved && !IsAI)
        {
            StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0.75f));
            
        }
        else
        {
            if (_gameManager.IsController)
            {
                _gameManager.SelectCharacter?.Invoke();
            }
        }

        HaveAttacked = true;
    }

    [ContextMenu("DeselectTile")]
    public void DeselectTile()
    {
        _tileManager.DeselectTiles();
    }

    public IEnumerator MoveAttack(AudioManager.SfxClass sfxAtSpawn,AudioManager.SfxClass noTargetSfx, AudioManager.SfxClass impactSfx,GameObject particleEffectPrefab, Tile tile)
    {
        if (sfxAtSpawn != null)
        {
            AudioManager._Instance.SpawnSound(sfxAtSpawn);
        }

        _TrailParticleEffect.SetActive(true);
        GameObject particleEffect = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.75f);
        yield return MoveTo(tile.Position, 0.05f);
        _gameManager.ActivateUIButtonCharacter?.Invoke();
        CurrentTile.UnSetCharacter();
        tile.SetCharacter(GameManager.Instance.CurrentCharacter);
        _gameManager.TilePreSelected = tile;
        CurrentTile = tile;
        InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
        for (int i = 0; i < tile.GetPreviousMoveTileLenght(); i++)
        {
            Character character = tile.PreviousMoveTilesList[i].CharacterReference;
            if (character == null || character == this)
            {
                continue;
            }

            Debug.Log("Character ::  MoveAttackGO = " + gameObject.name + "StateAttackCharacter._Attack.IsProjectile =" +
                      _gameManager.StateAttackCharacter._Attack.IsProjectile + "  charactarAttacked = " + character.gameObject.name + "  Tile = " +
                      tile.CoordX + " " + tile.CoordY);
            _attackTarget = character;
            character._IncomingAttacker = this;
            if (character.GetIsBlock(character._attackDirection))
            {
                AudioManager._Instance.SpawnSound(AudioManager._Instance._BlockSound);
                character.CharacterAnimator.SetTrigger(Block);

                character._isCounterAttack = _isCounterAttack;
                character.OnHealthPctChange(0, 0, 0, true);
            }
            else
            {
                AudioManager._Instance.SpawnSound(impactSfx);
                character.IsAttacked(_gameManager.StateAttackCharacter._Attack.Power * Strength, _isCounterAttack, _gameManager.StateAttackCharacter._Attack._IsFire, false);
            }
        }

        _tileManager.DeselectTiles();
        Destroy(particleEffect);
        _gameManager.ActivateUIButtonCharacter?.Invoke();
        _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
        InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
        if (tile.IsPotionTile)
        {
            tile.PotionAnimator.SetTrigger(GetPotion);
            AudioManager._Instance.SpawnSound(AudioManager._Instance._GetPotion);
        }
        AudioManager._Instance.SpawnSound(noTargetSfx);
        yield return new WaitForSeconds(0.75f);
        _TrailParticleEffect.SetActive(false);

        if (tile.IsPotionTile)
        {
            tile.IsPotionTile = false;
            yield return new WaitForSeconds(0.75f);
            CharacterAnimator.SetTrigger(Drink);
            StartCoroutine(_gameManager.ZoomBattleCamera(4));

            if (_Potion != null)
            {
                _Potion.SetActive(false);
                yield return new WaitForSeconds(0.001f);
                _Potion.SetActive(true);
                _Potion.transform.position = tile.PotionAnimator.gameObject.transform.GetChild(0).position;
                StartCoroutine(GameObjectMoveTo(_Potion, PotionHandPostion.position, 0.2f));
            }

            yield return new WaitForSeconds(2);
            SetPotionEffect(tile.PotionData.PotionType);
            AudioManager._Instance.SpawnSound(AudioManager._Instance._ShowBuffDebuffStats);
            yield return new WaitForSeconds(1);

        }

        if (_attackTarget == null || _attackTarget && !_attackTarget.HaveCounterAbility)
        {
            Debug.Log("Character :: Hit Set _gameManager.Wait(false) 111 _Attack = " + _gameManager.StateAttackCharacter._Attack.name +
                      ":: Character = " + gameObject.name);
            _gameManager.Wait = false;
        }

        if (HaveMoved && !IsAI)
        {
            if (!_isCounterAttack)
            {
                StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0.75f));
            }
        }
        else
        {
            if (_gameManager.IsController)
            {
                _gameManager.SelectCharacter?.Invoke();
            }
        }

        if (!_gameManager._IsMapScene)
        {
            SetElementEffect(tile.IsWater, false);
        }


        HaveAttacked = true;
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
        Vector3 relativePosEnd = destinationTile - transform.position;
        relativePosEnd = new Vector3(relativePosEnd.x, 0, relativePosEnd.z);
        Quaternion toRotationEnd = Quaternion.LookRotation(relativePosEnd);
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


        transform.rotation = toRotationEnd;
    }

    private void StopTurn()
    {
        _turn = false;
    }

    public void SetElementEffect(bool isWater, bool isSpawn)
    {
        if (isWater)
        {
            if (GameManager.Instance._tileManager.TileManagerData._IsLava && !ImmuneToFire)
            {
                if (isSpawn)
                {
                    StartCoroutine(DealFireDamageWithDelay());
                }
                else
                {
                    DealFireDamage();
                }

                _ElemenFireCharactertParticleEffect.SetActive(true);
                
                foreach (var elementSwordParticleEffect in _ElementSwordParticleEffect)
                {
                    elementSwordParticleEffect.gameObject.SetActive(true);
                    elementSwordParticleEffect.startColor = Color.red;
                }

                foreach (var elemenCharactertParticleEffect in _ElemenCharactertParticleEffect)
                {
                    elemenCharactertParticleEffect.gameObject.SetActive(true);
                    elemenCharactertParticleEffect.startColor = Color.red;
                }
            }
            else if (GameManager.Instance._tileManager.TileManagerData._IsPoison && !ImmuneToPoison)
            {
                StartCoroutine(AddPoisonEffect());
                if (!InFire)
                {
                    _ElemenFireCharactertParticleEffect.SetActive(false);
                
                    foreach (var elementSwordParticleEffect in _ElementSwordParticleEffect)
                    {
                        elementSwordParticleEffect.gameObject.SetActive(true);
                        elementSwordParticleEffect.startColor = Color.green;
                    }

                    foreach (var elemenCharactertParticleEffect in _ElemenCharactertParticleEffect)
                    {
                        elemenCharactertParticleEffect.gameObject.SetActive(true);
                        elemenCharactertParticleEffect.startColor = Color.green;
                    }
                }

            }
            else if(GameManager.Instance._tileManager.TileManagerData._IsWater)
            {
                if (!InFire)
                {
                    _ElemenFireCharactertParticleEffect.SetActive(false);
                    foreach (var elementSwordParticleEffect in _ElementSwordParticleEffect)
                    {
                        elementSwordParticleEffect.gameObject.SetActive(true);
                        elementSwordParticleEffect.startColor = Color.cyan;
                    }

                    foreach (var elemenCharactertParticleEffect in _ElemenCharactertParticleEffect)
                    {
                        elemenCharactertParticleEffect.gameObject.SetActive(true);
                        elemenCharactertParticleEffect.startColor = Color.cyan;
                    }
                }
            }
        }
        else
        {
            if (!InFire)
            {
                _ElemenFireCharactertParticleEffect.SetActive(false);
                foreach (var elementSwordParticleEffect in _ElementSwordParticleEffect)
                {
                    elementSwordParticleEffect.gameObject.SetActive(false);
                }
            }
            
            foreach (var elemenCharactertParticleEffect in _ElemenCharactertParticleEffect)
            {
                elemenCharactertParticleEffect.gameObject.SetActive(false);
            }
        }
    }

    protected void InvokeOnHealthPctChange(int currentHealth, int damage, int maxHealth, bool isDamage)
    {
        OnHealthPctChange(currentHealth, damage,maxHealth, isDamage);
    }


public void IsAttacked(int damage, bool isAcounterAttack, bool isFireAttack, bool isCritical)
    {
        Time.timeScale = 0.5f;
        Invoke(nameof(ResetTimeScale), 0.2f);
        _gameManager.StartCinemachineImpulseSource(isCritical);
        StartCinemachineImpulseSource();
        Debug.Log("Character :: IsAttacked Set _isCounterAttack = " + isAcounterAttack + " Character = " + gameObject.name);
        _isCounterAttack = isAcounterAttack;
        HitParticleSystem.startColor = Color.red;
        HitParticleSystem.Play();
        if (isFireAttack)
        {
            StartCoroutine(StartFireParticleEffect());
        }

        CurrentHealth -= damage;
        
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                StartCoroutine(Vanish());
            }
            else
            {
                CharacterAnimator.SetTrigger(TakeHit);
            }
            
            OnHealthPctChange(CurrentHealth, damage, MaxHealth, true);
    }


    private IEnumerator AddPoisonEffect()
    {
        if (ImmuneToPoison) {yield break;}
    
        AudioManager._Instance.SpawnSound( AudioManager._Instance._PoisonDamage);
        _IsPoisoned += 3;
        _PoisonParticleEffect.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        _PoisonParticleEffect.SetActive(true);
    }
    
    private void ResetTimeScale()
    {
        if (!_gameManager._GameIsFinish)
        {
            Time.timeScale = 1f;
        }
    }

    //Impact Time when this character hit another gameObject
    //THIS METHODE IS CALLED BY ANIMATOR (ATTACK) 
    public void Hit()
    {
        StartCoroutine(StartHit());
    }
    
    private IEnumerator StartHit()
    {
        if(_gameManager.StateAttackCharacter._Attack.IsWaterAttack && !_CanHit && !_isCounterAttack) {yield break;}

        _CanHit = false;
        
        Debug.Log("Character :: Hit  _Attack = " + _gameManager.StateAttackCharacter._Attack.name + ":: Character = " + gameObject.name);
        if (_attackTarget)
        {
            if (_attackTarget.GetIsBlock(_attackTarget._attackDirection))
            {
                AudioManager._Instance.SpawnSound(AudioManager._Instance._BlockSound);
                _attackTarget.CharacterAnimator.SetTrigger(Block);

                _attackTarget._isCounterAttack = _isCounterAttack;
                _attackTarget.OnHealthPctChange(0, 0, 0, true);
                Debug.Log("Character :: IsAttacked Set _isCounterAttack _Attack = " + _gameManager.StateAttackCharacter._Attack.name +
                          ":: Character = " + gameObject.name);
                if (!_gameManager.StateAttackCharacter._Attack.IsProjectile)
                {
                    CharacterAnimator.SetTrigger(HandUp);
                }

            }
            else
            {
                AudioManager._Instance.SpawnSound(_gameManager.StateAttackCharacter._Attack.ImpactSfx);
                Debug.Log("Chatacter :: Hit _gameManager.StateAttackCharacter._Attack = " + _gameManager.StateAttackCharacter._Attack.name +
                          ":: Character = " + gameObject.name);
                if (_attackTarget._attackDirection == GetAttackDirection.AttackDirection.Behind)
                {
                    _gameManager.ShowCriticalText();
                    _attackTarget.IsAttacked(_gameManager.StateAttackCharacter._Attack.Power * Strength * 2, _isCounterAttack,_gameManager.StateAttackCharacter._Attack._IsFire, true);
                }
                else
                {
                    _attackTarget.IsAttacked(_gameManager.StateAttackCharacter._Attack.Power * Strength, _isCounterAttack,_gameManager.StateAttackCharacter._Attack._IsFire, false);
                }
                
            }

            if (_gameManager.StateAttackCharacter._Attack._IsPoison)
            {
                StartCoroutine(_attackTarget.AddPoisonEffect());
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
            _gameManager.Wait = false;
            _gameManager.ActivateUIButtonCharacter?.Invoke();
            _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
            InputManager.Instance._TempSelectTileMaterial = _gameManager._tileManager.MoveTileMaterial;
            Debug.Log("Character :: Hit Set _gameManager.Wait(false) 000  _Attack = " + _gameManager.StateAttackCharacter._Attack.name + ":: Character = " + gameObject.name);
        }
        
        Debug.Log("Character :: Check RepeatableAttackInputIsPress = true;  _Attack = " + _gameManager.StateAttackCharacter._Attack.name + ":: Character = " + gameObject.name);
        if (_gameManager.RepeatableAttackInputIsPress && !_isCounterAttack && _NbrRepeatAttack < 1 && !IsAI && _gameManager.StateAttackCharacter._Attack.IsReapeatableAttack)
        {
            _NbrRepeatAttack++;
            CharacterAnimator.SetTrigger(Attack1);
            yield break;
        }

        _NbrRepeatAttack = 0;

        if (_attackTarget && !_attackTarget.HaveCounterAbility)
        {
            Debug.Log("Character :: Hit Set _gameManager.Wait(false) 111 _Attack = " + _gameManager.StateAttackCharacter._Attack.name + ":: Character = " + gameObject.name);
            _gameManager.Wait = false;
        }
        if (HaveMoved && !IsAI)
        {
            if (!_isCounterAttack)
            {
                StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0.75f));
            }
        }
        else
        {
            if (_gameManager.IsController)
            {
                yield return new WaitForSeconds(1);
                _gameManager.SelectCharacter?.Invoke();
            }
        }
            
        _tileManager.DeselectTiles();
        HaveAttacked = true;
    }

    //THIS METHODE IS CALLED BY ANIMATOR (TakeHit/Block)
    public void Counter()
    {
        if (!_SkipCounter && !_IsDead)
        {
            StartCoroutine(CheckIfCanCounterAttack());
        }
        else
        {
            _SkipCounter = false;
        }
    }

    private IEnumerator CheckIfCanCounterAttack()
    {
        ActionRemoveUIPopUpCharacterInfo(false);
        Debug.Log("Character :: Counter check for _isCounterAttack = " + _isCounterAttack + " GO = " + gameObject.name);
        if (!_isCounterAttack)
        {
            if (HaveCounterAbility && _IncomingAttacker != null && _IncomingAttacker._NbrRepeatAttack == 0)
            {
                _gameManager.IndexOccupiedTiles = 0;
                yield return _tileManager.GetAttackTiles(_Attack.AttackLenght, null, CurrentTile, null, false, _Attack.IsSpawnSkill);

                yield return new WaitForSeconds(.15f);
                bool canAttackIncomingAttacker = false;
                for (int i = 0; i <  _gameManager.IndexOccupiedTiles; i++)
                {
                    if (_gameManager.OccupiedTiles[i] != null)
                    {
                        if (_gameManager.OccupiedTiles[i].CharacterReference != null && _gameManager.OccupiedTiles[i].CharacterReference == _IncomingAttacker)
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
                { Debug.Log("Character :: Counter check for Is in front GO = " + gameObject.name);
                
                }
                ActionCounterAttack();
            }
            else
            {
                Debug.Log("Character :: CheckIfCanCounterAttack Set _gameManager.Wait(false) 000 :: Character = " + gameObject.name);
                if (_IncomingAttacker != null && _IncomingAttacker._NbrRepeatAttack == 0)
                {
                    _gameManager.Wait = false;
                }
                
                if (!IsAI && _gameManager.IsController && !HaveMoved)
                {
                    _gameManager.SelectCharacter?.Invoke();
                }
            }
        }
        else
        {
            Debug.Log("Character :: CheckIfCanCounterAttack Set _gameManager.Wait(false) 111 :: Character = " + gameObject.name);
            _gameManager.Wait = false;
            if (!IsAI && _gameManager.IsController && !HaveMoved)
            {
                _gameManager.SelectCharacter?.Invoke();
            }
        }
    }
    
    public void DestroyCharacter()
    {
        StartCoroutine(Vanish());
    }
    

    protected IEnumerator Vanish()
    {
        if(_IsDead || _DoOnceVanish ) {yield break;}

        _gameManager.Wait = true;

        if (_IncomingAttacker != null)
        {
            if (_IncomingAttacker.Class == DataCharacterSpawner.CharactersPrefab.SelfDestructRobotAI ||
                _IncomingAttacker.Class == DataCharacterSpawner.CharactersPrefab.Turret)
            {
                _IncomingAttacker = _IncomingAttacker._CharacterRelated;
            }
            for (int i = 0; i < 3; i++)
            {
                GameObject xpEarnGameObject = Instantiate(XpEarnPrefab, transform.position + new Vector3(0,1,0), Quaternion.identity);
                XpParticleEffect xpParticleEffect = xpEarnGameObject.GetComponent<XpParticleEffect>();
                _gameManager.StartCoroutine(xpParticleEffect.StartXpParticleEffect(_IncomingAttacker,  Level * 100));

            }
        }
        _DoOnceVanish = true;
        ActionShowHideHealthBar?.Invoke(false);
        ActionDestroyCharacterRelated?.Invoke();
        bool isCharacterTurn = _gameManager.CurrentCharacter == this;

      
        bool isBoss = _gameManager._IsRobotBoss && CharacterType == DataCharacterSpawner.CharactersPrefab.RobotAI || _gameManager._IsFatherNatureBoss && CharacterType == DataCharacterSpawner.CharactersPrefab.MotherNatureAI || _gameManager._IsWizardBoss && CharacterType == DataCharacterSpawner.CharactersPrefab.WizardAI;
        if (!isBoss)
        {
            _gameManager.RemoveCharacter(this);
            CharacterAnimator.SetTrigger(Die);
            yield return StartCoroutine(MoveTo(transform.position + Vector3.up * DeathGapZPosition, 0.5f));
            CurrentTile.UnSetCharacter();
        
            if (_DieFloorParticleEffect != null)
            {
                GameObject dieFloorParticleEffect = Instantiate(_DieFloorParticleEffect, CurrentTile.Position + new Vector3(0,0.4f,0), Quaternion.identity);
                dieFloorParticleEffect.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        
            StartCoroutine(LerpScale(Vector3.zero, _lerpScalingSpeed));
            yield return StartCoroutine(MoveTo(transform.position + transform.TransformDirection(Vector3.forward) * ForwardDistanceWhenDie, 0.25f));
            _gameManager.Wait = false;

            yield return MoveTo(transform.position + Vector3.down * DownDistanceWhenDie,DyingMoveSpeed);
        }
        else
        {
            Time.timeScale = 1f;
            _gameManager._GameIsFinish = true;
            CharacterAnimator.SetTrigger(Kneel);
            yield return new WaitForSeconds(3);
            ActionShowDialogueBubble(true);
            yield return new WaitForSeconds(1);
            ActionStartDialogue("plz spare me");
            yield return new WaitForSeconds(3);
            ActionStartDialogue("I will join you");
            yield return new WaitForSeconds(3);
            _gameManager.StartCoroutine(_gameManager.SetEndGame(true));
        }

        _gameManager.Wait = false;
        if (!IsAI && isCharacterTurn && _gameManager.CurrentCharacter == null || _gameManager.CurrentCharacterTurn == this)
        {
            Debug.Log("Character :: _gameManager.CurrentCharacter == this :: Character = " + gameObject.name);
            _gameManager.NextCharacterTurn();
        }

        ActionRemoveUIPopUpCharacterInfo(true);
        _IsDead = true;
        CurrentTile = null;
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

                if ( randomChange <= frontHitSuccesChance)
                {
                    return true;
                }
                
                break;
            case GetAttackDirection.AttackDirection.Behind:
                float behindHitSuccesChance = 100 - _behindHitSuccesChance;
                
                if (randomChange <= behindHitSuccesChance)
                {
                    return true;
                }
                
                break;
            default:
                float sideHitSuccesChance = 100 - _sideHitSuccesChance;
                
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
        ActionShowUIPopUpCharacterInfo(true, false);
        ActionShowUIHitSuccess(pct);
    }

    public void SetRemaininTimeTurn()
    {
        TurnTimeRemaining -= Speed;
    }

    private void SetPotionEffect(E_Potion potionType)
    {
        switch (potionType)
        {
            case E_Potion.HealthBoostSpeedPenalty:
            {
                int heathRecover = MaxHealth / 2;
                CurrentHealth += heathRecover;
                int debuffSpeed = Speed / 4;
                Speed -= debuffSpeed;

                if (CurrentHealth > MaxHealth)
                {
                    CurrentHealth = MaxHealth;
                }
            
                if (Speed < 0)
                {
                    Speed = 20;
                }

                string bufftext = "+50% HP";
                string debufftext = "-" + debuffSpeed + " Speed";
                ActionShowBuffDebuffPotionEffect?.Invoke(bufftext, debufftext);
                OnHealthPctChange(CurrentHealth, heathRecover, MaxHealth, false);
                break;
            }
            case E_Potion.SpeedBoostHealthPenalty:
            {
                int buffSpeed = Speed / 2;
                Speed += buffSpeed;
                int debuffHealth = MaxHealth / 4;
                CurrentHealth -= debuffHealth;

                if (CurrentHealth < 0)
                {
                    CurrentHealth = 1;
                }
                
                string bufftext = "+" + buffSpeed + " Speed";
                string debufftext = "-25% HP";
                ActionShowBuffDebuffPotionEffect?.Invoke(bufftext, debufftext); 
                OnHealthPctChange(CurrentHealth, debuffHealth, MaxHealth, true);
                break;
            }
            case E_Potion.MaxHPBoostHealthPenalty:
            {
                int maxHP = MaxHealth / 2;
                MaxHealth += maxHP;
                int debuffHealth = MaxHealth / 4;
                CurrentHealth -= debuffHealth;

                if (CurrentHealth < 0)
                {
                    CurrentHealth = 1;
                }
                
                string bufftext = "+" + maxHP + " MaxHP";
                string debufftext = "-25% HP";
                ActionShowBuffDebuffPotionEffect?.Invoke(bufftext, debufftext); 
                OnHealthPctChange(CurrentHealth, debuffHealth, MaxHealth, true);
                break;
            }
            case E_Potion.HealthBoostMaxHPPenalty:
            {
                int heathRecover = MaxHealth / 2;
                CurrentHealth += heathRecover;
                
                int debuffMaxHealth = MaxHealth / 4;
                MaxHealth -= debuffMaxHealth;

                if (CurrentHealth > MaxHealth)
                {
                    CurrentHealth = MaxHealth;
                }
                
                string bufftext = "+50% HP";
                string debufftext  = "-" + debuffMaxHealth + " MaxHP";
                ActionShowBuffDebuffPotionEffect?.Invoke(bufftext, debufftext); 
                OnHealthPctChange(CurrentHealth, heathRecover, MaxHealth, false);
                break;
            }
            case E_Potion.PowerBoostSpeedPenalty:
            {
                int buffStrenght = Strength / 2;
                Strength += buffStrenght;
                
                int debuffSpeed = Speed / 4;
                Speed -= debuffSpeed;
                
                
                string bufftext = "+ " + buffStrenght + " Strenght";
                string debufftext  = "-" + debuffSpeed + " Speed";
                ActionShowBuffDebuffPotionEffect?.Invoke(bufftext, debufftext);
                break;
            }
            case E_Potion.MoveBoostPowerPenalty:
            {
                MovementPoint++;
                int debuffPower = Strength / 4;
                Strength -= debuffPower;
                
                
                string bufftext = "+1 Move";
                string debufftext  = "-" + Strength + " debuffPower";
                ActionShowBuffDebuffPotionEffect?.Invoke(bufftext, debufftext);
                break;
            }
            case E_Potion.PowerBoostMovePenalty:
            {
                int buffStrenght = Strength / 2;
                Strength += buffStrenght;
                MovementPoint--;

                if (MovementPoint <= 0)
                {
                    MovementPoint = 1;
                }
                
                string bufftext = "+ " + buffStrenght + " Strenght";
                string debufftext  = "-1 Move";
                ActionShowBuffDebuffPotionEffect?.Invoke(bufftext, debufftext);
                break;
            }
        }
    }
    
    public void SetAbility( DataCharacterSpawner.CharacterAbility ability)
    {
        switch (ability)
        {
            case DataCharacterSpawner.CharacterAbility.None:
                break;
            case DataCharacterSpawner.CharacterAbility.CounterAttack:
                new CounterAbility(this, GameManager.Instance);
                break;
            case DataCharacterSpawner.CharacterAbility.ImmuneToFire:
                ImmuneToFire = true;
                break;
            case DataCharacterSpawner.CharacterAbility.ImmuneToPoison:
                ImmuneToPoison = true;
                break;
            case DataCharacterSpawner.CharacterAbility.InFire:
                InFire = true;
                _ElemenFireCharactertParticleEffect.SetActive(true);
                foreach (var elementSwordParticleEffect in _ElementSwordParticleEffect)
                {
                    elementSwordParticleEffect.gameObject.SetActive(true);
                    elementSwordParticleEffect.startColor = Color.red;
                }
                break;
            case DataCharacterSpawner.CharacterAbility.SpeedBoost:
                Speed *= 2;
                break;
        }
    }

    public bool GetInFire()
    {
        return InFire;
    }
}

public enum E_Potion
{
    HealthBoostSpeedPenalty = 0,  // Boost health, reduces speed
    SpeedBoostHealthPenalty = 1,   // Boost speed, reduces health
    MaxHPBoostHealthPenalty = 2,  // Boost Maxhealth, reduces Health
    HealthBoostMaxHPPenalty = 3,   // Boost health, reduces Max health
    PowerBoostSpeedPenalty = 4,   // Boost power, reduces Max health
    MoveBoostPowerPenalty = 5,   // Boost movement, reduces Power
    PowerBoostMovePenalty = 6   // Boost Power, reduces movement
}
