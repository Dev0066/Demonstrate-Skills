using GamePlay;
using Scoring;
using System.ComponentModel;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IPlayer>().To<Player>().AsSingle();
        Container.Bind<IEnemy>().To<Enemy>().AsSingle();
        Container.Bind<IScoreManager>().To<ScoreManager>().AsSingle();
        Container.Bind<ICoin>().To<Coin>().AsTransient();
    }
}
