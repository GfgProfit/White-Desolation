using UnityEngine;

public class TestSceneInstaller : SceneInstaller
{
    [SerializeField] private InventoryController _inventoryController;
    [SerializeField] private FireUIController _fireUIController;
    [SerializeField] private DayNightCycle _dayNightCycle;

    protected override void Install(IContainer container)
    {
        container.Bind<IPlayerInput>(_ => new LegacyPlayerInput()).AsSingle();

        container.BindInstance(_inventoryController).AsSingle();
        container.BindInstance(_dayNightCycle).AsSingle();

        container.Bind<IGameTimeConverter>(_ => _dayNightCycle).AsSingle();
        container.Bind<IGameTimeAdvancer>(_ => _dayNightCycle).AsSingle();
        container.Bind<IFireSourceInteractionHandler>(_ => _fireUIController).AsSingle();

        container.BindInstance(_fireUIController).AsSingle();
    }
}
