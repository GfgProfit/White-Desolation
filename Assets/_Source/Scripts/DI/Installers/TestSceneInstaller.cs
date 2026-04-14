public class TestSceneInstaller : SceneInstaller
{
    protected override void Install(IContainer container)
    {
        container.Bind<IPlayerInput>(_ => new LegacyPlayerInput()).AsSingle();
    }
}