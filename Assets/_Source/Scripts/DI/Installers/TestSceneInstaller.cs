using UnityEngine;

public class TestSceneInstaller : SceneInstaller
{
    [SerializeField] private InventoryController _inventoryController;

    protected override void Install(IContainer container)
    {
        container.Bind<IPlayerInput>(_ => new LegacyPlayerInput()).AsSingle();
        container.BindInstance(_inventoryController).AsSingle();
    }
}