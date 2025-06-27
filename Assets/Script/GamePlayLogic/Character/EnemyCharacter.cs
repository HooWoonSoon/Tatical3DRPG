
public class EnemyCharacter : CharacterBase
{
    public StateMachine stateMechine;

    private void Awake()
    {
        stateMechine = new StateMachine();
        
    }
    protected override void Start()
    {
        base.Start();
    }
}

