using UnityEngine;

public class TestSceneInstaller : SceneInstaller
{
    [SerializeField] private InventoryController _inventoryController;
    [SerializeField] private FireStartingUIController _fireStartingUI;

    protected override void Install(IContainer container)
    {
        container.Bind<IPlayerInput>(_ => new LegacyPlayerInput()).AsSingle();
        container.BindInstance(_inventoryController).AsSingle();
        container.BindInstance(_fireStartingUI).AsSingle();
    }
}