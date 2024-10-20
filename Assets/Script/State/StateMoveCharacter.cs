
public class StateMoveCharacter : State
{
    public MoveAIMapCharacter _WizardMoveAIMapCharacter { get;  set; }
    public MoveAIMapCharacter _RobotMoveAIMapCharacter{ get;  set; }
    public MoveAIMapCharacter _FatherNatureMoveAIMapCharacter{ get;  set; }
    private Tile _LastMoveTile;
    private Tile _SecondLastMoveTile;
    private Tile _ThirdLastMoveTile;
    private bool _WizardBossIsDead;
    private bool _RobotBossIsDead;
    private bool _FatherNatureBossIsDead;
    private bool _DoOnce;
    public StateMoveCharacter(GameManager gameManager)
    {
        _gameManager = gameManager;
        _tilesManager = TilesManager.Instance;
    }
    public override void SelectTile(Tile tile)
    {
        if(tile == _gameManager.TileSelected) return;
        if (tile.CanInteract)
        {
            if (_gameManager._IsMapScene && tile.CharacterReference != null)
            {
                _gameManager.StartCoroutine(_gameManager.MapSceneToBattleScene(tile.CharacterReference.gameObject.gameObject.name));
            }
            
            tile.CharacterReference =  GameManager.Instance.CurrentCharacter;
            _gameManager.TileSelected.UnSetCharacter();
            tile.SetCharacter(GameManager.Instance.CurrentCharacter);

            if (_gameManager._IsMapScene)
            {
                if (!_DoOnce)
                {
                    _DoOnce = true;
                    _WizardBossIsDead = FBPP.GetBool("WizardBossIsDead");
                    _RobotBossIsDead = FBPP.GetBool("RobotBossIsDead");
                    _FatherNatureBossIsDead = FBPP.GetBool("FatherNatureBossIsDead");
                }
                _ThirdLastMoveTile = _SecondLastMoveTile;
                _SecondLastMoveTile = _LastMoveTile;
                _LastMoveTile = _gameManager.CurrentCharacter.CurrentTile;
            
                if (_LastMoveTile != null)
                {
                    bool stop = false;
                    if (_WizardBossIsDead)
                    {
                        if (!_WizardMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _WizardMoveAIMapCharacter.SpawnFollowCharacter(_LastMoveTile, 1);
                    
                        }
                        else if(_WizardMoveAIMapCharacter._SpawnItteration == 1)
                        {
                            stop = true;
                            _WizardMoveAIMapCharacter.SetMoveCharacter(_LastMoveTile);
                        }
                    }
                
                    if (!stop && _RobotBossIsDead)
                    {
                        if (!_RobotMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _RobotMoveAIMapCharacter.SpawnFollowCharacter(_LastMoveTile, 1);
                    
                        }
                        else if(_RobotMoveAIMapCharacter._SpawnItteration == 1)
                        {
                            stop = true;
                            _RobotMoveAIMapCharacter.SetMoveCharacter(_LastMoveTile);
                        }
                    }
                
                    if (!stop && _FatherNatureBossIsDead)
                    {
                        if (!_FatherNatureMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _FatherNatureMoveAIMapCharacter.SpawnFollowCharacter(_LastMoveTile, 1);
                    
                        }
                        else if(_FatherNatureMoveAIMapCharacter._SpawnItteration == 1)
                        {
                            stop = true;
                            _FatherNatureMoveAIMapCharacter.SetMoveCharacter(_LastMoveTile);
                        }
                    }
                }
            
                if (_SecondLastMoveTile != null)
                {
                    bool stop = false;
                    if (_WizardBossIsDead)
                    {
                        if (!_WizardMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _WizardMoveAIMapCharacter.SpawnFollowCharacter(_SecondLastMoveTile, 2);
                    
                        }
                        else if(_WizardMoveAIMapCharacter._SpawnItteration == 2)
                        {
                            stop = true;
                            _WizardMoveAIMapCharacter.SetMoveCharacter(_SecondLastMoveTile);
                        }
                    }
                
                    if (!stop && _RobotBossIsDead)
                    {
                        if (!_RobotMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _RobotMoveAIMapCharacter.SpawnFollowCharacter(_SecondLastMoveTile, 2);
                    
                        }
                        else if(_RobotMoveAIMapCharacter._SpawnItteration == 2)
                        {
                            stop = true;
                            _RobotMoveAIMapCharacter.SetMoveCharacter(_SecondLastMoveTile);
                        }
                    }
                
                    if (!stop && _FatherNatureBossIsDead)
                    {
                        if (!_FatherNatureMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _FatherNatureMoveAIMapCharacter.SpawnFollowCharacter(_SecondLastMoveTile, 2);
                    
                        }
                        else if(_FatherNatureMoveAIMapCharacter._SpawnItteration == 2)
                        {
                            stop = true;
                            _FatherNatureMoveAIMapCharacter.SetMoveCharacter(_SecondLastMoveTile);
                        }
                    }
                }
            
                if (_ThirdLastMoveTile != null)
                {
                    bool stop = false;
                    if (_WizardBossIsDead)
                    {
                        if (!_WizardMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _WizardMoveAIMapCharacter.SpawnFollowCharacter(_ThirdLastMoveTile, 3);
                    
                        }
                        else if(_WizardMoveAIMapCharacter._SpawnItteration == 3)
                        {
                            stop = true;
                            _WizardMoveAIMapCharacter.SetMoveCharacter(_ThirdLastMoveTile);
                        }
                    }
                
                    if (!stop && _RobotBossIsDead)
                    {
                        if (!_RobotMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _RobotMoveAIMapCharacter.SpawnFollowCharacter(_ThirdLastMoveTile, 3);
                    
                        }
                        else if(_RobotMoveAIMapCharacter._SpawnItteration == 3)
                        {
                            stop = true;
                            _RobotMoveAIMapCharacter.SetMoveCharacter(_ThirdLastMoveTile);
                        }
                    }
                
                    if (!stop && _FatherNatureBossIsDead)
                    {
                        if (!_FatherNatureMoveAIMapCharacter._IsSpawn)
                        {
                            stop = true;
                            _FatherNatureMoveAIMapCharacter.SpawnFollowCharacter(_ThirdLastMoveTile, 3);
                    
                        }
                        else if(_FatherNatureMoveAIMapCharacter._SpawnItteration == 3)
                        {
                            stop = true;
                            _FatherNatureMoveAIMapCharacter.SetMoveCharacter(_ThirdLastMoveTile);
                        }
                    }
                }
            
            }
            _gameManager.CurrentCharacter.SetMovementCharacter(tile);
            if (_gameManager._IsMapScene)
            {
                tile.MapTilesManager.AddSelectedTile(tile);
            }
            else
            {
                _tilesManager.AddSelectedTile(tile);
            }

            if (_gameManager._IsMapScene && _gameManager.IsController)
            {
                
            }
            else
            {
                tile.SetTopMaterial(_tilesManager.MoveTileMaterial); 
            }
            
            _gameManager.CurrentState =  _gameManager.StateNavigation;
            _gameManager.NeedResetTiles = false;
            _gameManager.TileSelected = tile;
            _gameManager.CurrentCharacter = tile.CharacterReference;
            if (_gameManager.DesableMoveCharacterUIButtons != null)
            {
                _gameManager.DesableMoveCharacterUIButtons();
            }
        }
    }
}
